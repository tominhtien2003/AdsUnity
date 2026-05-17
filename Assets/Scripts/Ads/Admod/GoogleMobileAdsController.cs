using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;

/// <summary>
/// Lớp minh họa cách sử dụng plugin Google Mobile Ads cho Unity.
/// Lớp này quản lý việc cấu hình và khởi tạo hệ thống quảng cáo.
/// </summary>
///
/// [AddComponentMenu("GoogleMobileAds/Samples/GoogleMobileAdsController")]
public class GoogleMobileAdsController : MonoBehaviour
{
    // Luôn luôn sử dụng quảng cáo thử nghiệm (test ads) trong quá trình phát triển 
    // để tránh bị Google khóa tài khoản vì tự click vào quảng cáo thật.
    // https://developers.google.com/admob/unity/test-ads
    internal static List<string> TestDeviceIds = new List<string>()
    {
        // Thêm ID của máy ảo (Simulator)
        AdRequest.TestDeviceSimulator,
#if UNITY_IPHONE
        // Thêm ID của thiết bị iOS thật để test
        "96e23e80653bb28980d3f40beb58915c",
#elif UNITY_ANDROID
        // Thêm ID của thiết bị Android thật để test
        "702815ACFC14FF222DA1DC767672A573"
#endif
    };

    // Biến lưu trạng thái khởi tạo của SDK Google Mobile Ads.
    // SDK này chỉ cần (và chỉ được) khởi tạo MỘT LẦN duy nhất.
    private static bool? _isInitialized;

    // Tham chiếu đến lớp quản lý quyền riêng tư (UMP) mà chúng ta đã xem xét ở trước.
    [SerializeField, Tooltip("Controller quản lý plugin Google User Messaging Platform (UMP).")]
    private GoogleMobileAdsConsentController _consentController;

    /// <summary>
    /// Hàm Start chạy khi game bắt đầu. Minh họa cách cấu hình Google Mobile Ads.
    /// </summary>
    private void Start()
    {
        // Trên Android, Unity mặc định bị tạm dừng (pause) khi hiển thị quảng cáo toàn màn hình 
        // (interstitial) hoặc quảng cáo video tặng thưởng (rewarded video).
        // Cài đặt này giúp iOS cũng có hành vi tạm dừng tương tự như Android để tránh lỗi logic game.
        MobileAds.SetiOSAppPauseOnBackground(true);

        // Cấu hình RequestConfiguration cho quảng cáo.
        // Tại đây bạn thiết lập danh sách các thiết bị test.
        MobileAds.SetRequestConfiguration(new RequestConfiguration
        {
            TestDeviceIds = TestDeviceIds
        });

        // NẾU chúng ta ĐÃ có thể yêu cầu quảng cáo (ví dụ: người dùng đã đồng ý từ lần chơi trước, 
        // hoặc ở quốc gia không yêu cầu quyền riêng tư), ta tiến hành khởi tạo AdMob ngay lập tức.
        if (_consentController.CanRequestAds)
        {
            InitializeGoogleMobileAds();
        }

        // Bất kể đã khởi tạo được quảng cáo ở trên hay chưa, 
        // luôn gọi hàm này để đảm bảo thông tin về quyền riêng tư được cập nhật mới nhất từ Google.
        InitializeGoogleMobileAdsConsent();
    }

    /// <summary>
    /// Đảm bảo thông tin về sự đồng ý quyền riêng tư được cập nhật.
    /// </summary>
    private void InitializeGoogleMobileAdsConsent()
    {
        Debug.Log("Google Mobile Ads đang thu thập sự đồng ý (consent).");

        // Gọi hàm GatherConsent từ script ConsentController
        _consentController.GatherConsent((string error) =>
        {
            // Nếu quá trình xin phép bị lỗi
            if (error != null)
            {
                Debug.LogError("Thu thập sự đồng ý thất bại với lỗi: " + error);
            }
            else
            {
                // Nếu thành công (người dùng đã chọn xong hoặc không cần chọn)
                Debug.Log("Sự đồng ý của Google Mobile Ads đã được cập nhật: "
                    + ConsentInformation.ConsentStatus);
            }

            // Sau khi quá trình xin phép hoàn tất, kiểm tra lại xem ĐÃ ĐƯỢC PHÉP tải quảng cáo chưa.
            // Nếu được phép, tiến hành khởi tạo SDK AdMob (nếu nó chưa được khởi tạo ở hàm Start).
            if (_consentController.CanRequestAds)
            {
                InitializeGoogleMobileAds();
            }
        });
    }

