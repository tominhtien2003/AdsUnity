using System;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

/// <summary>
/// Lớp minh họa cách sử dụng quảng cáo xen kẽ nhận phần thưởng (Rewarded Interstitial Ads) của Google Mobile Ads.
/// </summary>
[AddComponentMenu("GoogleMobileAds/Samples/RewardedInterstitialAdController")]
public class RewardedInterstitialAdController : MonoBehaviour
{
    // Các ID đơn vị quảng cáo (Ad Unit ID) này được cấu hình để luôn hiển thị quảng cáo THỬ NGHIỆM (Test Ads).
#if UNITY_ANDROID
    // ID test dành cho nền tảng Android
    private const string _adUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
    // ID test dành cho nền tảng iOS
    private const string _adUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
    // Dành cho các nền tảng khác (không sử dụng)
    private const string _adUnitId = "unused";
#endif

    // Biến lưu trữ đối tượng quảng cáo xen kẽ nhận phần thưởng của AdMob
    private RewardedInterstitialAd _rewardedInterstitialAd;
    private Action _onRewardEarnedCallback;
    private Action _onAdClosedCallback;
    /// <summary>
    /// Tải nội dung quảng cáo về.
    /// </summary>
    public void LoadAd()
    {
        // Dọn dẹp quảng cáo cũ trước khi tải một quảng cáo mới để tránh rò rỉ bộ nhớ.
        if (_rewardedInterstitialAd != null)
        {
            DestroyAd();
        }

        Debug.Log("Đang tải quảng cáo xen kẽ nhận phần thưởng (Rewarded Interstitial Ad).");

        // Tạo một yêu cầu tải quảng cáo mặc định.
        var adRequest = new AdRequest();

        // Gửi yêu cầu để bắt đầu tải quảng cáo từ server Google về.
        RewardedInterstitialAd.Load(_adUnitId, adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                // Nếu quá trình tải thất bại kèm theo lý do lỗi cụ thể.
                if (error != null)
                {
                    Debug.LogError("Quảng cáo xen kẽ nhận phần thưởng tải thất bại với lỗi: "
                                    + error);
                    return;
                }
                
                // Nếu quá trình tải thất bại vì những lý do không xác định.
                // Đây là một lỗi ngoài ý muốn, hãy báo cáo lỗi này nếu nó xảy ra.
                if (ad == null)
                {
                    Debug.LogError("Lỗi ngoài ý muốn: Sự kiện tải quảng cáo xen kẽ nhận phần thưởng bị kích hoạt nhưng cả đối tượng quảng cáo và lỗi đều bị null.");
                    return;
                }

                // Quá trình tải quảng cáo đã hoàn thành thành công.
                Debug.Log("Quảng cáo xen kẽ nhận phần thưởng đã tải thành công với thông tin phản hồi: "
                    + ad.GetResponseInfo());
                _rewardedInterstitialAd = ad;

                // Đăng ký các sự kiện của quảng cáo để mở rộng các tính năng xử lý.
                RegisterEventHandlers(ad);
            });
    }

    /// <summary>
    /// Hiển thị quảng cáo lên màn hình.
    /// </summary>
    public void ShowAd(Action onReward, Action onClosed)
    {
        // Kiểm tra xem đối tượng quảng cáo đã tồn tại chưa và đã sẵn sàng để hiển thị hay không.
        if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
        {
            _onRewardEarnedCallback = onReward;
            _onAdClosedCallback = onClosed;
            
            // Kích hoạt hiển thị quảng cáo. Hàm Show nhận vào một hàm callback để xử lý phần thưởng khi người dùng xem xong.
            _rewardedInterstitialAd.Show((Reward reward) =>
            {
                // ĐOẠN QUAN TRỌNG NHẤT: Viết code cộng vàng, quà hoặc vật phẩm cho người chơi tại đây.
                Debug.Log("Quảng cáo xen kẽ nhận phần thưởng đã trao quà thành công, số lượng: " + reward.Amount);
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    _onRewardEarnedCallback?.Invoke();
                    _onRewardEarnedCallback = null;
                });
            });
        }
        else
        {
            Debug.LogError("Quảng cáo xen kẽ nhận phần thưởng chưa sẵn sàng để hiển thị.");
            onClosed?.Invoke();
            LoadAd();
        }
    }

    /// <summary>
    /// Xóa hoàn toàn quảng cáo để giải phóng bộ nhớ.
    /// </summary>
    public void DestroyAd()
    {
        if (_rewardedInterstitialAd != null)
        {
            Debug.Log("Đang hủy (destroy) quảng cáo xen kẽ nhận phần thưởng.");
            _rewardedInterstitialAd.Destroy();
            _rewardedInterstitialAd = null; // Xóa tham chiếu
        }
    }

    /// <summary>
    /// In ra các thông tin phản hồi từ máy chủ Google (dùng để debug).
    /// </summary>
    public void LogResponseInfo()
    {
        if (_rewardedInterstitialAd != null)
        {
            var responseInfo = _rewardedInterstitialAd.GetResponseInfo();
            UnityEngine.Debug.Log(responseInfo);
        }
    }

    /// <summary>
    /// Lắng nghe và xử lý các sự kiện do quảng cáo phát ra (Callbacks).
    /// </summary>
    protected void RegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // Sự kiện: Khi quảng cáo ước tính đã KIẾM ĐƯỢC TIỀN (DOANH THU).
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Quảng cáo xen kẽ nhận phần thưởng đã trả {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode)); // In ra số tiền và loại tiền tệ (VD: USD)
        };
        
        // Sự kiện: Khi hệ thống ghi nhận được một lượt HIỂN THỊ (Impression).
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Quảng cáo xen kẽ nhận phần thưởng đã ghi nhận một lượt hiển thị.");
        };
        
        // Sự kiện: Khi người dùng CLICK vào quảng cáo.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Quảng cáo xen kẽ nhận phần thưởng đã bị click.");
        };
        
        // Sự kiện: Khi quảng cáo MỞ NỘI DUNG TOÀN MÀN HÌNH (phủ kín màn hình game).
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Quảng cáo xen kẽ nhận phần thưởng đã mở nội dung toàn màn hình.");
        };
        
        // Sự kiện: Khi người dùng ĐÓNG NỘI DUNG TOÀN MÀN HÌNH đó lại để quay về game.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Quảng cáo xen kẽ nhận phần thưởng đã đóng nội dung toàn màn hình.");
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                _onAdClosedCallback?.Invoke();
                _onAdClosedCallback = null;
                LoadAd(); 
            });
        };
        
        // Sự kiện: Khi quảng cáo THẤT BẠI trong việc mở nội dung toàn màn hình.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Quảng cáo xen kẽ nhận phần thưởng thất bại khi mở nội dung toàn màn hình với lỗi: " + error);
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                _onAdClosedCallback?.Invoke();
                _onAdClosedCallback = null;
                LoadAd();
            });
        };
    }
}