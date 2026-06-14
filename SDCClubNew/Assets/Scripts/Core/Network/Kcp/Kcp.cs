using System;
using System.Collections.Generic;

namespace SDClub.Core
{
    public class Kcp
    {
        public const int IKCP_RTO_NDL = 30;
        public const int IKCP_RTO_MIN = 100;
        public const int IKCP_RTO_DEF = 200;
        public const int IKCP_RTO_MAX = 60000;
        public const int IKCP_CMD_PUSH = 81;
        public const int IKCP_CMD_ACK = 82;
        public const int IKCP_CMD_WASK = 83;
        public const int IKCP_CMD_WINS = 84;
        public const int IKCP_ASK_SEND = 1;
        public const int IKCP_ASK_TELL = 2;
        public const int IKCP_WND_SND = 32;
        public const int IKCP_WND_RCV = 128;
        public const int IKCP_MTU_DEF = 1400;
        public const int IKCP_ACK_FAST = 3;
        public const int IKCP_INTERVAL = 100;
        public const int IKCP_OVERHEAD = 24;
        public const int IKCP_DEADLINK = 20;
        public const int IKCP_THRESH_INIT = 2;
        public const int IKCP_THRESH_MIN = 2;
        public const int IKCP_PROBE_INIT = 7000;
        public const int IKCP_PROBE_LIMIT = 120000;

        private readonly uint conv;
        private uint mtu = IKCP_MTU_DEF;
        private uint mss = IKCP_MTU_DEF - IKCP_OVERHEAD;
        private uint state;
        private uint snd_una;
        private uint snd_nxt;
        private uint rcv_nxt;
        private uint ts_recent;
        private uint ts_lastack;
        private uint ssthresh;
        private int rx_rttval;
        private int rx_srtt;
        private int rx_rto = IKCP_RTO_DEF;
        private int rx_minrto = IKCP_RTO_MIN;
        private uint snd_wnd = IKCP_WND_SND;
        private uint rcv_wnd = IKCP_WND_RCV;
        private uint rmt_wnd = IKCP_WND_RCV;
        private uint cwnd;
        private uint probe;
        private uint current;
        private uint interval = IKCP_INTERVAL;
        private uint ts_flush = IKCP_INTERVAL;
        private uint xmit;
        private uint nrcv_buf;
        private uint nsnd_buf;
        private uint nrcv_que;
        private uint nsnd_que;
        private uint nodelay;
        private uint updated;
        private uint ts_probe;
        private uint probe_wait;
        private uint dead_link = IKCP_DEADLINK;
        private int incr;

        private readonly List<Segment> snd_queue = new List<Segment>();
        private readonly List<Segment> rcv_queue = new List<Segment>();
        private readonly List<Segment> snd_buf = new List<Segment>();
        private readonly List<Segment> rcv_buf = new List<Segment>();
        private readonly List<uint> acklist = new List<uint>();
        private byte[] buffer;
        private readonly Action<byte[], int> output;
        private int fastresend;
        private int fastlimit;
        private int nocwnd;
        private int stream;

        private class Segment
        {
            public uint conv;
            public uint cmd;
            public uint frg;
            public uint wnd;
            public uint ts;
            public uint sn;
            public uint una;
            public uint resendts;
            public uint rto;
            public uint fastack;
            public uint xmit;
            public byte[] data;
            public int len;
        }

        public Kcp(uint conv, Action<byte[], int> output)
        {
            this.conv = conv;
            this.output = output;
            this.buffer = new byte[(mtu + IKCP_OVERHEAD) * 3];
        }

        public void SetNoDelay(int nodelay, int interval, int resend, int nc)
        {
            if (nodelay > 0)
            {
                this.nodelay = (uint)nodelay;
                if (nodelay != 0)
                {
                    this.rx_minrto = IKCP_RTO_NDL;
                }
                else
                {
                    this.rx_minrto = IKCP_RTO_MIN;
                }
            }
            if (interval >= 0)
            {
                if (interval > 5000) interval = 5000;
                else if (interval < 10) interval = 10;
                this.interval = (uint)interval;
            }
            if (resend >= 0)
            {
                this.fastresend = resend;
            }
            if (nc >= 0)
            {
                this.nocwnd = nc;
            }
        }

