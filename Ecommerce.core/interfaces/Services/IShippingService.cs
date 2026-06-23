using Ecommerce.core.DTos.Shipping;

namespace Ecommerce.core.interfaces.Services;

public interface IShippingService
{
    // Customer يشوف شحنة الـ Order بتاعه
    Task<ShippingDto?> GetShippingByOrderIdAsync(
        int orderId, int userId, bool isAdmin);

    // Admin يضيف Shipping للـ Order
    Task<ShippingDto?> CreateShippingAsync(
        CreateShippingDto dto);

    // Admin يعدل Status + Tracking
    Task<bool> UpdateShippingAsync(
        int shippingId, UpdateShippingDto dto);
}