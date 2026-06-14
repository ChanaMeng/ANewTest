# SDCClubNew 游戏框架技术文档

## 项目概述

基于 ET (Entity-Component) 框架思想构建的 Unity 游戏框架。采用 Entity 为基类的四重架构（Model / ModelView / Hotfix / HotfixView），World 作为根节点管理全局单例，EventSystem 事件系统实现模块间解耦通信。

**引擎**: Unity (Tuanjie) 2022.3.62t8  
**资源管理**: YooAsset 3.0  
**异步**: UniTask  
**序列化**: 自定义轻量序列化  

> 不使用 HybridCLR 代码热更，所有 Assembly 正常编译，Hotfix/HotfixView 仅作为架构分层命名。

---

## 架构总览

```
                    ┌─────────────────────────────┐
                    │      SDClub.HotfixView       │
                    │      (视图逻辑层)             │
                    └────────────┬────────────────┘
                ┌────────────────┼──────────────────┐
                ▼                                 ▼
┌──────────────────────────┐    ┌──────────────────────────┐
│   SDClub.ModelView        │    │     SDClub.Hotfix         │
│   (视图数据层)             │    │     (逻辑层)              │
└────────────┬─────────────┘    └────────────┬─────────────┘
             └────────────┬─────────────────┘
                          ▼
              ┌───────────────────────┐
              │    SDClub.Model        │
              │    (数据定义层)         │
              └───────────┬───────────┘
                          ▼
              ┌───────────────────────┐
              │    SDClub.Core         │
              │    (框架核心)           │
              │  Entity / World / Fiber │
              │  EventSystem / Network  │
              └───────────────────────┘
                          ▲
          ┌───────────────┼───────────────┐
          ▼                               ▼
┌──────────────────┐          ┌──────────────────┐
│  SDClub.Loader    │          │SDClub.UIFrameWork │
│  (启动/YooAsset)   │          │  (UI 框架)        │
└──────────────────┘          └──────────────────┘
```

---

## 一、核心框架 (SDClub.Core)

### 1.1 Entity 实体系统

`Entity` 是整个框架的基石，所有游戏对象（玩家、NPC、UI、组件）均为 Entity。

```csharp
public abstract class Entity : DisposeObject, IPool
{
    public long Id { get; set; }          // 持久化 ID
    public long InstanceId { get; }       // 唯一实例 ID (0=已销毁)
    public Entity Parent { get; }         // 父节点
    public bool IsDisposed { get; }       // 是否已销毁
    
    // 子节点管理
    T AddChild<T>(...) where T : Entity;
    T GetChild<T>(long id) where T : Entity;
    
    // 组件管理
    T AddComponent<T>(...) where T : Entity;
    T GetComponent<T>() where T : Entity;
    void RemoveComponent<T>() where T : Entity;
    
    // 领域根节点
    IScene Domain { get; }  // 向上遍历找到最近的 Scene
}
```

**层次关系**: Scene → Entity → Children/Components (树形结构)  
**生命周期**: `AddComponent()` → `Awake()` → `Update()` → `LateUpdate()` → `Dispose()` → `Destroy()`

### 1.2 Scene 场景

```csharp
public class Scene : Entity, IScene
{
    public SceneType SceneType { get; set; }  // Main, NetClient 等
    public Fiber Fiber { get; set; }           // 所属 Fiber
}
```

Scene 是 Entity 树的根节点，每个 Fiber 持有唯一的 Root Scene。

### 1.3 EntitySystem 系统驱动

所有行为通过 **EntitySystem** 驱动，而非在 Entity 内部编写逻辑：

```csharp
[EntitySystem]
public abstract class AwakeSystem<T> : IAwakeSystem where T : Entity, IAwake
{
    protected abstract void Awake(T self);
}

[EntitySystem]
public abstract class UpdateSystem<T> : IUpdateSystem where T : Entity, IUpdate
{
    protected abstract void Update(T self);
}
```

