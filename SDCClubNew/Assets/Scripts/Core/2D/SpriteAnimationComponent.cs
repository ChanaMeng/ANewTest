using System.Collections.Generic;

namespace SDClub.Core
{
    [ComponentOf(typeof(Scene))]
    public class SpriteAnimationComponent : Entity, IAwake, IUpdate, IDestroy
    {
        public Dictionary<string, SpriteAnimationClip> Clips { get; set; }
        public string CurrentClip { get; set; }
        public bool IsPlaying { get; set; }
        public bool Loop { get; set; }
        public float Speed { get; set; } = 1f;

        // internal state
        public int CurrentFrameIndex;
        public float FrameTimer;
    }

    public struct SpriteAnimationClip
    {
        public string Name;
        public string[] SpritePaths;
        public float FrameDuration;
        public bool Loop;
    }

    // Animation events published via EventSystem
    public struct OnAnimationFrameChanged
    {
        public string ClipName;
        public int FrameIndex;
    }

    public struct OnAnimationComplete
    {
        public string ClipName;
    }
}
