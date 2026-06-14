using System;
using System.Collections.Generic;
using System.IO;

namespace SDClub.Core
{
    public class ProcessInnerSender
    {
        private readonly long serviceId;
        private readonly HashSet<long> channels = new HashSet<long>();

        public ProcessInnerSender(long serviceId)
        {
            this.serviceId = serviceId;
        }

        public void AddChannel(long channelId)
        {
            this.channels.Add(channelId);
        }

        public void RemoveChannel(long channelId)
        {
            this.channels.Remove(channelId);
        }

        public void Send(long channelId, ushort opcode, object message)
        {
            byte[] data = SerializeMessage(opcode, message);
            NetServices.Instance.Send(this.serviceId, channelId, data);
        }

        public void Broadcast(ushort opcode, object message)
        {
            byte[] data = SerializeMessage(opcode, message);
            foreach (long channelId in this.channels)
            {
                NetServices.Instance.Send(this.serviceId, channelId, data);
            }
        }

        /// <summary>
        /// Simplified serialization: [opcode(2 bytes)][payload]
        /// For the wire format: [4-byte length][opcode(2 bytes)][payload]
        /// The length prefix is added by the channel layer (TChannel/KChannel/WChannel)
        /// </summary>
        private byte[] SerializeMessage(ushort opcode, object message)
        {
            byte[] opcodeBytes = BitConverter.GetBytes(opcode);

            // For primitive types or simple messages, serialize using custom logic
            if (message is byte[] rawData)
            {
                byte[] result = new byte[opcodeBytes.Length + rawData.Length];
                Array.Copy(opcodeBytes, 0, result, 0, opcodeBytes.Length);
                Array.Copy(rawData, 0, result, opcodeBytes.Length, rawData.Length);
                return result;
            }

            if (message is string str)
            {
                byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
                byte[] result = new byte[opcodeBytes.Length + strBytes.Length];
                Array.Copy(opcodeBytes, 0, result, 0, opcodeBytes.Length);
                Array.Copy(strBytes, 0, result, opcodeBytes.Length, strBytes.Length);
                return result;
            }

            // For MemoryStream messages
            if (message is MemoryStream ms)
            {
                byte[] msData = ms.ToArray();
                byte[] result = new byte[opcodeBytes.Length + msData.Length];
                Array.Copy(opcodeBytes, 0, result, 0, opcodeBytes.Length);
                Array.Copy(msData, 0, result, opcodeBytes.Length, msData.Length);
                return result;
            }

            // Default: just send opcode with no payload
            return opcodeBytes;
        }

        public static (ushort opcode, byte[] payload) DeserializeMessage(byte[] data)
        {
            if (data.Length < 2)
            {
                throw new Exception("Message too short, needs at least 2 bytes for opcode");
            }

            ushort opcode = BitConverter.ToUInt16(data, 0);
            int payloadLength = data.Length - 2;
            byte[] payload = new byte[payloadLength];
            Array.Copy(data, 2, payload, 0, payloadLength);

            return (opcode, payload);
        }
    }
}
