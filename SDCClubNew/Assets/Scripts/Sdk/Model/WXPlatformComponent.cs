using SDClub.Core;
using SDClub.Sdk;

#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace SDClub.Sdk.Model
{
    public class WXPlatformComponent : Entity, IWXSDK, IAwake
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void WXLogin();

        [DllImport("__Internal")]
        private static extern void WXGetUserInfo();

        [DllImport("__Internal")]
        private static extern void WXShare(string title, string imageUrl);

        [DllImport("__Internal")]
        private static extern void WXShowRewardedVideoAd(string adUnitId);

        [DllImport("__Internal")]
        private static extern void WXShowInterstitialAd(string adUnitId);

        [DllImport("__Internal")]
        private static extern void WXCreateBannerAd(string adUnitId, int left, int top, int width);

        [DllImport("__Internal")]
        private static extern void WXDestroyBannerAd();

        [DllImport("__Internal")]
        private static extern void WXVibrateLong();

        [DllImport("__Internal")]
        private static extern void WXVibrateShort();

        [DllImport("__Internal")]
        private static extern string WXGetSystemInfo();
#endif

        public void Login()
        {
#if UNITY_WEBGL
            WXLogin();
#endif
        }

        public void GetUserInfo()
        {
#if UNITY_WEBGL
            WXGetUserInfo();
#endif
        }

        public void Share(string title, string imageUrl)
        {
#if UNITY_WEBGL
            WXShare(title, imageUrl);
#endif
        }

        public void ShowRewardedVideoAd(string adUnitId)
        {
#if UNITY_WEBGL
            WXShowRewardedVideoAd(adUnitId);
#endif
        }

        public void ShowInterstitialAd(string adUnitId)
        {
#if UNITY_WEBGL
            WXShowInterstitialAd(adUnitId);
#endif
        }

        public void CreateBannerAd(string adUnitId, int left, int top, int width)
        {
#if UNITY_WEBGL
            WXCreateBannerAd(adUnitId, left, top, width);
#endif
        }

        public void DestroyBannerAd()
        {
#if UNITY_WEBGL
            WXDestroyBannerAd();
#endif
        }

        public void VibrateLong()
        {
#if UNITY_WEBGL
            WXVibrateLong();
#endif
        }

        public void VibrateShort()
        {
#if UNITY_WEBGL
            WXVibrateShort();
#endif
        }

        public string GetSystemInfo()
        {
#if UNITY_WEBGL
            return WXGetSystemInfo();
#else
            return string.Empty;
#endif
        }
    }
}