**注册方式**: `EntitySystemSingleton` 启动时扫描 `[EntitySystem]` 特性，构建 `TypeSystems` 映射表。

| 接口 | 触发时机 |
|------|---------|
| `IAwakeSystem<T>` | AddComponent() 时 |
| `IUpdateSystem<T>` | 每帧 Update |
| `ILateUpdateSystem<T>` | 每帧 LateUpdate |
| `IDestroySystem<T>` | Dispose() 时 |
| `IOnLoadSystem<T>` | UI 创建后 |
| `IOnShowSystem<T>` | UI 显示时 |
| `IBindSystem<T>` | UI 绑定时 |

### 1.4 World 全局管理

```csharp
public class World : IDisposable
{
    public static World Instance { get; }
    
    T AddSingleton<T>() where T : ASingleton, ISingletonAwake;
    T AddSingleton<T, A>(A a) where T : ASingleton, ISingletonAwake<A>;
    void AddSingleton(ASingleton singleton);
    void Dispose();  // 逆序（栈）释放所有单例
}
```

所有全局服务通过 `World.Instance.AddSingleton<T>()` 注册：
- Options、Logger、TimeInfo
- FiberManager、CodeTypes
- EventSystem、EntitySystemSingleton
- TimerComponent、ObjectPool 等

### 1.5 Fiber 执行上下文

```csharp
public class Fiber
{
    public int Id { get; }
    public Scene Root { get; }
    public EntitySystem EntitySystem { get; }
    public TimerComponent Timer { get; }
    public CoroutineLockComponent CoroutineLock { get; }
    
    public static Fiber Instance { get; }  // [ThreadStatic]
    
    void Update();
    void LateUpdate();
}
```

每个 Fiber 拥有独立的 EntitySystem 和组件，实现隔离的游戏循环。

### 1.6 EventSystem 事件系统

**Publish（广播模式）**: 一对多，可能无人订阅，模块间解耦

```csharp
// 定义事件
public struct OnPlayerLogin { public long PlayerId; }

// 订阅事件
[Event(SceneType.Main)]
public class OnPlayerLoginHandler : AEvent<Scene, OnPlayerLogin>
{
    protected override async UniTask Run(Scene scene, OnPlayerLogin a) { ... }
}

// 发布事件
EventSystem.Instance.Publish(scene, new OnPlayerLogin { PlayerId = 1 });
```

**Invoke（函数调用模式）**: 一对一，必须有订阅者，模块内分发

```csharp
[Invoke((long)SceneType.Main)]
public class FiberInit_Main : AInvokeHandler<FiberInit> { ... }
```

### 1.7 反射注册 (CodeTypes)

```
CodeTypes.Awake() 
  → 扫描所有 Assembly 中带 [EntitySystem]/[Event]/[Invoke]/[Code] 特性的类
  → 建立 Attribute → Type 映射表
  → CodeTypes.CreateCode() 实例化所有 [Code] 类并注册到 World
```

---

## 二、网络层

### 2.1 传输层

| 类 | 协议 | 说明 |
|----|------|------|
| `AService` | 抽象 | Accept/Read/Error 回调 |
| `AChannel` | 抽象 | Id, ChannelType, RemoteAddress |
| `KService/KChannel` | KCP | 可靠 UDP |
| `TService/TChannel` | TCP | TCP 传输 |
| `WService/WChannel` | WebSocket | WS 传输 |

### 2.2 消息层

```csharp
public class Session : Entity
{
    public AService AService { get; }
    
    // 发送消息
    void Send(IMessage message);
    
    // RPC 调用
    UniTask<IResponse> Call(IRequest request);
    
    // 回调管理
    Dictionary<int, RpcInfo> requestCallbacks;
}
```

**消息分发**:
```
收到消息 → Session.Run() → MessageSessionDispatcher.Dispatch()
  → 根据 Opcode + SceneType 查找 Handler
  → Handler.Run(session, message)
```

---

## 三、Loader 启动层

