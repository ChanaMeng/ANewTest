using System;

namespace SDClub.Core
{
    [Flags]
    public enum SceneType : long
    {
        None = 0,
        Main = 1,
        BenchmarkClient = 1 << 11,
        BenchmarkServer = 1 << 12,
        LockStepClient = 1 << 16,
        LockStep = 1L << 32,
        NetClient = 1L << 35,
        All = long.MaxValue,
    }

    public static class SceneTypeHelper
    {
        public static bool HasSameFlag(this SceneType a, SceneType b)
        {
            if (((ulong)a & (ulong)b) == 0)
            {
                return false;
            }
            return true;
        }
    }
}
