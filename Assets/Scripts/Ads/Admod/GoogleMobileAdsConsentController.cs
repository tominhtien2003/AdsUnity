using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp hỗ trợ tích hợp việc thu thập sự đồng ý của người dùng (consent) 
/// sử dụng SDK Google User Messaging Platform (UMP).
/// </summary>
public class GoogleMobileAdsConsentController : MonoBehaviour
{
    /// <summary>
    /// Nếu giá trị này là true, nghĩa là đã an toàn (đã có sự đồng ý hoặc không cần) 
    /// để gọi hàm MobileAds.Initialize() và bắt đầu tải quảng cáo.
    /// </summary>
    public bool CanRequestAds => ConsentInformation.CanRequestAds();

    [SerializeField, Tooltip("Nút để hiển thị lại cài đặt quyền riêng tư cho người dùng.")]
    private Button _privacyButton;

    [SerializeField, Tooltip("GameObject chứa cửa sổ (popup) hiển thị lỗi.")]
    private GameObject _errorPopup;

    [SerializeField, Tooltip("Text dùng để hiển thị nội dung thông báo lỗi trên popup.")]
    private Text _errorText;

    private void Start()
    {
        // Ẩn popup báo lỗi khi game/app mới bắt đầu để giao diện sạch sẽ.
        if (_errorPopup != null)
        {
            _errorPopup.SetActive(false);
        }
    }

