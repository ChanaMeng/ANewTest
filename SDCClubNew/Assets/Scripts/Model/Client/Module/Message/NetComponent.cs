using SDClub.Core;

namespace SDClub.Model
{
    public class NetComponent : Entity, IAwake, IUpdate
    {
        public AService Service { get; set; }
        public Session Session { get; set; }
    }
}
