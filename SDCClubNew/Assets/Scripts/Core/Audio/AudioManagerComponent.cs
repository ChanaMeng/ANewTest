namespace SDClub.Core
{
    [ComponentOf(typeof(Scene))]
    public class AudioManagerComponent : Entity, IAwake, IUpdate
    {
        public float BgmVolume { get; set; } = 1f;

        public float SfxVolume { get; set; } = 1f;

        public bool IsBgmMuted { get; set; }

        public bool IsSfxMuted { get; set; }

        public bool IsMuted
        {
            get => IsBgmMuted && IsSfxMuted;
            set
            {
                IsBgmMuted = value;
                IsSfxMuted = value;
            }
        }
    }
}
