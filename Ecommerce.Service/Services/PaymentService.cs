using Ecommerce.core.DTos.Order;
using Ecommerce.core.DTos.Payments;
using Ecommerce.core.interfaces;
using Ecommerce.core.interfaces.Services;
using Ecommerce_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Service.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentDto?> GetPaymentByOrderIdAsync(
        int orderId, int userId, bool isAdmin)
    {
        var query = _unitOfWork.Payments
            .GetAllQueryable()
            .Include(x => x.Order)
            .Where(x =>
                x.OrderId == orderId &&
                !x.IsDeleted);

        
        if (!isAdmin)
            query = query.Where(x => x.Order.UserId == userId);

        var payment = await query.FirstOrDefaultAsync();

        if (payment == null)
            return null;

        return MapToDto(payment);
    }

    public async Task<PaymentDto?> CreatePaymentAsync(
     int userId, CreatePaymentDto dto)
    {
        
        using var transaction = await _unitOfWork
            .BeginTransactionAsync();

        try
        {
            
            var order = await _unitOfWork.Orders
                .GetAllQueryable()
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.OrderId &&
                    x.UserId == userId &&
                    !x.IsDeleted);

            if (order == null)
                return null;

            if (order.Status != (int)OrderStatus.Pending)
                return null;

            var paymentExists = await _unitOfWork.Payments
                .GetAllQueryable()
                .AnyAsync(x =>
                    x.OrderId == dto.OrderId &&
                    !x.IsDeleted);

            if (paymentExists)
                return null;

            
            var payment = new Payment
            {
                OrderId = dto.OrderId,
                Amount = order.TotalAmount,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = (int)PaymentStatus.Completed,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Payments.AddAsync(payment);

            
            order.Status = (int)OrderStatus.Confirmed;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            
            await transaction.CommitAsync();

            payment = await _unitOfWork.Payments
                .GetAllQueryable()
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == payment.Id);

            return MapToDto(payment!);
        }
        catch
        {
            
            await transaction.RollbackAsync();
            return null;
        }
    }

    public async Task<bool> UpdatePaymentStatusAsync(
        int paymentId, UpdatePaymentStatusDto dto)
    {
        var payment = await _unitOfWork.Payments
            .GetAllQueryable()
            .FirstOrDefaultAsync(x =>
                x.Id == paymentId &&
                !x.IsDeleted);

        if (payment == null)
            return false;

        payment.PaymentStatus = (int)dto.PaymentStatus;
        payment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    
    private static PaymentDto MapToDto(Payment payment)
    {
        var status = (PaymentStatus)payment.PaymentStatus;

        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount ?? 0,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = status,
            PaymentStatusName = status.ToString(),
            TransactionDate = payment.TransactionDate,
            CreatedAt = payment.CreatedAt
        };
    }
}