using SDClub.Core;
using UnityEngine;

namespace SDClub.UIFrameWork
{
    public class AnimatorComponent : Entity, IAwake
    {
        public Animator Animator { get; set; }
        public void Play(string stateName) => Animator?.Play(stateName);
        public void SetTrigger(string name) => Animator?.SetTrigger(name);
    }
}
