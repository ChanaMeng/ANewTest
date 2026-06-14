namespace SDClub.Sdk
{
    public interface IWXSDK
    {
        void Login();
        void GetUserInfo();
        void Share(string title, string imageUrl);
        void ShowRewardedVideoAd(string adUnitId);
        void ShowInterstitialAd(string adUnitId);
        void CreateBannerAd(string adUnitId, int left, int top, int width);
        void DestroyBannerAd();
        void VibrateLong();
        void VibrateShort();
        string GetSystemInfo();
    }
}
