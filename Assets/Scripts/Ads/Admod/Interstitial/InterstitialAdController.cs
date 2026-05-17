using System;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

/// <summary>
/// Lớp minh họa cách sử dụng quảng cáo xen kẽ (Interstitial Ads) của Google Mobile Ads.
/// </summary>
[AddComponentMenu("GoogleMobileAds/Samples/InterstitialAdController")]
public class InterstitialAdController : MonoBehaviour
{
    // Các ID đơn vị quảng cáo (Ad Unit ID) này được cấu hình để luôn hiển thị quảng cáo THỬ NGHIỆM (Test Ads).
#if UNITY_ANDROID
    // ID test dành cho nền tảng Android
    private const string _adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
    // ID test dành cho nền tảng iOS
    private const string _adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
    // Dành cho các nền tảng khác (không sử dụng)
    private const string _adUnitId = "unused";
#endif

    // Biến lưu trữ đối tượng quảng cáo xen kẽ của AdMob
    private InterstitialAd _interstitialAd;
    private Action _onAdClosedCallback;
    /// <summary>
    /// Tải nội dung quảng cáo về.
    /// </summary>
    public void LoadAd()
    {
        // Dọn dẹp quảng cáo cũ trước khi tải một quảng cáo mới để tránh rò rỉ bộ nhớ.
        if (_interstitialAd != null)
        {
            DestroyAd();
        }

        Debug.Log("Đang tải quảng cáo xen kẽ (Interstitial Ad).");

        // Tạo một yêu cầu tải quảng cáo mặc định.
        var adRequest = new AdRequest();

        // Gửi yêu cầu để bắt đầu tải quảng cáo từ server Google về.
        InterstitialAd.Load(_adUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            // Nếu quá trình tải thất bại kèm theo lý do lỗi cụ thể.
            if (error != null)
            {
                Debug.LogError("Quảng cáo xen kẽ tải thất bại với lỗi: " + error);
                return;
            }
            
            // Nếu quá trình tải thất bại vì những lý do không xác định.
            // Đây là một lỗi ngoài ý muốn, hãy báo cáo lỗi này nếu nó xảy ra.
            if (ad == null)
            {
                Debug.LogError("Lỗi ngoài ý muốn: Sự kiện tải quảng cáo xen kẽ bị kích hoạt nhưng cả đối tượng quảng cáo và lỗi đều bị null.");
                return;
            }

            // Quá trình tải quảng cáo đã hoàn thành thành công.
            Debug.Log("Quảng cáo xen kẽ đã tải thành công với thông tin phản hồi: " + ad.GetResponseInfo());
            _interstitialAd = ad;

            // Đăng ký các sự kiện của quảng cáo để mở rộng các tính năng xử lý.
            RegisterEventHandlers(ad);
        });
    }

    /// <summary>
    /// Hiển thị quảng cáo lên màn hình.
    /// </summary>
    public void ShowAd(Action onClosed)
    {
        // Kiểm tra xem đối tượng quảng cáo đã tồn tại chưa và đã sẵn sàng để hiển thị hay không.
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Đang hiển thị quảng cáo xen kẽ.");
            _onAdClosedCallback = onClosed;
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Quảng cáo xen kẽ chưa sẵn sàng để hiển thị.");
            onClosed?.Invoke(); // Bỏ qua quảng cáo, cho game chạy tiếp
            LoadAd();
        }
    }

    /// <summary>
    /// Xóa hoàn toàn quảng cáo để giải phóng bộ nhớ.
    /// </summary>
    public void DestroyAd()
    {
        if (_interstitialAd != null)
        {
            Debug.Log("Đang hủy (destroy) quảng cáo xen kẽ.");
            _interstitialAd.Destroy();
            _interstitialAd = null; // Xóa tham chiếu
        }
    }

    /// <summary>
    /// In ra các thông tin phản hồi từ máy chủ Google (dùng để debug).
    /// </summary>
    public void LogResponseInfo()
    {
        if (_interstitialAd != null)
        {
            var responseInfo = _interstitialAd.GetResponseInfo();
            UnityEngine.Debug.Log(responseInfo);
        }
    }

    /// <summary>
    /// Lắng nghe và xử lý các sự kiện do quảng cáo phát ra (Callbacks).
    /// </summary>
    private void RegisterEventHandlers(InterstitialAd ad)
    {
        // Sự kiện: Khi quảng cáo ước tính đã KIẾM ĐƯỢC TIỀN (DOANH THU).
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Quảng cáo xen kẽ đã trả {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode)); // In ra số tiền và loại tiền tệ (VD: USD)
        };
        
        // Sự kiện: Khi hệ thống ghi nhận được một lượt HIỂN THỊ (Impression).
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Quảng cáo xen kẽ đã ghi nhận một lượt hiển thị.");
        };
        
        // Sự kiện: Khi người dùng CLICK vào quảng cáo.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Quảng cáo xen kẽ đã bị click.");
        };
        
        // Sự kiện: Khi quảng cáo MỞ NỘI DUNG TOÀN MÀN HÌNH (phủ kín màn hình game).
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Quảng cáo xen kẽ đã mở nội dung toàn màn hình.");
        };
        
        // Sự kiện: Khi người dùng ĐÓNG NỘI DUNG TOÀN MÀN HÌNH đó lại (bấm nút X) để quay về game.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Quảng cáo xen kẽ đã đóng nội dung toàn màn hình.");
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                _onAdClosedCallback?.Invoke();
                _onAdClosedCallback = null;
                LoadAd(); // Tự động load đạn mới
            });
        };
        
        // Sự kiện: Khi quảng cáo THẤT BẠI trong việc mở nội dung toàn màn hình.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Quảng cáo xen kẽ thất bại khi mở nội dung toàn màn hình với lỗi: "
                + error);
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                _onAdClosedCallback?.Invoke();
                _onAdClosedCallback = null;
                LoadAd(); 
            });
        };
    }
}