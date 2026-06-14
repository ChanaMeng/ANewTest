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
    }
}
