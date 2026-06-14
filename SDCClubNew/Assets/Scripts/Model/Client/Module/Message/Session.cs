using System;
using SDClub.Core;

namespace SDClub.Model
{
    public class Session : Entity, IAwake<AService, long>
    {
        public AService AService { get; set; }
        public long ChannelId { get; set; }
        public long LastRecvTime { get; set; }
        public long LastSendTime { get; set; }
        
        public void Send(IMessage message)
        {
            // 序列化消息并发送
            var opcode = OpcodeType.Instance.GetOpcode(message.GetType());
            var bytes = SerializeHelper.Serialize(message);
            // 构造: [opcode(2字节)][payload]
            var sendBuffer = new byte[bytes.Length + 2];
            BitConverter.GetBytes(opcode).CopyTo(sendBuffer, 0);
            bytes.CopyTo(sendBuffer, 2);
            AService.Send(ChannelId, sendBuffer);
        }
    }
}
