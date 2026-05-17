using System;
using UnityEngine;
using GoogleMobileAds.Api;

/// <summary>
/// Lớp minh họa cách sử dụng quảng cáo banner (biểu ngữ) của Google Mobile Ads.
/// </summary>
[AddComponentMenu("GoogleMobileAds/Samples/BannerViewController")]
public class BannerViewController : MonoBehaviour
{
    // Các ID đơn vị quảng cáo (Ad Unit ID) này là ID dùng để THỬ NGHIỆM (Test Ads) của Google.
    // Dùng ID test trong lúc lập trình giúp bạn không bị khóa tài khoản vì tự click vào quảng cáo thật.
#if UNITY_ANDROID
    // ID test dành cho nền tảng Android
    private const string _adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
    // ID test dành cho nền tảng iOS
    private const string _adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
    // Dành cho các nền tảng khác (không sử dụng)
    private const string _adUnitId = "unused";
#endif

    // Biến lưu trữ đối tượng quảng cáo banner của AdMob
    private BannerView _bannerView;

    /// <summary>
    /// Tạo một banner kích thước 320x50 ở vị trí trên cùng của màn hình.
    /// </summary>
    public void CreateBannerView()
    {
        Debug.Log("Đang tạo banner view.");

        // Nếu chúng ta đã tạo một banner trước đó rồi, hãy hủy cái cũ đi để tránh rò rỉ bộ nhớ.
        if(_bannerView != null)
        {
            DestroyAd();
        }

        // Khởi tạo đối tượng banner mới với: ID quảng cáo, Kích thước (AdSize.Banner là chuẩn 320x50), Vị trí (Top - Trên cùng).
        // Bạn có thể đổi AdPosition thành Bottom (Dưới cùng) tùy thiết kế game.
        _bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Bottom);

        // Đăng ký lắng nghe các sự kiện của quảng cáo (ví dụ: tải xong, click, lỗi...)
        ListenToAdEvents();

        Debug.Log("Đã tạo banner view thành công.");
    }

    /// <summary>
    /// Tạo đối tượng banner và tiến hành tải nội dung quảng cáo về.
    /// </summary>
    public void LoadAd()
    {
        // Kiểm tra xem đối tượng banner đã được tạo chưa, nếu chưa thì tạo mới.
        if(_bannerView == null)
        {
            CreateBannerView();
        }

        // Tạo một yêu cầu tải quảng cáo mặc định
        var adRequest = new AdRequest();

        // Gửi yêu cầu để bắt đầu tải quảng cáo từ server Google về
        Debug.Log("Đang tải nội dung quảng cáo banner.");
        _bannerView.LoadAd(adRequest);
        
        _bannerView.Hide();
    }

    /// <summary>
    /// Hiển thị quảng cáo banner lên màn hình.
    /// </summary>
    public void ShowAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Đang hiển thị banner.");
            _bannerView.Show();
        }
    }

    /// <summary>
    /// Ẩn quảng cáo banner đi (nhưng không xóa, lúc sau có thể Show() lại).
    /// </summary>
    public void HideAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Đang ẩn banner.");
            _bannerView.Hide();
        }
    }

    /// <summary>
    /// Xóa hoàn toàn quảng cáo banner.
    /// RẤT QUAN TRỌNG: Khi bạn chuyển scene hoặc không dùng đến banner nữa, 
    /// bạn BẮT BUỘC phải gọi hàm Destroy() để giải phóng bộ nhớ.
    /// </summary>
    public void DestroyAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Đang hủy (destroy) banner.");
            _bannerView.Destroy();
            _bannerView = null; // Xóa tham chiếu
        }
    }

    /// <summary>
    /// In ra các thông tin phản hồi từ máy chủ Google (dùng để debug).
    /// </summary>
    public void LogResponseInfo()
    {
        if (_bannerView != null)
        {
            var responseInfo = _bannerView.GetResponseInfo();
            if (responseInfo != null)
            {
                UnityEngine.Debug.Log(responseInfo);
            }
        }
    }

    /// <summary>
    /// Lắng nghe và xử lý các sự kiện do banner phát ra (Callbacks).
    /// </summary>
    private void ListenToAdEvents()
    {
        // Sự kiện: Khi một quảng cáo đã được TẢI THÀNH CÔNG vào banner.
        _bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner đã tải thành công một quảng cáo với thông tin : "
                + _bannerView.GetResponseInfo());
        };
        
        // Sự kiện: Khi tải quảng cáo THẤT BẠI (ví dụ: rớt mạng, sai ID, không có quảng cáo phù hợp).
        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner tải thất bại với lỗi : " + error);
        };
        
        // Sự kiện: Khi quảng cáo ước tính đã KIẾM ĐƯỢC TIỀN (DOANH THU).
        _bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner đã trả {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode)); // In ra số tiền và loại tiền tệ (VD: USD)
        };
        
        // Sự kiện: Khi hệ thống ghi nhận được một lượt HIỂN THỊ (Impression).
        _bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner đã ghi nhận một lượt hiển thị.");
        };
        
        // Sự kiện: Khi người dùng CLICK vào quảng cáo banner.
        _bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner đã bị click.");
        };
        
        // Sự kiện: Khi quảng cáo MỞ NỘI DUNG TOÀN MÀN HÌNH (Ví dụ click vào banner nó mở ra 1 trang web phủ kín màn hình).
        _bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner đã mở nội dung toàn màn hình.");
        };
        
        // Sự kiện: Khi người dùng ĐÓNG NỘI DUNG TOÀN MÀN HÌNH đó lại và quay về game.
        _bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner đã đóng nội dung toàn màn hình.");
        };
    }
}