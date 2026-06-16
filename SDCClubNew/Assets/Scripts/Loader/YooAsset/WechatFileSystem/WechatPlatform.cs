using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using WeChatWASM;

namespace SDClub.Loader
{
    /// <summary>
    /// 微信小游戏平台实现
    /// </summary>
    internal class WechatPlatform : IWebPlatformStrategy
    {
        public UnityWebRequest CreateAssetBundleRequest(WebAssetBundleRequestArgs args)
        {
            UnityWebRequest request = WXAssetBundle.GetAssetBundle(args.Url);
            request.disposeDownloadHandlerOnDispose = true;
            return request;
        }

        public AssetBundle ExtractAssetBundle(UnityWebRequest request)
        {
            var downloadHandler = (DownloadHandlerWXAssetBundle)request.downloadHandler;
            return downloadHandler.assetBundle;
        }

        public void UnloadAssetBundle(AssetBundle assetBundle, bool unloadAll)
        {
            assetBundle.WXUnload(unloadAll);
        }
    }
}
