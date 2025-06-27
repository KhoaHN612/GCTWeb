namespace GCTWeb.Models.ViewModels;

public class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();

    // Thông tin thanh toán (Billing)
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [Display(Name = "Họ và tên")]
    [StringLength(100)]
    public string BillingFullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [Display(Name = "Số điện thoại")]
    public string BillingPhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    [Display(Name = "Địa chỉ email")]
    public string BillingEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố.")]
    [Display(Name = "Tỉnh/Thành phố")]
    public string BillingState { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện.")]
    [Display(Name = "Quận/Huyện")]
    public string BillingCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn Phường/Xã.")]
    [Display(Name = "Phường/Xã")]
    public string BillingWard { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ (số nhà, tên đường).")]
    [Display(Name = "Địa chỉ (Số nhà, tên đường)")]
    [StringLength(255)]
    public string BillingAddress1 { get; set; } = string.Empty;

    // Thông tin giao hàng (tùy chọn)
    public bool ShipToDifferentAddress { get; set; }

    // Ghi chú
    [Display(Name = "Ghi chú đơn hàng")] public string? OrderNotes { get; set; }

    // Lựa chọn
    [Required(ErrorMessage = "Vui lòng chọn phương thức vận chuyển.")]
    public string ShippingMethod { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
    public string PaymentMethod { get; set; } = string.Empty;
}