    /// <summary>
    /// Phương thức khởi động cho SDK Google UMP. 
    /// Nó sẽ chạy toàn bộ logic khởi tạo bao gồm gửi yêu cầu kiểm tra 
    /// và hiển thị các form xin phép quyền riêng tư nếu cần thiết.
    /// </summary>
    public void GatherConsent(Action<string> onComplete)
    {
        Debug.Log("Đang tiến hành thu thập sự đồng ý (consent).");

        // Cấu hình các thông số gửi lên Google để kiểm tra
        var requestParameters = new ConsentRequestParameters
        {
            // False nghĩa là người dùng không nằm trong độ tuổi vị thành niên.
            TagForUnderAgeOfConsent = false,
            
            ConsentDebugSettings = new ConsentDebugSettings
            {
                // Dùng để giả lập/test cài đặt consent theo vị trí địa lý (ví dụ: test như đang ở Châu Âu).
                DebugGeography = DebugGeography.Disabled,
                // Danh sách ID thiết bị test (những máy này sẽ nhận quảng cáo test thay vì quảng cáo thật)
                // https://developers.google.com/admob/unity/test-ads
                TestDeviceHashedIds = GoogleMobileAdsController.TestDeviceIds,
            }
        };

        // Gộp callback (hàm sẽ chạy khi mọi thứ hoàn tất) với hàm hiển thị popup lỗi.
        // Nếu có lỗi, nó sẽ tự động hiện popup lên.
        onComplete = (onComplete == null)
            ? UpdateErrorPopup
            : onComplete + UpdateErrorPopup;

        // Gửi yêu cầu lên Google để cập nhật trạng thái đồng ý hiện tại của người dùng.
        ConsentInformation.Update(requestParameters, (FormError updateError) =>
        {
            // Cập nhật trạng thái hiển thị của nút Cài đặt quyền riêng tư
            UpdatePrivacyButton();

            // Nếu việc cập nhật thông tin từ Google bị lỗi (ví dụ: mất mạng)
            if (updateError != null)
            {
                onComplete(updateError.Message); // Trả về thông báo lỗi
                return;
            }

            // Dựa vào ConsentStatus để quyết định bước tiếp theo.
            if (CanRequestAds)
            {
                // Người dùng ĐÃ đồng ý trước đó, HOẶC họ ở quốc gia KHÔNG cần phải xin phép.
                // Trả quyền kiểm soát lại cho bạn (onComplete nhận giá trị null nghĩa là thành công không có lỗi).
                onComplete(null);
                return;
            }

            // Nếu chạy đến đây nghĩa là: CHƯA có sự đồng ý VÀ bắt buộc phải hỏi xin phép.
            // Tiến hành tải và hiển thị form xin phép (popup của Google) cho người dùng.
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError showError) =>
            {
                UpdatePrivacyButton();
                if (showError != null)
                {
                    // Quá trình tải/hiển thị form bị thất bại.
                    if (onComplete != null)
                    {
                        onComplete(showError.Message); // Báo lỗi
                    }
                }
                // Quá trình hiển thị form thành công (người dùng đã chọn xong Đồng ý/Từ chối).
                else if (onComplete != null)
                {
                    onComplete(null); // Thành công
                }
            });
        });
    }

    /// <summary>
    /// Hiển thị form tùy chọn quyền riêng tư cho người dùng (Form này dùng để đổi ý).
    /// </summary>
    /// <remarks>
    /// Theo luật GDPR, ứng dụng của bạn PHẢI cho phép người dùng thay đổi 
    /// quyết định đồng ý của họ bất cứ lúc nào (thông qua nút Privacy settings trong game).
    /// </remarks>
    public void ShowPrivacyOptionsForm(Action<string> onComplete)
    {
        Debug.Log("Đang hiển thị form tùy chọn quyền riêng tư.");

        // Gộp callback với hàm hiển thị popup lỗi.
        onComplete = (onComplete == null)
            ? UpdateErrorPopup
            : onComplete + UpdateErrorPopup;

        // Tải và hiển thị bảng cho phép người dùng đổi ý định về quyền riêng tư
        ConsentForm.ShowPrivacyOptionsForm((FormError showError) =>
        {
            UpdatePrivacyButton();
            if (showError != null)
            {
                // Hiển thị form thất bại.
                if (onComplete != null)
                {
                    onComplete(showError.Message);
                }
            }
            // Hiển thị form thành công.
            else if (onComplete != null)
            {
                onComplete(null);
            }
        });
    }

    /// <summary>
    /// Xóa toàn bộ thông tin/lịch sử về sự đồng ý của người dùng trên thiết bị này.
    /// (Thường dùng cho Lập trình viên để Test xem bảng xin phép có hiện ra đúng không).
    /// </summary>
    public void ResetConsentInformation()
    {
        ConsentInformation.Reset();
        UpdatePrivacyButton();
    }

    // Hàm nội bộ: Dùng để cập nhật trạng thái của nút "Cài đặt quyền riêng tư"
    void UpdatePrivacyButton()
    {
        if (_privacyButton != null)
        {
            // ExecuteInUpdate giúp đoạn code này chạy an toàn trên luồng chính (Main Thread) của Unity
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
               // Nút chỉ có thể bấm được (interactable = true) NẾU luật ở khu vực của người dùng
               // BẮT BUỘC phải cung cấp tùy chọn thay đổi quyền riêng tư.
               _privacyButton.interactable =
                   ConsentInformation.PrivacyOptionsRequirementStatus ==
                       PrivacyOptionsRequirementStatus.Required;
            });
        }
    }

    // Hàm nội bộ: Xử lý việc hiển thị popup lỗi lên màn hình
    void UpdateErrorPopup(string message)
    {
        // ExecuteInUpdate giúp tương tác với UI an toàn trên luồng chính (Main Thread)
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            // Nếu không có lỗi gì cả thì bỏ qua, không làm gì hết.
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            // Điền tin nhắn lỗi vào Text
            if (_errorText != null)
            {
                _errorText.text = message;
            }
            // Bật Popup lỗi lên màn hình
            if (_errorPopup != null)
            {
                _errorPopup.SetActive(true);
            }
            // Nếu đang hiện lỗi, đảm bảo nút Privacy vẫn bấm được để họ thử lại sau
            if (_privacyButton != null)
            {
                _privacyButton.interactable = true;
            }
        });
    }
}