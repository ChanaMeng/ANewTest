using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    [Invoke((long)SceneType.Main)]
    public class FiberInit_Main : AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit args)
        {
            Scene root = args.Fiber.Root;

            // 添加核心组件到 Root Scene
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();

            // 添加 Model 层组件
            root.AddComponent<NetComponent>();

            // 发布 EntryEvent3 触发客户端组件初始化
            EventSystem.Instance.Publish(root, new EntryEvent3());
        }
    }

    public class CoroutineLockComponent : Entity, IAwake
    {
    }
}
