using System.Collections.Generic;

namespace SDClub.Core
{
    [ComponentOf(typeof(Scene))]
    public class ConfigComponent : Entity, IAwake
    {
        public Dictionary<string, object> Configs { get; } = new();
    }
}
