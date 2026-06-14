using SDClub.Core;
using UnityEngine;

namespace SDClub.Hotfix.Client
{
    [EntitySystem]
    public class SpriteAnimationAwakeSystem : AwakeSystem<SpriteAnimationComponent>
    {
        protected override void Awake(SpriteAnimationComponent self)
        {
            self.Clips = new System.Collections.Generic.Dictionary<string, SpriteAnimationClip>();
            self.CurrentClip = null;
            self.IsPlaying = false;
            self.Loop = false;
            self.Speed = 1f;
            self.CurrentFrameIndex = 0;
            self.FrameTimer = 0f;
        }
    }

    [EntitySystem]
    public class SpriteAnimationUpdateSystem : UpdateSystem<SpriteAnimationComponent>
    {
        protected override void Update(SpriteAnimationComponent self)
        {
            if (!self.IsPlaying || string.IsNullOrEmpty(self.CurrentClip))
                return;

            if (!self.Clips.TryGetValue(self.CurrentClip, out var clip))
                return;

            self.FrameTimer += Time.deltaTime * self.Speed;

            if (self.FrameTimer >= clip.FrameDuration)
            {
                self.FrameTimer -= clip.FrameDuration;
                self.CurrentFrameIndex++;

                if (self.CurrentFrameIndex >= clip.SpritePaths.Length)
                {
                    if (clip.Loop || self.Loop)
                    {
                        self.CurrentFrameIndex = 0;
                    }
                    else
                    {
                        self.CurrentFrameIndex = clip.SpritePaths.Length - 1;
                        self.IsPlaying = false;
                        EventSystem.Instance.Publish(self.IScene as Scene, new OnAnimationComplete { ClipName = self.CurrentClip });
                        return;
                    }
                }

                EventSystem.Instance.Publish(self.IScene as Scene, new OnAnimationFrameChanged
                {
                    ClipName = self.CurrentClip,
                    FrameIndex = self.CurrentFrameIndex
                });
            }
        }
    }

    [EntitySystem]
    public class SpriteAnimationDestroySystem : DestroySystem<SpriteAnimationComponent>
    {
        protected override void Destroy(SpriteAnimationComponent self)
        {
            self.Clips?.Clear();
            self.Clips = null;
            self.CurrentClip = null;
            self.IsPlaying = false;
        }
    }
}
