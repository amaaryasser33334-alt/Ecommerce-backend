using Ecommerce.core.DTos.Shipping;
using Ecommerce.core.interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ShippingsController : ControllerBase
{
    private readonly IShippingService _shippingService;

    public ShippingsController(
        IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    // GET /api/Shippings/{orderId} — Owner أو Admin
    [HttpGet("{orderId}", Name = "GetShippingByOrderId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShippingDto>> GetShippingByOrderId(
        int orderId)
    {
        if (orderId < 1)
            return BadRequest($"Invalid ID {orderId}");

        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var result = await _shippingService
            .GetShippingByOrderIdAsync(orderId, userId, isAdmin);

        if (result == null)
            return NotFound(
                $"Shipping for order {orderId} not found");

        return Ok(result);
    }

    // POST /api/Shippings — Admin فقط
    [Authorize(Roles = "Admin")]
    [HttpPost(Name = "CreateShipping")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ShippingDto>> CreateShipping(
        [FromBody] CreateShippingDto dto)
    {
        var result = await _shippingService
            .CreateShippingAsync(dto);

        if (result == null)
            return BadRequest(
                "Order not found, not confirmed yet, " +
                "or already has a shipment");

        return CreatedAtRoute(
            "GetShippingByOrderId",
            new { orderId = result.OrderId },
            result);
    }

    // PUT /api/Shippings/{id} — Admin فقط
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}", Name = "UpdateShipping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateShipping(
        int id, [FromBody] UpdateShippingDto dto)
    {
        var result = await _shippingService
            .UpdateShippingAsync(id, dto);

        if (!result)
            return NotFound(
                $"Shipping with ID {id} not found");

        return Ok("Shipping updated successfully");
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!);
}