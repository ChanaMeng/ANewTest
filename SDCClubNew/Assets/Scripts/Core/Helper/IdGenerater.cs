using System;

namespace SDClub.Core
{
    public class IdGenerater : Singleton<IdGenerater>, ISingletonAwake
    {
        private long instanceIdCounter;
        private long idCounter;

        public void Awake()
        {
            instanceIdCounter = 0;
            idCounter = 0;
        }

        public long GenerateInstanceId()
        {
            return System.Threading.Interlocked.Increment(ref instanceIdCounter);
        }

        public long GenerateId()
        {
            return System.Threading.Interlocked.Increment(ref idCounter);
        }
    }
}
