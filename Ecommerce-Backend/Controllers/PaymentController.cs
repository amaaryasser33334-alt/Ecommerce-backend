using Ecommerce.core.DTos.Payments;
using Ecommerce.core.interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    
    [HttpGet("{orderId}", Name = "GetPaymentByOrderId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetPaymentByOrderId(int orderId)
    {
        if (orderId < 1)
            return BadRequest($"Invalid ID {orderId}");

        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var result = await _paymentService
            .GetPaymentByOrderIdAsync(orderId, userId, isAdmin);

        if (result == null)
            return NotFound(
                $"Payment for order {orderId} not found");

        return Ok(result);
    }

 
    [HttpPost(Name = "CreatePayment")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        var userId = GetUserId();

        var result = await _paymentService
            .CreatePaymentAsync(userId, dto);

        if (result == null)
            return BadRequest(
                "Order not found, already paid, or cannot be paid at this stage");

        return CreatedAtRoute("GetPaymentByOrderId",new { orderId = result.OrderId },result);
    }

   
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/status", Name = "UpdatePaymentStatus")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePaymentStatus(
        int id, [FromBody] UpdatePaymentStatusDto dto)
    {
        var result = await _paymentService
            .UpdatePaymentStatusAsync(id, dto);

        if (!result)
            return NotFound($"Payment with ID {id} not found");

        return Ok($"Payment status updated to {dto.PaymentStatus}");
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!);
}