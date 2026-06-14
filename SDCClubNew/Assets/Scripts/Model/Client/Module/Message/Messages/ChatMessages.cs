namespace SDClub.Model
{
    [Message(Opcode.C2S_Chat)]
    public class C2S_Chat : IMessage
    {
        public string Content { get; set; }
        public int Channel { get; set; }
    }

    [Message(Opcode.S2C_Chat)]
    public class S2C_Chat : IMessage
    {
        public long SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public int Channel { get; set; }
    }
}
