namespace SDClub.Model
{
    [Message(Opcode.C2S_EnterRoom)]
    public class C2S_EnterRoom : IRequest
    {
        public int RpcId { get; set; }
        public long RoomId { get; set; }
    }

    [Message(Opcode.S2C_EnterRoom)]
    public class S2C_EnterRoom : IResponse
    {
        public int RpcId { get; set; }
        public int Error { get; set; }
        public string Message { get; set; }
        public long RoomId { get; set; }
        public long PlayerId { get; set; }
    }

    [Message(Opcode.C2S_Move)]
    public class C2S_Move : IMessage
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float RotationY { get; set; }
    }

    [Message(Opcode.S2C_Move)]
    public class S2C_Move : IMessage
    {
        public long PlayerId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float RotationY { get; set; }
    }

    [Message(Opcode.S2C_PlayerJoin)]
    public class S2C_PlayerJoin : IMessage
    {
        public long PlayerId { get; set; }
        public string Nickname { get; set; }
        public int Level { get; set; }
    }

    [Message(Opcode.S2C_PlayerLeave)]
    public class S2C_PlayerLeave : IMessage
    {
        public long PlayerId { get; set; }
    }
}