### 3.1 启动流程

```
Init.Start() (MonoBehaviour)
  → StartAsync()
    → World.AddSingleton<Options>()
    → World.AddSingleton<Logger>()
    → World.AddSingleton<TimeInfo>()
    → World.AddSingleton<FiberManager>()
    → World.AddSingleton<YooAssetComponent>()
    → YooAssetComponent.InitializeAsync()  // YooAsset 3.0 初始化
    → CodeLoader.Start()
      → CodeTypes 扫描所有 Assembly
      → CodeTypes.CreateCode()              // 注册 [Code] 类
      → EntitySystemSingleton 初始化         // 注册 [EntitySystem]
      → EventSystem 初始化                   // 注册 [Event]
      → FiberManager.Create(SceneType.Main)  // 创建主 Fiber
      → 加载初始场景
```

### 3.2 YooAsset 3.0 集成

```csharp
// 初始化
YooAssets.Initialize();
var package = YooAssets.CreatePackage("DefaultPackage");
await package.InitializeAsync(parameters);

// 资源加载
var handle = package.LoadAssetAsync<GameObject>("prefab_path");
await handle;
GameObject obj = handle.InstantiateSync();

// 场景加载
var sceneHandle = package.LoadSceneAsync("scene_path");
await sceneHandle;
```

**资源包分类**: DefaultPackage（通用）、UIPackage（UI）、ConfigPackage（配置）

---

## 四、Model 数据层

定义所有 Entity 类型、Component 类型、事件类型的声明，以及消息模型。

### 4.1 Entry 入口

```csharp
public static class Entry
{
    public static void Start()
    {
        // 注册元数据
        // 发布 EntryEvent1: 注册序列化类型
        // 发布 EntryEvent2: 创建管理器单例
        // 发布 EntryEvent3: 初始化客户端组件
    }
}
```

### 4.2 消息模型

```csharp
public interface IMessage { }
public interface IRequest : IMessage { int RpcId { get; set; } }
public interface IResponse : IMessage { int RpcId { get; set; } int Error { get; set; } }
```

---

## 五、UI 框架 (SDClub.UIFrameWork)

### 5.1 核心设计：逻辑与视图分离

```
┌─────────────────────────────┐
│   UI Entity (逻辑)           │  ← 继承 Entity，纯逻辑
│   - 数据状态                 │
│   - 业务逻辑                 │
└──────────┬──────────────────┘
           │ 关联
┌──────────▼──────────────────┐
│   UIViewComponent (视图)      │  ← 持有 Prefab/GameObject
│   - BundlePath               │
│   - UIPrefab                 │
└──────────┬──────────────────┘
           │ 引用
┌──────────▼──────────────────┐
│   UI 组件封装 (绑定)          │  ← ButtonComponent 等
│   - Button → ButtonComponent │     Entity Component
│   - Text → TextComponent     │     封装 Unity 原生组件
└─────────────────────────────┘
```

### 5.2 UI 创建流程

```
GameUIComponent.CreateUI<T>(args)
  → AddComponent<T>()              // 创建逻辑 Entity
  → AddComponent<UIViewComponent>()// 创建视图组件
  → 通过 YooAsset 加载 Prefab
  → AddToLayer(layer)              // 挂到对应 Canvas 层级
  → CallBindUI()                   // [UIBind] 自动绑定原生组件
  → CallOnLoad()                   // [UILife] OnLoad
  → CallOnShow(args)               // [UILife] OnShow
```

### 5.3 层级系统

| 层级 | 说明 |
|------|------|
| Bottom | 底层 (背景等) |
| Center | 中层 (主界面) |
| Popup | 弹窗层 |
| Top | 顶层 (Loading、提示) |

### 5.4 绑定示例

