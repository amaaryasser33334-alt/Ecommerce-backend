using Ecommerce.core.DTos.common;
using Ecommerce.core.DTos.Order;
using Ecommerce.core.interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET /api/Orders — Admin فقط
    [Authorize(Roles = "Admin")]
    [HttpGet(Name = "GetAllOrders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<OrderDto>>> GetAllOrders(
        [FromQuery] OrderQueryParameters parameters)
    {
        var result = await _orderService
            .GetAllOrdersAsync(parameters);

        if (!result.Data.Any())
            return NotFound("No orders found");

        return Ok(result);
    }

    // GET /api/Orders/my-orders — Customer
    [HttpGet("my-orders", Name = "GetMyOrders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<OrderDto>>> GetMyOrders(
        [FromQuery] OrderQueryParameters parameters)
    {
        var userId = GetUserId();
        var result = await _orderService
            .GetMyOrdersAsync(userId, parameters);

        if (!result.Data.Any())
            return NotFound("You have no orders");

        return Ok(result);
    }

    // GET /api/Orders/{id} — Owner أو Admin
    [HttpGet("{id}", Name = "GetOrderById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderById(int id)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var result = await _orderService
            .GetOrderByIdAsync(id, userId, isAdmin);

        if (result == null)
            return NotFound($"Order with ID {id} not found");

        return Ok(result);
    }

    // POST /api/Orders — Customer يعمل Order من الـ Cart
    [HttpPost(Name = "PlaceOrder")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> PlaceOrder(
        [FromBody] PlaceOrderDto dto)
    {
        var userId = GetUserId();
        var result = await _orderService
            .PlaceOrderAsync(userId, dto);

        if (result == null)
            return BadRequest(
                "Cart is empty or insufficient stock for one or more items");

        return CreatedAtRoute(
            "GetOrderById",
            new { id = result.Id },
            result);
    }

    // PUT /api/Orders/{id}/status — Admin فقط
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/status", Name = "UpdateOrderStatus")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOrderStatus(
        int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var result = await _orderService
            .UpdateOrderStatusAsync(id, dto);

        if (!result)
            return NotFound($"Order with ID {id} not found");

        return Ok($"Order {id} status updated to {dto.Status}");
    }

    // PUT /api/Orders/{id}/cancel — Customer
    [HttpPut("{id}/cancel", Name = "CancelOrder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = GetUserId();
        var result = await _orderService
            .CancelOrderAsync(id, userId);

        if (!result)
            return BadRequest(
                "Order not found or cannot be cancelled " +
                "(only Pending orders can be cancelled)");

        return Ok($"Order {id} cancelled successfully");
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!);
}