        public void SetWindowSize(int sndwnd, int rcvwnd)
        {
            if (sndwnd > 0) this.snd_wnd = (uint)sndwnd;
            if (rcvwnd > 0)
            {
                this.rcv_wnd = (uint)Math.Max(rcvwnd, IKCP_WND_RCV);
            }
        }

        public void SetMtu(int mtu)
        {
            if (mtu < 50) mtu = 50;
            this.mtu = (uint)mtu;
            this.mss = (uint)mtu - IKCP_OVERHEAD;
            this.buffer = new byte[(mtu + IKCP_OVERHEAD) * 3];
        }

        public void SetMinrto(int minrto)
        {
            this.rx_minrto = minrto;
        }

        public int WaitSendCount
        {
            get { return snd_buf.Count + snd_queue.Count; }
        }

        private uint GetInterval()
        {
            return interval;
        }

        private void InsertSegmentInQueue(List<Segment> queue, Segment seg)
        {
            int idx = 0;
            while (idx < queue.Count)
            {
                if (seg.sn < queue[idx].sn) break;
                if (seg.sn == queue[idx].sn) return;
                idx++;
            }
            queue.Insert(idx, seg);
        }

        public int PeekSize()
        {
            if (rcv_queue.Count == 0) return -1;
            Segment seg = rcv_queue[0];
            if (seg.frg == 0) return seg.len;
            if (rcv_queue.Count < (int)seg.frg + 1) return -1;

            int length = 0;
            for (int i = 0; i <= (int)seg.frg; i++)
            {
                if (i >= rcv_queue.Count) break;
                length += rcv_queue[i].len;
            }
            return length;
        }

        public int Receive(Span<byte> buffer)
        {
            if (rcv_queue.Count == 0) return -1;

            int peekSize = PeekSize();
            if (peekSize < 0) return -1;

            int recover = 0;
            if (rcv_queue.Count >= (int)rcv_wnd) recover = 1;

            Segment seg = rcv_queue[0];
            int fragCount = (int)seg.frg + 1;
            int totalLen = 0;
            int copyPos = 0;

            for (int i = 0; i < fragCount; i++)
            {
                seg = rcv_queue[0];
                rcv_queue.RemoveAt(0);
                if (seg.data != null)
                {
                    Array.Copy(seg.data, 0, buffer.ToArray(), copyPos, seg.len);
                    copyPos += seg.len;
                    totalLen += seg.len;
                }
            }

            if (rcv_queue.Count < (int)rcv_wnd && recover != 0)
            {
                Probe |= IKCP_ASK_TELL;
            }

            return totalLen;
        }

        public int Send(Span<byte> buffer)
        {
            if (buffer.Length == 0) return -1;

            int count = buffer.Length <= (int)mss ? 1 : (int)((uint)buffer.Length / mss) + 1;

            for (int i = 0; i < count; i++)
            {
                int size = buffer.Length > (i + 1) * (int)mss ? (int)mss : buffer.Length - i * (int)mss;
                Segment seg = new Segment();
                seg.data = new byte[size];
                Array.Copy(buffer.ToArray(), i * (int)mss, seg.data, 0, size);
                seg.len = size;
                seg.frg = this.stream != 0 ? 0 : (uint)(count - i - 1);
                snd_queue.Add(seg);
            }

            return 0;
        }

        public void Update(uint current, byte[] buffer)
        {
            this.current = current;
            Flush(buffer);
        }

        public uint Check(uint current)
        {
            uint ts_flush_temp = ts_flush;
            uint tm_packet = 0x7fffffff;
            uint minimal = 0;

            if (updated == 0)
            {
                return current;
            }

            if (current - ts_flush >= 10000 || current < ts_flush)
            {
                ts_flush = current;
                ts_flush_temp = current;
            }

            if (snd_buf.Count != 0)
            {
                Segment seg = snd_buf[0];
                ts_flush_temp = Math.Min(ts_flush_temp, seg.resendts);
            }

            if (rcv_buf.Count != 0)
            {
                ts_flush_temp = Math.Min(ts_flush_temp, rcv_buf[0].resendts);
            }

            return ts_flush_temp;
        }

