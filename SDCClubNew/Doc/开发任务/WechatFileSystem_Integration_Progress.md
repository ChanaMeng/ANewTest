# WechatFileSystem 集成改造进度

> 目标：将 YooAsset 提供的 WechatFileSystem 从 Samples~ 搬入项目，替换 YooAssetComponent 中通用的 WebPlayMode

## 最终状态：✅ 集成完成

---

## 改造记录

| 日期 | 操作 | 结果 |
|------|------|------|
| 2026-06-17 | 搬运 WechatFileSystem 4 个文件 | ✅ 完成 |
| 2026-06-17 | 创建 YooAsset.MiniGame.asmdef | ✅ 完成 |
| 2026-06-17 | 更新 SDClub.Loader.asmdef 引用 | ✅ 完成 |
| 2026-06-17 | 更新 YooAssetComponent.cs | ✅ 完成 |
| 2026-06-17 | MCP 编译验证 | ✅ 零编译错误 |
| 2026-06-17 | MCP 日志检查 | ✅ 无关联错误 |

---

## 改造步骤详情

### 1. 搬运 WechatFileSystem 文件 ✅
创建目录 `Assets/Scripts/Loader/YooAsset/WechatFileSystem/`，包含：
- `WechatFileSystem.cs` — `WechatFileSystemCreater` (public) + `WechatFileSystem` (internal)
- `WechatPlatform.cs` — `WechatPlatform` (internal, IWebPlatformStrategy)
- `Operation/WXFSClearAllBundleFilesOperation.cs` — 清除全部缓存
- `Operation/WXFSClearUnusedBundleFilesAsync.cs` — 清除未使用缓存

**适配修改：**
- 添加 `namespace SDClub.Loader`
- 移除条件编译宏（由 asmdef 控制）
- `fileSystemClass` 改为 `"WechatFileSystem,SDClub.Loader"`
- 修复 `EFileSystemParameter.AssetBundleDecryptor` / `RawBundleDecryptor` 大小写

### 2. 创建 YooAsset.MiniGame.asmdef ✅
- 程序集名：`YooAsset.MiniGame`（匹配 YooAsset `InternalsVisibleTo`）
- 引用：`YooAsset`、`Wx`
- `autoReferenced: true`

### 3. 更新程序集引用 ✅
`SDClub.Loader.asmdef` references 添加：
- `"Wx"` — 微信 SDK 程序集
- `"YooAsset.MiniGame"` — 微信文件系统程序集

### 4. 更新初始化代码 ✅
`YooAssetComponent.cs` WEIXINMINIGAME 分支：
- 移除通用 `WebServerFileSystemParameters` + `WebNetworkFileSystemParameters`
- 改用 `WechatFileSystemCreater.CreateFileSystemParameters(remoteService)`
- 只设置 `WebNetworkFileSystemParameters`（WechatFileSystem 包含所需全部能力）

---

## 架构关系

```
YooAsset.MiniGame (SDClub.Loader 命名空间)
├── WechatFileSystemCreater (public)
│     └── fileSystemClass = "WechatFileSystem,SDClub.Loader"
└── WechatFileSystem : WebNetworkFileSystem (internal)
      ├── 缓存路径: WX.env.USER_DATA_PATH/__GAME_FILE_CACHE
      ├── 缓存清理: WX.CleanAllFileCache / WX.RemoveFile
      └── WechatPlatform: WXAssetBundle 加载
           └──引用 "Wx" 程序集 (WeChatWASM)

SDClub.Loader
├── 引用 YooAsset.MiniGame ✅
├── 引用 Wx ✅
└── YooAssetComponent.cs
      └── WEIXINMINIGAME → WechatFileSystemCreater ✅
```

---

## 待完成（非本次改造范围）

| # | 待处理项 | 说明 |
|---|----------|------|
| 1 | YooAsset 资源分组配置 | 在 Unity Editor 中配置 DefaultPackage/UIPackage/ConfigPackage 的资源收集规则 |
| 2 | CDN 地址替换 | `YooAssetConfig.WeChatCDNBaseUrl` 改为真实 CDN 地址 |
| 3 | 构建测试 | 实际构建微信小游戏验证资源加载流程 |
