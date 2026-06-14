namespace SDClub.Model
{
    public static class Opcode
    {
        // Range definitions (per module)
        public const ushort System_Start = 0;
        public const ushort System_End = 99;

        // System messages
        public const ushort C2S_Login = 1;
        public const ushort S2C_Login = 2;
        public const ushort C2S_Heartbeat = 3;
        public const ushort S2C_Heartbeat = 4;
        public const ushort S2C_Kick = 5;
        public const ushort C2S_Reconnect = 6;
        public const ushort S2C_Reconnect = 7;

        public const ushort Game_Start = 100;
        public const ushort Game_End = 199;

        // Game messages
        public const ushort C2S_EnterRoom = 100;
        public const ushort S2C_EnterRoom = 101;
        public const ushort C2S_Move = 102;
        public const ushort S2C_Move = 103;
        public const ushort S2C_PlayerJoin = 104;
        public const ushort S2C_PlayerLeave = 105;

        public const ushort Chat_Start = 200;
        public const ushort Chat_End = 299;

        public const ushort C2S_Chat = 200;
        public const ushort S2C_Chat = 201;
    }
}