        public int Input(Span<byte> data)
        {
            uint old_una = this.snd_una;
            uint inputedCount = 0;

            if (data.Length < IKCP_OVERHEAD) return -1;

            while (true)
            {
                if (data.Length < IKCP_OVERHEAD) break;

                uint conv = ReadUInt32(data, 0);
                byte cmd = data[4];
                byte frg = data[5];
                ushort wnd = ReadUInt16(data, 6);
                uint ts = ReadUInt32(data, 8);
                uint sn = ReadUInt32(data, 12);
                uint una = ReadUInt32(data, 16);
                int len = ReadInt32(data, 20);

                if (data.Length < IKCP_OVERHEAD + len) break;

                if (cmd != IKCP_CMD_PUSH && cmd != IKCP_CMD_ACK &&
                    cmd != IKCP_CMD_WASK && cmd != IKCP_CMD_WINS)
                {
                    return -2;
                }

                this.rmt_wnd = wnd;
                this.ParseUna(una);
                this.ShrinkBuf();

                if (cmd == IKCP_CMD_ACK)
                {
                    if (current - ts >= 0)
                    {
                        this.UpdateAck((int)(current - ts));
                    }
                    this.ParseAck(sn);
                    this.ShrinkBuf();
                }
                else if (cmd == IKCP_CMD_PUSH)
                {
                    if (sn < rcv_nxt + rcv_wnd)
                    {
                        this.AckPush(sn, ts);
                        if (sn >= rcv_nxt)
                        {
                            Segment seg = new Segment();
                            seg.conv = conv;
                            seg.cmd = cmd;
                            seg.frg = frg;
                            seg.wnd = wnd;
                            seg.ts = ts;
                            seg.sn = sn;
                            seg.una = una;
                            seg.data = new byte[len];
                            Array.Copy(data.ToArray(), IKCP_OVERHEAD, seg.data, 0, len);
                            seg.len = len;
                            this.ParseData(seg);
                        }
                    }
                }
                else if (cmd == IKCP_CMD_WASK)
                {
                    this.Probe |= IKCP_ASK_TELL;
                }
                else if (cmd == IKCP_CMD_WINS)
                {
                    // Do nothing
                }

                data = data.Slice(IKCP_OVERHEAD + len);
                inputedCount++;
            }

            if (old_una != this.snd_una)
            {
                if (this.cwnd < this.rmt_wnd)
                {
                    uint mss = this.mss;
                    if (this.cwnd < this.ssthresh)
                    {
                        this.cwnd++;
                        this.incr += (int)mss;
                    }
                    else
                    {
                        if (this.incr < (int)mss) this.incr = (int)mss;
                        this.incr += (int)(mss * mss / this.incr + mss / 16);
                        if ((uint)(this.incr / (int)mss) > 0)
                        {
                            this.cwnd += (uint)(this.incr / (int)mss);
                            this.incr %= (int)mss;
                        }
                    }
                    if (this.cwnd > this.rmt_wnd)
                    {
                        this.cwnd = this.rmt_wnd;
                        this.incr = 0;
                    }
                }
            }

            return 0;
        }

