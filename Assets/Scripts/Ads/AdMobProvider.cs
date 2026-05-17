using System;
using UnityEngine;

public class AdMobProvider : MonoBehaviour, IAdProvider
{
    [Header("AdMob Controllers")]
    public BannerViewController Banner;
    public AppOpenAdController AppOpen;
    public InterstitialAdController Interstitial;
    public RewardedAdController Rewarded;
    public RewardedInterstitialAdController RewardedInterstitial;

    private bool _isInitialized;

    public void InitializeAdsSystem()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        
        Banner?.LoadAd();
        Interstitial?.LoadAd();
        Rewarded?.LoadAd();
        RewardedInterstitial?.LoadAd();
        AppOpen?.LoadAd();
    }

    public void ShowBanner() => Banner?.ShowAd();
    public void HideBanner() => Banner?.HideAd();

    public void ShowInterstitialAd(Action onAdClosed = null)
    {
        if (Interstitial != null) Interstitial.ShowAd(onAdClosed);
        else onAdClosed?.Invoke();
    }

    public void ShowRewardedAd(Action onRewardEarned, Action onAdClosed = null)
    {
        if (Rewarded != null) Rewarded.ShowAd(onRewardEarned, onAdClosed);
        else onAdClosed?.Invoke();
    }

    public void ShowRewardedInterstitialAd(Action onRewardEarned, Action onAdClosed = null)
    {
        if (RewardedInterstitial != null) RewardedInterstitial.ShowAd(onRewardEarned, onAdClosed);
        else onAdClosed?.Invoke();
    }
}