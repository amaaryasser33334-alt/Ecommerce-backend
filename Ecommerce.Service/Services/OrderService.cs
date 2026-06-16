using Ecommerce.core.DTos.common;
using Ecommerce.core.DTos.Order;
using Ecommerce.core.interfaces;
using Ecommerce.core.interfaces.Services;
using Ecommerce_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Service.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<OrderDto>> GetAllOrdersAsync(
        OrderQueryParameters parameters)
    {
        var query = _unitOfWork.Orders
            .GetAllQueryable()
            .Include(x => x.User)
            .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
            .Where(x => !x.IsDeleted);

        // ✅ Filter by status
        if (parameters.Status.HasValue)
            query = query.Where(x =>
                x.Status == (int)parameters.Status.Value);

        var totalRecords = await query.CountAsync();

        var orders = await query
            .OrderByDescending(x => x.OrderDate)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => MapToOrderDto(x))
            .ToListAsync();

        return new PagedResponse<OrderDto>
        {
            Data = orders,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(
                totalRecords / (double)parameters.PageSize)
        };
    }

    public async Task<PagedResponse<OrderDto>> GetMyOrdersAsync(
        int userId, OrderQueryParameters parameters)
    {
        var query = _unitOfWork.Orders
            .GetAllQueryable()
            .Include(x => x.User)
            .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
            .Where(x => x.UserId == userId && !x.IsDeleted);

        if (parameters.Status.HasValue)
            query = query.Where(x =>
                x.Status == (int)parameters.Status.Value);

        var totalRecords = await query.CountAsync();

        var orders = await query
            .OrderByDescending(x => x.OrderDate)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => MapToOrderDto(x))
            .ToListAsync();

        return new PagedResponse<OrderDto>
        {
            Data = orders,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(
                totalRecords / (double)parameters.PageSize)
        };
    }

    public async Task<OrderDto?> GetOrderByIdAsync(
        int orderId, int userId, bool isAdmin)
    {
        var query = _unitOfWork.Orders
            .GetAllQueryable()
            .Include(x => x.User)
            .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
            .Where(x => x.Id == orderId && !x.IsDeleted);

        // ✅ Customer يشوف Orders بتاعته بس
        if (!isAdmin)
            query = query.Where(x => x.UserId == userId);

        var order = await query.FirstOrDefaultAsync();

        if (order == null)
            return null;

        return MapToOrderDto(order);
    }

    public async Task<OrderDto?> PlaceOrderAsync(
        int userId, PlaceOrderDto dto)
    {
        // ✅ Step 1 — جيب الـ Cart
        var cart = await _unitOfWork.Carts
            .GetAllQueryable()
            .Include(x => x.User)
            .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            return null;

        // ✅ Step 2 — تحقق من الـ Stock لكل item                          
        foreach (var item in cart.CartItems)
        {
            if (item.Product.StockQuantity < item.Quantity)
                return null; // ✅ مفيش stock كافي
        }

        // ✅ Step 3 — احسب الـ Total
        var totalAmount = cart.CartItems.Sum(x =>
            x.Product.Price * x.Quantity);

        // ✅ Step 4 — اعمل الـ Order
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            Status = (int)OrderStatus.Pending,
            ShippingAddress = dto.ShippingAddress,
            PaymentMethod = dto.PaymentMethod,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // ✅ Step 5 — اعمل OrderItems وانقص الـ Stock
        foreach (var item in cart.CartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Product.Price,
                TotalPrice = item.Product.Price * item.Quantity
            };

            await _unitOfWork.OrderItems.AddAsync(orderItem);

            // ✅ نقص الـ Stock
            item.Product.StockQuantity -= item.Quantity;
        }

        // ✅ Step 6 — امسح الـ Cart
        cart.CartItems.Clear();

        await _unitOfWork.SaveChangesAsync();

        // ✅ Step 7 — رجّع الـ Order كامل
        var createdOrder = await _unitOfWork.Orders
            .GetAllQueryable()
            .Include(x => x.User)
            .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == order.Id);

        return MapToOrderDto(createdOrder!);
    }

    public async Task<bool> UpdateOrderStatusAsync(
        int orderId, UpdateOrderStatusDto dto)
    {
        var order = await _unitOfWork.Orders
            .GetAllQueryable()
            .FirstOrDefaultAsync(x =>
                x.Id == orderId && !x.IsDeleted);

        if (order == null)
            return false;

        order.Status = (int)dto.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelOrderAsync(
        int orderId, int userId)
    {
        var order = await _unitOfWork.Orders
            .GetAllQueryable()
            .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.Id == orderId &&
                x.UserId == userId &&
                !x.IsDeleted);

        if (order == null)
            return false;

        // ✅ Customer يقدر يلغي بس لو Order لسه Pending
        if (order.Status != (int)OrderStatus.Pending)
            return false;

        order.Status = (int)OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        // ✅ رجّع الـ Stock
        foreach (var item in order.OrderItems)
        {
            item.Product.StockQuantity += item.Quantity;
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ✅ Helper — بنستخدمه في كل method
    private static OrderDto MapToOrderDto(Order order)
    {
        var status = (OrderStatus)order.Status;

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            UserFullName =
                $"{order.User.FirstName} {order.User.LastName}",
            UserEmail = order.User.Email,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = status,
            StatusName = status.ToString(),
            ShippingAddress = order.ShippingAddress,
            PaymentMethod = order.PaymentMethod,
            Items = order.OrderItems.Select(item =>
                new OrderItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    MainImageUrl = item.Product.MainImageUrl,
                    UnitPrice = item.Price,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                }).ToList()
        };
    }
}