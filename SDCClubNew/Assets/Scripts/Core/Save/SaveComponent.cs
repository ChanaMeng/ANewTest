using System.Collections.Generic;

namespace SDClub.Core
{
    [ComponentOf(typeof(Scene))]
    public class SaveComponent : Entity, IAwake
    {
        public Dictionary<string, object> SaveData { get; } = new();
    }
}