```csharp
// 逻辑 Entity
[UILife(typeof(LoginPanelLogic))]
[UIBind(typeof(LoginPanelBind))]
[ComponentOf(typeof(Scene))]
public class LoginPanelEntity : Entity, IAwake, IOnLoad, IOnShow
{
    public ButtonComponent LoginButton { get; set; }
    public TextComponent TitleText { get; set; }
}

// 绑定逻辑
public class LoginPanelBind : UIAutoSystem<LoginPanelEntity>
{
    protected override void BindUI(LoginPanelEntity self)
    {
        self.LoginButton = self.AddComponent<ButtonComponent>("LoginBtn");
        self.TitleText = self.AddComponent<TextComponent>("TitleTxt");
    }
}

// 生命周期
public class LoginPanelLogic : UILogicSystem<LoginPanelEntity>
{
    protected override async UniTask OnLoad(LoginPanelEntity self) { ... }
    protected override void OnShow(LoginPanelEntity self, object args) { ... }
}
```

---

## 六、关键技术栈

| 技术 | 用途 | 版本 |
|------|------|------|
| Unity (Tuanjie) | 游戏引擎 | 2022.3.62t8 |
| YooAsset | 资源管理 | 3.0 |
| UniTask | 异步编程 | 2.x |
| KCP | 可靠 UDP 传输 | - |
| Newtonsoft.Json | JSON 序列化 | - |

---

## 七、命名空间

```
SDClub.Core.*         - 框架核心
SDClub.Loader.*       - 启动层
SDClub.Model.*        - 数据层
SDClub.ModelView.*    - 视图数据层
SDClub.Hotfix.*       - 逻辑层
SDClub.HotfixView.*   - 视图逻辑层
SDClub.UIFrameWork.*  - UI 框架
SDClub.Editor.*       - 编辑器工具
```

---

## 八、实现状态

**完成日期**: 2026-06-11

**总体进度**: 框架核心代码全部完成，共 **113 个 C# 文件**，覆盖 8 个 Assembly。

### 各模块文件分布

| Assembly | 文件数 | 状态 | 说明 |
|----------|--------|------|------|
| SDClub.Core | 42 | 完成 | Entity、World、Fiber、EventSystem、Network(KCP/TCP/WS)、CodeTypes、Timer、Log、Helper、ObjectPool、序列化 |
| SDClub.Loader | 8 | 完成 | Init.cs (MonoBehaviour)、Options、Logger、TimeInfo、FiberManager、CodeLoader、YooAsset 集成 |
| SDClub.Model | 7 | 完成 | Entry.cs (分阶段启动)、Session (网络会话)、MessageSessionHandler/Dispatcher、消息模型定义 |
| SDClub.ModelView | 2 | 完成 | ViewContext、ModelViewConfig |
| SDClub.Hotfix | 7 | 完成 | 主流程 (EntryEvent 处理器)、网络系统、Camera 系统、Timer 系统 |
| SDClub.HotfixView | 7 | 完成 | 主流程视图、UI 按钮/输入/滑块/开关事件系统 |
| SDClub.UIFrameWork | 16 | 完成 | UILogic 生命周期、UIViewComponent、10 个 UI 组件封装、GameUIComponent、对象池、音频 |
| SDClub.Editor | 1 | 框架就绪 | asmdef 已创建，编辑器工具待后续扩展 |

### 核心文件清单

