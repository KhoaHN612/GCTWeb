namespace GCTWeb.Models.Enums;

public enum OrderStatus
{
    Pending,
    Processing,
    PaymentFailed,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}