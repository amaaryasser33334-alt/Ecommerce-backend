using Ecommerce.core.DTos.Order;
using Ecommerce.core.DTos.Shipping;
using Ecommerce.core.Enums;
using Ecommerce.core.interfaces;
using Ecommerce.core.interfaces.Services;
using Ecommerce_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Service.Services;

public class ShippingService : IShippingService
{
    private readonly IUnitOfWork _unitOfWork;

    public ShippingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ShippingDto?> GetShippingByOrderIdAsync(
        int orderId, int userId, bool isAdmin)
    {
        // ✅ بنبني الـ Query
        var query = _unitOfWork.Shippings
            .GetAllQueryable()
            .Include(x => x.Order) // محتاجينه عشان نتحقق UserId
            .Where(x => x.OrderId == orderId);

        // ✅ لو مش Admin — يشوف Orders بتاعته بس
        if (!isAdmin)
            query = query.Where(x => x.Order.UserId == userId);

        // ✅ هنا بس بنكلم الـ DB
        var shipping = await query.FirstOrDefaultAsync();

        if (shipping == null)
            return null;

        return MapToDto(shipping);
    }

    public async Task<ShippingDto?> CreateShippingAsync(
        CreateShippingDto dto)
    {
        // ✅ Step 1 — تحقق إن الـ Order موجود
        var order = await _unitOfWork.Orders
            .GetAllQueryable()
            .FirstOrDefaultAsync(x =>
                x.Id == dto.OrderId &&
                !x.IsDeleted);

        if (order == null)
            return null;

        // ✅ Step 2 — تحقق إن الـ Order اتدفع
        // مش منطقي تشحن Order لسه Pending
        if (order.Status != (int)OrderStatus.Confirmed)
            return null;

        // ✅ Step 3 — تحقق مفيش Shipping موجود بالفعل
        // DB عندها UNIQUE constraint على OrderId
        var shippingExists = await _unitOfWork.Shippings
            .GetAllQueryable()
            .AnyAsync(x => x.OrderId == dto.OrderId);

        if (shippingExists)
            return null;

        // ✅ Step 4 — اعمل الـ Shipping
        var shipping = new Shipping
        {
            OrderId = dto.OrderId,
            CarrierName = dto.CarrierName,
            TrackingNumber = dto.TrackingNumber,
            ShippingStatus = (int)ShippingStatus.Preparing,
            EstimatedDeliveryDate = dto.EstimatedDeliveryDate
        };

        await _unitOfWork.Shippings.AddAsync(shipping);

        // ✅ Step 5 — غير Order Status لـ Shipped
        order.Status = (int)OrderStatus.Shipped;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        // ✅ Step 6 — Reload عشان نجيب الـ Order navigation
        shipping = await _unitOfWork.Shippings
            .GetAllQueryable()
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == shipping.Id);

        return MapToDto(shipping!);
    }

    public async Task<bool> UpdateShippingAsync(
        int shippingId, UpdateShippingDto dto)
    {
        // ✅ محتاجين الـ Order عشان نغير Status بتاعه
        var shipping = await _unitOfWork.Shippings
            .GetAllQueryable()
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == shippingId);

        if (shipping == null)
            return false;

        // ✅ غير الـ Shipping Status
        shipping.ShippingStatus = (int)dto.ShippingStatus;

        if (dto.TrackingNumber != null)
            shipping.TrackingNumber = dto.TrackingNumber;

        if (dto.ActualDeliveryDate.HasValue)
            shipping.ActualDeliveryDate = dto.ActualDeliveryDate;

        // ✅ لو وصل — غير الـ Order Status لـ Delivered
        if (dto.ShippingStatus == ShippingStatus.Delivered)
        {
            shipping.Order.Status = (int)OrderStatus.Delivered;
            shipping.Order.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ✅ Helper — بنستخدمه في كل method
    private static ShippingDto MapToDto(Shipping shipping)
    {
        var status = (ShippingStatus)shipping.ShippingStatus;

        return new ShippingDto
        {
            Id = shipping.Id,
            OrderId = shipping.OrderId,
            CarrierName = shipping.CarrierName,
            TrackingNumber = shipping.TrackingNumber,
            ShippingStatus = status,
            ShippingStatusName = status.ToString(),
            EstimatedDeliveryDate = shipping.EstimatedDeliveryDate,
            ActualDeliveryDate = shipping.ActualDeliveryDate
        };
    }
}