```
Assets/Scripts/
├── Core/                              (42 文件)
│   ├── Entity/                        (13)   Entity.cs, Scene.cs, EntitySystemSingleton.cs, EntitySystemAttribute.cs, IAwake.cs...
│   ├── World/                         (4)    World.cs, Singleton.cs, ISingletonAwake.cs
│   │   └── Module/
│   │       ├── EventSystem/            (6)    EventSystem.cs, AEvent.cs, AInvokeHandler.cs, TypeEventSystem.cs...
│   │       ├── Code/                   (2)    CodeTypes.cs, ICodeCreator.cs
│   │       ├── Timer/                  (2)    TimerComponent.cs, TimerType.cs
│   │       └── Log/                    (2)    Log.cs, ILogger.cs
│   ├── Fiber/                          (4)    Fiber.cs, FiberInit.cs, FiberManager.cs
│   ├── Network/                        (13+1) KService/KChannel, TService/TChannel, WService/WChannel, Session相关, Kcp算法
│   ├── Helper/                         (5)    IdGenerater.cs, ByteHelper.cs, ZipHelper.cs, MD5Helper.cs, MathHelper.cs
│   └── Object/                         (3)    DisposeObject.cs, ObjectPool.cs, IPool.cs
├── Loader/                             (8 文件)
│   ├── MonoBehaviour/                  (3)    Init.cs, SetLayers.cs
│   ├── CodeLoader.cs, Options.cs, Logger.cs, TimeInfo.cs, FiberManager.cs
│   └── YooAsset/                       (1)    YooAssetComponent.cs
├── Model/                              (7 文件)
│   └── Client/
│       ├── Entry.cs, Session.cs, MessageSessionHandler.cs, MessageSessionDispatcher.cs
│       ├── Helper/                     (1)
│       └── Module/Message/             (5)    消息定义 (IMessage, IRequest, IResponse, MessageObject...)
├── ModelView/                          (2 文件)
│   └── Client/ViewContext.cs, ModelViewConfig.cs
├── Hotfix/                             (7 文件)
│   └── Client/
│       ├── EntryEvent1_2_3.cs, FiberInit_Main.cs
│       └── Module/Message/             (3)    NetComponentSystem, SessionSystem
├── HotfixView/                         (7 文件)
│   └── Client/
│       ├── EntryEvent1_2_3.cs, FiberInit_Main.cs
│       └── UI/                         (5)    ButtonSystem, InputSystem, SliderSystem, ToggleSystem, ScrollRectSystem
├── UIFrameWork/                        (16 文件)
│   ├── UI/                             (3)    UILogic.cs, UIViewComponent.cs, UILifeComponent.cs
│   ├── UIComponent/                    (12)   ButtonComponent.cs, ImageComponent.cs, TextComponent.cs, InputComponent.cs,
│   │                                          SliderComponent.cs, ToggleComponent.cs, ScrollRectComponent.cs,
│   │                                          RawImageComponent.cs, CanvasGroupComponent.cs, AnimatorComponent.cs,
│   │                                          GameUIComponent.cs, GameUIGlobalComponent.cs
│   ├── GameObjectPool/                 (1)    GameObjectPoolComponent.cs
│   ├── Sound/                          (1)    AudioComponent.cs
│   └── Helper/Const/ExtensionAPI/Gear/ (4)   辅助类
└── Editor/                             asmdef 已创建 (待具体实现)
```

### 已确认的架构决策

- 不使用 HybridCLR：所有 Assembly 直接编译引用，无 IL 解释
- CodeTypes 简化为 `AppDomain.CurrentDomain.GetAssemblies()` 扫描
- Entity 对象池 (`IPool`) 默认启用，EntityStatus 标志位管理生命周期
- IAwake / IUpdate / ILateUpdate / IDestroy 四重 EntitySystem 接口
- EventSystem 支持 Publish (广播) + Invoke (函数调用) 双模式
- Fiber 通过 `[ThreadStatic]` 实现线程隔离
- YooAsset 3.0 通过 `#if YOOASSET` 条件编译切换编辑器/真机模式
- UI 逻辑视图分离：UI Entity (逻辑) + UIViewComponent (视图) + UIBind 绑定

### 后续工作

1. **资源准备**: 配置 YooAsset 资源包 (DefaultPackage, UIPackage, ConfigPackage)
2. **场景搭建**: 创建 Boot 启动场景，挂载 Init MonoBehaviour
3. **网络接入**: 根据实际协议定义具体 Message 类型和 Opcode
4. **UI 模板**: 创建 UI Prefab 模板，定义 Canvas 层级结构
5. **编辑器工具**: 填充 SDClub.Editor Assembly
6. **单元测试**: 为 Entity、EventSystem、Network 等核心模块编写测试
