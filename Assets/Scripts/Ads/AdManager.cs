using System;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("Controllers")]
    public BannerViewController BannerViewController;
    public AppOpenAdController  AppOpenAdController;
    public InterstitialAdController  InterstitialAdController;
    public RewardedAdController  RewardedAdController;
    public RewardedInterstitialAdController  RewardedInterstitialAdController;

    
    //cache
    // Lưu trạng thái game trước khi quảng cáo mở để restore chính xác.
    private float _previousTimeScale = 1f;
    private bool _previousAudioPause;
    private bool _isPausedByAd;
    private bool _isInitialized;
    
    // CỜ QUAN TRỌNG: Xác định xem có đang show quảng cáo full màn hình nào không
    public bool IsShowingFullScreenAd { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void InitializeAdsSystem()
    {
        if (_isInitialized)
        {
            Debug.Log("[AdManager] Ads system đã được khởi tạo trước đó.");
            return;
        }
        _isInitialized = true;
        Debug.Log("[AdManager] Tải toàn bộ quảng cáo...");
        
        
        BannerViewController?.LoadAd();
        InterstitialAdController?.LoadAd();
        RewardedAdController?.LoadAd();
        RewardedInterstitialAdController?.LoadAd();
    }

    public void PauseGame()
    {
        if (_isPausedByAd)
            return;

        _isPausedByAd = true;
        IsShowingFullScreenAd = true; // Bật cờ khóa AppOpenAd

        _previousTimeScale = Time.timeScale;
        _previousAudioPause = AudioListener.pause;

        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        if (!_isPausedByAd)
            return;

        _isPausedByAd = false;
        IsShowingFullScreenAd = false; // Tắt cờ, mở lại AppOpenAd

        Time.timeScale = _previousTimeScale;
        AudioListener.pause = _previousAudioPause;
    }

    // =========================================================================
    // BANNER & APP OPEN
    // =========================================================================
    public void ShowBanner() => BannerViewController?.ShowAd();
    public void HideBanner() => BannerViewController?.HideAd();

    // =========================================================================
    // CALL TỪ BẤT KỲ ĐÂU: REWARDED AD
    // =========================================================================
    public void ShowRewardedAd(Action onRewardEarned, Action onAdClosed = null)
    {
        if (RewardedAdController == null || IsShowingFullScreenAd)
        {
            Debug.LogWarning("[AdManager] Không thể show rewarded ad vì RewardedAdController chưa được gán.");
            onAdClosed?.Invoke();
            return;
        }
        PauseGame(); // 1. Đóng băng game

        // 2. Truyền lệnh xuống Controller chuyên biệt
        RewardedAdController.ShowAd(
            onReward: onRewardEarned, 
            onClosed: () => 
            {
                ResumeGame(); // 3. Mở băng game khi quảng cáo đã tắt
                onAdClosed?.Invoke(); // 4. Trả logic về cho script gọi nó
            }
        );
    }

    // =========================================================================
    // CALL TỪ BẤT KỲ ĐÂU: INTERSTITIAL AD
    // =========================================================================
    public void ShowInterstitialAd(Action onAdClosed = null)
    {
        if (InterstitialAdController == null || IsShowingFullScreenAd)
        {
            Debug.LogWarning("[AdManager] Không thể show interstitial ad vì InterstitialAdController chưa được gán.");
            onAdClosed?.Invoke();
            return;
        }
        PauseGame();

        InterstitialAdController.ShowAd(
            onClosed: () => 
            {
                ResumeGame();
                onAdClosed?.Invoke();
            }
        );
    }
    
    // =========================================================================
    // CALL TỪ BẤT KỲ ĐÂU: REWARDED INTERSTITIAL AD
    // =========================================================================
    public void ShowRewardedInterstitialAd(Action onRewardEarned, Action onAdClosed = null)
    {
        if (RewardedInterstitialAdController == null || IsShowingFullScreenAd)
        {
            Debug.LogWarning("[AdManager] Không thể show rewarded interstitial ad vì RewardedInterstitialAdController chưa được gán.");
            onAdClosed?.Invoke();
            return;
        }

        PauseGame();
        RewardedInterstitialAdController.ShowAd(
            onReward: onRewardEarned,
            onClosed: () =>
            {
                ResumeGame();
                onAdClosed?.Invoke();
            }
        );
    }
}