    /// <summary>
    /// Khởi tạo plugin Google Mobile Ads.
    /// </summary>
    private void InitializeGoogleMobileAds()
    {
        // Nếu biến này đã có giá trị (false là đang khởi tạo, true là đã khởi tạo xong), 
        // thì thoát hàm để không chạy khởi tạo lại lần thứ 2.
        if (_isInitialized.HasValue)
        {
            return;
        }

        // Đánh dấu là đang trong quá trình khởi tạo
        _isInitialized = false;

        Debug.Log("Google Mobile Ads đang khởi tạo.");

        // Bắt đầu quá trình khởi tạo SDK AdMob
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            // Nếu trạng thái khởi tạo bị null (lỗi nghiêm trọng)
            if (initstatus == null)
            {
                Debug.LogError("Google Mobile Ads khởi tạo thất bại.");
                _isInitialized = null; // Reset lại để có thể thử lại sau
                return;
            }

            // NẾU BẠN CÓ DÙNG MEDIATION (Phân phối quảng cáo qua trung gian - ví dụ kết hợp cả Unity Ads, AppLovin, Meta Ads...)
            // Đoạn này giúp bạn kiểm tra xem từng mạng quảng cáo trung gian đó đã khởi tạo thành công hay chưa.
            var adapterStatusMap = initstatus.getAdapterStatusMap();
            if (adapterStatusMap != null)
            {
                foreach (var item in adapterStatusMap)
                {
                    Debug.Log(string.Format("Adapter trung gian {0} đang ở trạng thái {1}",
                        item.Key,
                        item.Value.InitializationState));
                }
            }

            Debug.Log("Google Mobile Ads khởi tạo hoàn tất.");
            _isInitialized = true; // Đánh dấu là đã khởi tạo thành công!

            // LƯU Ý TỪ GOOGLE:
            // Các sự kiện của Google Mobile Ads (như báo quảng cáo đã tải xong, quảng cáo đã tắt...) 
            // được kích hoạt ở một luồng (thread) chạy ngầm, KHÔNG PHẢI luồng chính (Main Thread) của Unity.
            // Nên nếu bạn muốn tương tác với các object của Unity (ví dụ Update điểm số, tắt/bật UI...) 
            // bên trong các sự kiện quảng cáo, bạn PHẢI dùng hàm MobileAdsEventExecutor.ExecuteInUpdate().
            
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (AdManager.Instance != null)
                {
                    AdManager.Instance.InitializeAdsSystem();
                }
            });
        });
    }

    /// <summary>
    /// Mở AdInspector (Trình kiểm tra quảng cáo).
    /// Đây là một công cụ rất hữu ích của Google giúp bạn debug/kiểm tra 
    /// xem tại sao quảng cáo không hiện, hoặc mạng mediation nào đang bị lỗi.
    /// </summary>
    public void OpenAdInspector()
    {
        Debug.Log("Đang mở trình kiểm tra quảng cáo (Ad Inspector).");
        MobileAds.OpenAdInspector((AdInspectorError error) =>
        {
            // Nếu có lỗi khi mở trình kiểm tra
            if (error != null)
            {
                Debug.Log("Mở Ad Inspector thất bại với lỗi: " + error);
                return;
            }

            Debug.Log("Mở Ad Inspector thành công.");
        });
    }

    /// <summary>
    /// Mở form tùy chọn quyền riêng tư cho người dùng.
    /// </summary>
    /// <remarks>
    /// Ứng dụng của bạn phải có 1 nút "Cài đặt quyền riêng tư" trong game, 
    /// và gọi hàm này khi người dùng bấm vào nút đó để họ thay đổi ý định (theo luật GDPR).
    /// </remarks>
    public void OpenPrivacyOptions()
    {
        // Gọi hàm hiển thị form từ script ConsentController
        _consentController.ShowPrivacyOptionsForm((string error) =>
        {
            if (error != null)
            {
                Debug.LogError("Hiển thị form quyền riêng tư thất bại với lỗi: " + error);
            }
            else
            {
                Debug.Log("Mở form quyền riêng tư thành công.");
            }
        });
    }
}