        private void Flush(byte[] buffer)
        {
            uint current = this.current;

            if (this.updated == 0) return;

            // Check for lost packets
            for (int i = 0; i < snd_buf.Count; i++)
            {
                Segment seg = snd_buf[i];
                if (seg.xmit == 0) continue;
                if (current - seg.resendts >= 0)
                {
                    seg.resendts = current + seg.rto;
                    seg.rto += Math.Max(seg.rto, (uint)rx_rto);
                    seg.xmit++;
                    if (nocwnd == 0)
                    {
                        ssthresh = (uint)(cwnd / 2);
                        if (ssthresh < 2) ssthresh = 2;
                        cwnd = 1;
                        incr = 0;
                    }
                    if (seg.xmit >= dead_link) state = unchecked((uint)(-1));
                    if (fastresend > 0 && seg.fastack >= fastresend)
                    {
                        seg.fastack = 0;
                        seg.rto = (uint)rx_rto;
                        seg.resendts = current + seg.rto;
                    }
                }
            }

            // Flush ACKs
            int count = acklist.Count;
            int offset = 0;
            for (int i = 0; i < count; i++)
            {
                int size = (count - i) * IKCP_OVERHEAD;
                if (size > mtu) size = (int)mtu;
                int nums = size / IKCP_OVERHEAD;

                WriteUInt32(buffer, offset + 0, conv);
                WriteUInt32(buffer, offset + 8, ts_flush);
                WriteUInt32(buffer, offset + 12, ts_flush);
                WriteUInt32(buffer, offset + 16, snd_una);
                buffer[offset + 4] = IKCP_CMD_ACK;
                buffer[offset + 6] = (byte)(rcv_wnd >> 8);
                buffer[offset + 7] = (byte)rcv_wnd;

                int segCount = Math.Min(nums, acklist.Count - i);
                for (int j = 0; j < segCount; j++)
                {
                    uint sn = acklist[i + j];
                    WriteUInt32(buffer, offset + IKCP_OVERHEAD * (j + 1), sn);
                }

                output(buffer, IKCP_OVERHEAD * (segCount + 1));

                i += segCount - 1;
                offset += (int)mtu;
            }
            acklist.Clear();

            // Probe window size
            if (this.rmt_wnd == 0)
            {
                if (this.probe_wait == 0)
                {
                    this.probe_wait = IKCP_PROBE_INIT;
                    this.ts_probe = current + this.probe_wait;
                }
                else
                {
                    if (current - this.ts_probe >= 0)
                    {
                        if (this.probe_wait < IKCP_PROBE_INIT)
                            this.probe_wait = IKCP_PROBE_INIT;
                        this.probe_wait += this.probe_wait / 2;
                        if (this.probe_wait > IKCP_PROBE_LIMIT)
                            this.probe_wait = IKCP_PROBE_LIMIT;
                        this.ts_probe = current + this.probe_wait;
                        this.Probe |= IKCP_ASK_SEND;
                    }
                }
            }
            else
            {
                this.ts_probe = 0;
                this.probe_wait = 0;
            }

            // Flush window probing
            if ((this.Probe & IKCP_ASK_SEND) != 0)
            {
                WriteUInt32(buffer, 0, conv);
                buffer[4] = IKCP_CMD_WASK;
                WriteUInt32(buffer, 16, snd_una);
                output(buffer, IKCP_OVERHEAD);
            }

            if ((this.Probe & IKCP_ASK_TELL) != 0)
            {
                WriteUInt32(buffer, 0, conv);
                buffer[4] = IKCP_CMD_WINS;
                WriteUInt32(buffer, 16, snd_una);
                output(buffer, IKCP_OVERHEAD);
            }

            this.Probe = 0;

            // Flush pending data
            uint change = 0;
            uint lost = 0;
            offset = 0;

            for (int i = 0; i < snd_buf.Count; i++)
            {
                Segment seg = snd_buf[i];
                if (seg.xmit == 0)
                {
                    seg.resendts = current + seg.rto;
                    seg.xmit++;
                    seg.fastack = 0;
                }
            }

            uint cwnd_temp = Math.Min(snd_wnd, rmt_wnd);
            if (nocwnd == 0) cwnd_temp = Math.Min(cwnd, cwnd_temp);

            Queue<Segment> sendSegs = new Queue<Segment>();

            while (snd_una + cwnd_temp > snd_nxt)
            {
                if (snd_queue.Count == 0) break;

                Segment newSeg = snd_queue[0];
                snd_queue.RemoveAt(0);

                if (newSeg.sn == 0)
                {
                    newSeg.sn = snd_nxt;
                    newSeg.conv = conv;
                    newSeg.resendts = current;
                    newSeg.rto = (uint)rx_rto;
                    snd_nxt++;
                }

                snd_buf.Add(newSeg);
                sendSegs.Enqueue(newSeg);
            }

            foreach (Segment seg in sendSegs)
            {
                if (offset + seg.len + IKCP_OVERHEAD > mtu)
                {
                    output(buffer, offset);
                    offset = 0;
                }

                WriteUInt32(buffer, offset + 0, seg.conv);
                buffer[offset + 4] = IKCP_CMD_PUSH;
                buffer[offset + 5] = (byte)seg.frg;
                WriteUInt16(buffer, offset + 6, (ushort)rcv_wnd);
                WriteUInt32(buffer, offset + 8, seg.ts);
                WriteUInt32(buffer, offset + 12, seg.sn);
                WriteUInt32(buffer, offset + 16, snd_una);
                WriteInt32(buffer, offset + 20, seg.len);
                Array.Copy(seg.data, 0, buffer, offset + IKCP_OVERHEAD, seg.len);
                offset += IKCP_OVERHEAD + seg.len;
            }

            if (offset > 0)
            {
                output(buffer, offset);
            }

            this.ts_flush = current + this.interval;
        }

