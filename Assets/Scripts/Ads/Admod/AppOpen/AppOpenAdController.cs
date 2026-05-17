using System;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

/// <summary>
/// Lớp minh họa cách sử dụng quảng cáo mở ứng dụng (App Open Ads) của Google Mobile Ads.
/// </summary>
[AddComponentMenu("GoogleMobileAds/Samples/AppOpenAdController")]
public class AppOpenAdController : MonoBehaviour
{
    // Các ID đơn vị quảng cáo (Ad Unit ID) này được cấu hình để luôn hiển thị quảng cáo THỬ NGHIỆM (Test Ads).
#if UNITY_ANDROID
    // ID test dành cho nền tảng Android
    private const string _adUnitId = "ca-app-pub-3940256099942544/9257395921";
#elif UNITY_IPHONE
    // ID test dành cho nền tảng iOS
    private const string _adUnitId = "ca-app-pub-3940256099942544/5575463023";
#else
    // Dành cho các nền tảng khác (không sử dụng)
    private const string _adUnitId = "unused";
#endif

    // Quảng cáo mở ứng dụng có thể được tải trước (preload) và lưu trữ tối đa trong 4 giờ.
    private static readonly TimeSpan TIMEOUT = TimeSpan.FromHours(4);
    private DateTime _expireTime;
    private AppOpenAd _appOpenAd;
    
    /// <summary>
    /// Kiểm tra ad hiện tại có thể show không.
    /// Bao gồm: object tồn tại, chưa show, chưa hết hạn.
    /// </summary>
    public bool IsAdAvailable()
    {
        return _appOpenAd != null &&
               _appOpenAd.CanShowAd() &&
               DateTime.Now < _expireTime;
    }
    private void Awake()
    {
        // Sử dụng AppStateEventNotifier để lắng nghe các sự kiện bật/tắt (mở/đóng) của ứng dụng.
        // Điều này được dùng để kích hoạt hiển thị quảng cáo đã tải khi người dùng mở lại app.
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }

    private void OnDestroy()
    {
        // Luôn luôn hủy đăng ký lắng nghe sự kiện khi đối tượng bị hủy để tránh rò rỉ bộ nhớ.
        AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
        DestroyAd();
    }

    /// <summary>
    /// Tải nội dung quảng cáo về.
    /// </summary>
    public void LoadAd()
    {
        // Dọn dẹp quảng cáo cũ trước khi tải một quảng cáo mới.
        if (_appOpenAd != null)
        {
            DestroyAd();
        }

        Debug.Log("Đang tải quảng cáo mở ứng dụng (App Open Ad).");

        // Tạo một yêu cầu tải quảng cáo mặc định.
        var adRequest = new AdRequest();

        // Gửi yêu cầu để bắt đầu tải quảng cáo từ server Google về.
        AppOpenAd.Load(_adUnitId, adRequest, (AppOpenAd ad, LoadAdError error) =>
        {
            // Nếu quá trình tải thất bại kèm theo lý do lỗi cụ thể.
            if (error != null)
            {
                Debug.LogError("Quảng cáo mở ứng dụng tải thất bại với lỗi: "
                               + error);
                return;
            }

            // Nếu quá trình tải thất bại vì những lý do không xác định.
            // Đây là một lỗi ngoài ý muốn, hãy báo cáo lỗi này nếu nó xảy ra.
            if (ad == null)
            {
                Debug.LogError("Lỗi ngoài ý muốn: Sự kiện tải quảng cáo mở ứng dụng bị kích hoạt " +
                               "nhưng cả đối tượng quảng cáo và lỗi đều bị null.");
                return;
            }

            // Quá trình tải quảng cáo đã hoàn thành thành công.
            Debug.Log("Quảng cáo mở ứng dụng đã tải thành công với thông tin phản hồi: " + ad.GetResponseInfo());
            _appOpenAd = ad;

            // Quảng cáo mở ứng dụng có thể lưu trữ tối đa trong 4 giờ. Tính toán thời gian hết hạn.
            _expireTime = DateTime.Now + TIMEOUT;

            // Đăng ký các sự kiện của quảng cáo để mở rộng các tính năng xử lý.
            RegisterEventHandlers(ad);
        });
    }

