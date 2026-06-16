using Ecommerce.core.DTos.common;
using Ecommerce.core.DTos.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.interfaces.Services
{
    public interface IOrderService
    {
        // Admin — كل الـ Orders
        Task<PagedResponse<OrderDto>> GetAllOrdersAsync(
            OrderQueryParameters parameters);

        // Customer — Orders بتاعته بس
        Task<PagedResponse<OrderDto>> GetMyOrdersAsync(
            int userId, OrderQueryParameters parameters);

        // أي حد يشوف Order معين — بس لو هو صاحبه أو Admin
        Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId, bool isAdmin);

        // Customer — يعمل Order من الـ Cart
        Task<OrderDto?> PlaceOrderAsync(int userId, PlaceOrderDto dto);

        // Admin — يعدل Status
        Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto);

        // Customer — يلغي Order لو لسه Pending
        Task<bool> CancelOrderAsync(int orderId, int userId);
    }
}
