namespace SDClub.Loader
{
    /// <summary>
    /// YooAsset 资源包配置
    /// </summary>
    public static class YooAssetConfig
    {
        // 资源包名称
        public const string DefaultPackage = "DefaultPackage";
        public const string UIPackage = "UIPackage";
        public const string ConfigPackage = "ConfigPackage";

        // 微信小游戏 CDN 资源地址（发布前替换为实际 CDN 地址）
        public const string WeChatCDNBaseUrl = "https://your-cdn.com/resources/";

        // 资源版本号
        public const int ResourceVersion = 1;

        /// <summary>
        /// 获取指定资源包的 CDN URL
        /// </summary>
        public static string GetPackageUrl(string packageName)
        {
            return $"{WeChatCDNBaseUrl}{packageName}/{ResourceVersion}";
        }

        /// <summary>
        /// 是否为可运行的主资源包
        /// </summary>
        public static bool IsMainPackage(string packageName)
        {
            return packageName == DefaultPackage;
        }
    }
}