        private void ParseUna(uint una)
        {
            int count = 0;
            while (snd_buf.Count > 0)
            {
                Segment seg = snd_buf[0];
                if (una < seg.sn) break;
                snd_buf.RemoveAt(0);
                count++;
            }
            if (count > 0) snd_una = una;
        }

        private void ShrinkBuf()
        {
            if (snd_buf.Count > 0)
            {
                snd_una = snd_buf[0].sn;
            }
            else
            {
                snd_una = snd_nxt;
            }
        }

        private void UpdateAck(int rtt)
        {
            if (rx_srtt == 0)
            {
                rx_srtt = rtt;
                rx_rttval = rtt / 2;
            }
            else
            {
                int delta = rtt - rx_srtt;
                if (delta < 0) delta = -delta;
                rx_rttval = (3 * rx_rttval + delta) / 4;
                rx_srtt = (7 * rx_srtt + rtt) / 8;
                if (rx_srtt < 1) rx_srtt = 1;
            }
            int rto = rx_srtt + Math.Max((int)interval, 4 * rx_rttval);
            rx_rto = Math.Max(rx_minrto, Math.Min(rto, IKCP_RTO_MAX));
        }

        private void ParseAck(uint sn)
        {
            if (snd_buf.Count == 0) return;

            for (int i = 0; i < snd_buf.Count; i++)
            {
                Segment seg = snd_buf[i];
                if (sn == seg.sn)
                {
                    snd_buf.RemoveAt(i);
                    break;
                }
                if (sn < seg.sn) break;
            }
        }

        private void AckPush(uint sn, uint ts)
        {
            acklist.Add(sn);
            acklist.Add(ts);
        }

        private void ParseData(Segment newSeg)
        {
            uint sn = newSeg.sn;

            if (sn >= rcv_nxt + rcv_wnd) return;

            bool repeat = false;
            int insertIdx = 0;

            for (int i = rcv_buf.Count - 1; i >= 0; i--)
            {
                Segment seg = rcv_buf[i];
                if (sn == seg.sn)
                {
                    repeat = true;
                    break;
                }
                if (sn > seg.sn)
                {
                    insertIdx = i + 1;
                    break;
                }
            }

            if (!repeat)
            {
                rcv_buf.Insert(insertIdx, newSeg);
                nrcv_buf++;
            }

            // Move data from rcv_buf to rcv_queue
            while (rcv_buf.Count > 0)
            {
                Segment seg = rcv_buf[0];
                if (seg.sn == rcv_nxt && rcv_queue.Count < (int)rcv_wnd)
                {
                    rcv_buf.RemoveAt(0);
                    nrcv_buf--;
                    rcv_queue.Add(seg);
                    rcv_nxt++;
                }
                else
                {
                    break;
                }
            }
        }

        public uint Probe
        {
            get { return probe; }
            set { probe = value; }
        }

        private static uint ReadUInt32(Span<byte> data, int offset)
        {
            return (uint)(data[offset] | (data[offset + 1] << 8) |
                (data[offset + 2] << 16) | (data[offset + 3] << 24));
        }

        private static ushort ReadUInt16(Span<byte> data, int offset)
        {
            return (ushort)(data[offset] | (data[offset + 1] << 8));
        }

        private static int ReadInt32(Span<byte> data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) |
                (data[offset + 2] << 16) | (data[offset + 3] << 24);
        }

        private static void WriteUInt32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)value;
            data[offset + 1] = (byte)(value >> 8);
            data[offset + 2] = (byte)(value >> 16);
            data[offset + 3] = (byte)(value >> 24);
        }

        private static void WriteUInt16(byte[] data, int offset, ushort value)
        {
            data[offset] = (byte)value;
            data[offset + 1] = (byte)(value >> 8);
        }

        private static void WriteInt32(byte[] data, int offset, int value)
        {
            data[offset] = (byte)value;
            data[offset + 1] = (byte)(value >> 8);
            data[offset + 2] = (byte)(value >> 16);
            data[offset + 3] = (byte)(value >> 24);
        }
    }
}
