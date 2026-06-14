using System;

namespace SDClub.Core
{
    public interface ISystemType
    {
        Type Type();
        Type SystemType();
        int GetInstanceQueueIndex();
    }
}