    /// <summary>
    /// Hiển thị quảng cáo lên màn hình.
    /// </summary>
    public void ShowAd()
    {
        // Quảng cáo mở ứng dụng có thể được tải trước tối đa 4 giờ.
        // Kiểm tra xem quảng cáo đã tồn tại chưa, có sẵn sàng hiển thị không và đã bị quá hạn 4 giờ chưa.
        if (_appOpenAd != null && _appOpenAd.CanShowAd() && DateTime.Now < _expireTime)
        {
            // Báo cho AdManager biết AppOpenAd đang chiếm màn hình để Pause game
            if (AdManager.Instance != null) AdManager.Instance.PauseGame();
            
            Debug.Log("Đang hiển thị quảng cáo mở ứng dụng.");
            _appOpenAd.Show();
        }
        else
        {
            Debug.LogError("Quảng cáo mở ứng dụng chưa sẵn sàng để hiển thị.");
        }
    }

    /// <summary>
    /// Xóa hoàn toàn quảng cáo để giải phóng bộ nhớ.
    /// </summary>
    public void DestroyAd()
    {
        if (_appOpenAd != null)
        {
            Debug.Log("Đang hủy (destroy) quảng cáo mở ứng dụng.");
            _appOpenAd.Destroy();
            _appOpenAd = null; // Xóa tham chiếu
        }
    }

    /// <summary>
    /// In ra các thông tin phản hồi từ máy chủ Google (dùng để debug).
    /// </summary>
    public void LogResponseInfo()
    {
        if (_appOpenAd != null)
        {
            var responseInfo = _appOpenAd.GetResponseInfo();
            UnityEngine.Debug.Log(responseInfo);
        }
    }

    private void OnAppStateChanged(AppState state)
    {
        Debug.Log("Trạng thái ứng dụng thay đổi thành: " + state);
        
        // BẢO VỆ: Nếu game đang show Rewarded/Interstitial, TUYỆT ĐỐI không pop AppOpenAd lên đè vào.
        if (AdManager.Instance != null && AdManager.Instance.IsShowingFullScreenAd)
        {
            return; 
        }
        // Nếu ứng dụng được đưa lên chạy nền trước (Foreground - người dùng vừa mở lại app) và quảng cáo đã sẵn sàng, hãy hiển thị nó.
        if (state == AppState.Foreground)
        {
            ShowAd();
        }
    }

    /// <summary>
    /// Lắng nghe và xử lý các sự kiện do quảng cáo phát ra (Callbacks).
    /// </summary>
    private void RegisterEventHandlers(AppOpenAd ad)
    {
        // Sự kiện: Khi quảng cáo ước tính đã KIẾM ĐƯỢC TIỀN (DOANH THU).
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Quảng cáo mở ứng dụng đã trả {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode)); // In ra số tiền và loại tiền tệ (VD: USD)
        };
        
        // Sự kiện: Khi hệ thống ghi nhận được một lượt HIỂN THỊ (Impression).
        ad.OnAdImpressionRecorded += () => { Debug.Log("Quảng cáo mở ứng dụng đã ghi nhận một lượt hiển thị."); };
        
        // Sự kiện: Khi người dùng CLICK vào quảng cáo.
        ad.OnAdClicked += () => { Debug.Log("Quảng cáo mở ứng dụng đã bị click."); };
        
        // Sự kiện: Khi quảng cáo MỞ NỘI DUNG TOÀN MÀN HÌNH.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Quảng cáo mở ứng dụng đã mở nội dung toàn màn hình.");
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (AdManager.Instance != null)
                {
                    AdManager.Instance.PauseGame();
                }
            });
        };
        
        // Sự kiện: Khi người dùng ĐÓNG NỘI DUNG TOÀN MÀN HÌNH đó lại để quay về game.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Quảng cáo mở ứng dụng đã đóng nội dung toàn màn hình.");
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (AdManager.Instance != null)
                {
                    AdManager.Instance.ResumeGame();
                }
                // Sẽ rất hữu ích nếu chúng ta tự động tải một quảng cáo mới ngay khi quảng cáo hiện tại vừa đóng xong.
                LoadAd(); // Tải đạn an toàn trên Main Thread
            });
        };
        
        // Sự kiện: Khi quảng cáo THẤT BẠI trong việc mở nội dung toàn màn hình.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Quảng cáo mở ứng dụng thất bại khi mở nội dung toàn màn hình với lỗi: "
                           + error);
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                // Đảm bảo game không bị kẹt Pause
                if (AdManager.Instance != null)
                {
                    AdManager.Instance.PauseGame();
                }
                LoadAd();
            });
        };
    }
}