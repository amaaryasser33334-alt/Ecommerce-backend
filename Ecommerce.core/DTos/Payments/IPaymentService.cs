using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.Payments
{
    public interface IPaymentService
    {
        // Owner أو Admin يشوف Payment بتاع Order
        Task<PaymentDto?> GetPaymentByOrderIdAsync(
            int orderId, int userId, bool isAdmin);

        // Customer يدفع
        Task<PaymentDto?> CreatePaymentAsync(
            int userId, CreatePaymentDto dto);

        // Admin يعدل Status
        Task<bool> UpdatePaymentStatusAsync(
            int paymentId, UpdatePaymentStatusDto dto);
    }
}
