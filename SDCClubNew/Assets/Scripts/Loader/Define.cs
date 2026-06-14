namespace SDClub.Loader
{
    public static class Define
    {
        public static bool IsEditor
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

#if UNITY_EDITOR
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif

#if ENABLE_IL2CPP
        public const bool IsIL2CPP = true;
#else
        public const bool IsIL2CPP = false;
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        public const bool IsWebGL = true;
#else
        public const bool IsWebGL = false;
#endif

        /// <summary>
        /// 是否为微信小游戏平台（使用 WEIXINMINIGAME 宏精确检测）
        /// </summary>
#if WEIXINMINIGAME && !UNITY_EDITOR
        public const bool IsWeChat = true;
#else
        public const bool IsWeChat = false;
#endif

        /// <summary>
        /// 是否为 WebGL 平台（含微信小游戏）
        /// </summary>
        public static bool IsWeChatWebGL
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }
    }
}
