using System.Collections.Generic;

namespace SDClub.Core
{
    public class MessageInfo
    {
        public long ChannelId { get; set; }
        public ushort Opcode { get; set; }
        public byte[] Data { get; set; }
    }

    public class MessageQueue
    {
        private readonly Queue<MessageInfo> queue = new Queue<MessageInfo>();
        private readonly object lockObj = new object();

        public int Count
        {
            get
            {
                lock (this.lockObj)
                {
                    return this.queue.Count;
                }
            }
        }

        public void Enqueue(long channelId, ushort opcode, byte[] data)
        {
            lock (this.lockObj)
            {
                this.queue.Enqueue(new MessageInfo
                {
                    ChannelId = channelId,
                    Opcode = opcode,
                    Data = data
                });
            }
        }

        public bool TryDequeue(out MessageInfo message)
        {
            lock (this.lockObj)
            {
                if (this.queue.Count > 0)
                {
                    message = this.queue.Dequeue();
                    return true;
                }
                message = null;
                return false;
            }
        }

        public void Clear()
        {
            lock (this.lockObj)
            {
                this.queue.Clear();
            }
        }
    }
}
