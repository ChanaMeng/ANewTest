var WXBridge = {
    $WX_BANNER_AD: null,

    WXLogin: function() {
        if (typeof wx === "undefined") return;
        wx.login({
            success: function(res) {
                console.log("WXLogin success: " + res.code);
            },
            fail: function(err) {
                console.error("WXLogin fail: " + err.errMsg);
            }
        });
    },

    WXGetUserInfo: function() {
        if (typeof wx === "undefined") return;
        wx.getUserInfo({
            success: function(res) {
                console.log("WXGetUserInfo success: " + JSON.stringify(res.userInfo));
            },
            fail: function(err) {
                console.error("WXGetUserInfo fail: " + err.errMsg);
            }
        });
    },

    WXShare: function(titlePtr, imageUrlPtr) {
        if (typeof wx === "undefined") return;
        var title = UTF8ToString(titlePtr);
        var imageUrl = UTF8ToString(imageUrlPtr);
        wx.shareAppMessage({
            title: title,
            imageUrl: imageUrl
        });
    },

    WXShowRewardedVideoAd: function(adUnitIdPtr) {
        if (typeof wx === "undefined") return;
        var adUnitId = UTF8ToString(adUnitIdPtr);
        var rewardedVideoAd = wx.createRewardedVideoAd({
            adUnitId: adUnitId
        });
        rewardedVideoAd.onLoad(function() {
            console.log("RewardedVideoAd loaded");
        });
        rewardedVideoAd.onError(function(err) {
            console.error("RewardedVideoAd error: " + err.errMsg);
        });
        rewardedVideoAd.onClose(function(res) {
            console.log("RewardedVideoAd closed, isEnded: " + (res && res.isEnded));
        });
        rewardedVideoAd.show().catch(function(err) {
            rewardedVideoAd.load().then(function() {
                return rewardedVideoAd.show();
            });
        });
    },

    WXShowInterstitialAd: function(adUnitIdPtr) {
        if (typeof wx === "undefined") return;
        var adUnitId = UTF8ToString(adUnitIdPtr);
        var interstitialAd = wx.createInterstitialAd({
            adUnitId: adUnitId
        });
        interstitialAd.onLoad(function() {
            console.log("InterstitialAd loaded");
        });
        interstitialAd.onError(function(err) {
            console.error("InterstitialAd error: " + err.errMsg);
        });
        interstitialAd.show().catch(function(err) {
            interstitialAd.load().then(function() {
                return interstitialAd.show();
            });
        });
    },

    WXCreateBannerAd: function(adUnitIdPtr, left, top, width) {
        if (typeof wx === "undefined") return;
        if (WX_BANNER_AD) {
            WX_BANNER_AD.destroy();
            WX_BANNER_AD = null;
        }
        var adUnitId = UTF8ToString(adUnitIdPtr);
        var windowInfo = wx.getWindowInfo ? wx.getWindowInfo() : wx.getSystemInfoSync();
        WX_BANNER_AD = wx.createBannerAd({
            adUnitId: adUnitId,
            adIntervals: 30,
            style: {
                left: left,
                top: top,
                width: width
            }
        });
        WX_BANNER_AD.onResize(function(res) {
            WX_BANNER_AD.style.top = windowInfo.windowHeight - WX_BANNER_AD.style.realHeight - 10;
        });
        WX_BANNER_AD.show().catch(function(err) {
            console.error("BannerAd show error: " + err.errMsg);
        });
    },

    WXDestroyBannerAd: function() {
        if (WX_BANNER_AD) {
            WX_BANNER_AD.destroy();
            WX_BANNER_AD = null;
        }
    },

    WXVibrateLong: function() {
        if (typeof wx === "undefined") return;
        wx.vibrateLong({
            success: function() {},
            fail: function(err) {
                console.error("VibrateLong fail: " + err.errMsg);
            }
        });
    },

    WXVibrateShort: function() {
        if (typeof wx === "undefined") return;
        wx.vibrateShort({
            type: "light",
            success: function() {},
            fail: function(err) {
                console.error("VibrateShort fail: " + err.errMsg);
            }
        });
    },

    WXGetSystemInfo: function() {
        if (typeof wx === "undefined") return 0;
        var sysInfo = wx.getSystemInfoSync();
        var json = JSON.stringify(sysInfo);
        var lengthBytes = lengthBytesUTF8(json) + 1;
        var buffer = _malloc(lengthBytes);
        stringToUTF8(json, buffer, lengthBytes);
        return buffer;
    }
};

autoAddDeps(WXBridge, "$WX_BANNER_AD");
mergeInto(LibraryManager.library, WXBridge);
