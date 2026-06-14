namespace SDClub.Model
{
    // Server response codes
    public enum ErrorCode
    {
        Success = 0,
        UnknownError = 1,
        TokenInvalid = 2,
        AlreadyLoggedIn = 3,
        ServerFull = 4,
    }

    [Message(Opcode.C2S_Login)]
    public class C2S_Login : IRequest
    {
        public int RpcId { get; set; }
        public string Token { get; set; }
        public string DeviceId { get; set; }
    }

    [Message(Opcode.S2C_Login)]
    public class S2C_Login : IResponse
    {
        public int RpcId { get; set; }
        public int Error { get; set; }
        public string Message { get; set; }
        public long PlayerId { get; set; }
        public string Nickname { get; set; }
        public int Level { get; set; }
    }

    [Message(Opcode.C2S_Heartbeat)]
    public class C2S_Heartbeat : IMessage
    {
        public long Timestamp { get; set; }
    }

    [Message(Opcode.S2C_Heartbeat)]
    public class S2C_Heartbeat : IMessage
    {
        public long ServerTime { get; set; }
    }

    [Message(Opcode.S2C_Kick)]
    public class S2C_Kick : IMessage
    {
        public int Reason { get; set; }
    }
}
