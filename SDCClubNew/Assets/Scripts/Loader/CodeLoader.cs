using SDClub.Core;

namespace SDClub.Loader
{
    /// <summary>
    /// 代码加载器，负责扫描 Assembly、创建 CodeTypes 并驱动 [Code] 类初始化
    /// </summary>
    public class CodeLoader : Singleton<CodeLoader>, ISingletonAwake
    {
        public void Awake()
        {
        }

        public void Start()
        {
            Log.Debug("CodeLoader.Start - 开始代码加载");

            // 1. 扫描所有 Assembly，创建 CodeTypes 单例 (CodeTypes.Awake() 内部扫描)
            World.Instance.AddSingleton<CodeTypes>();

            // 2. 实例化所有 [Code] 特性标注的类
            //    EventSystem、EntitySystemSingleton 等通过 [Code] 自动注册为单例
            CodeTypes.Instance.CreateCode();

            Log.Debug("CodeLoader.Start - CodeTypes 和 [Code] 类已初始化");

            // 3. 进入业务逻辑
            Entry.Start();
        }
    }
}
