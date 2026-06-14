using SDClub.Core;

namespace SDClub.Model
{
    public static class Entry
    {
        public static void Start()
        {
            // 发布分阶段初始化事件
            var fiber = FiberManager.Instance.Get(1);
            var scene = fiber?.Root;
            if (scene == null) return;
            
            EventSystem.Instance.Publish(scene, new EntryEvent1());
            EventSystem.Instance.Publish(scene, new EntryEvent2());
            EventSystem.Instance.Publish(scene, new EntryEvent3());
        }
    }
}
