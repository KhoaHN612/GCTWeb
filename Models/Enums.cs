namespace GCTWeb.Models;

public enum DiscountUnit
{
    Percentage,
    FixedAmount // Bao gồm cả VND
}

public enum VoucherApplicabilityEntity
{
    Order,
    Product,
    Category,
    Brand
}

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

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}