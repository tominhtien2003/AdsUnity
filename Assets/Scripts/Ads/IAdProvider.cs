using System;

public interface IAdProvider
{
    void InitializeAdsSystem();
    void ShowBanner();
    void HideBanner();
    void ShowInterstitialAd(Action onAdClosed = null);
    void ShowRewardedAd(Action onRewardEarned, Action onAdClosed = null);
    void ShowRewardedInterstitialAd(Action onRewardEarned, Action onAdClosed = null);
}