using Ecommerce.core.DTos.Cart;
using Ecommerce.core.interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // ✅ كل الـ Cart endpoints محتاجة Login
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // GET /api/Cart — جيب الـ Cart بتاعتك
    [HttpGet(Name = "GetCart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetUserId();
        var result = await _cartService.GetCartAsync(userId);

        if (result == null)
            return NotFound("Your cart is empty");

        return Ok(result);
    }

    // POST /api/Cart/items — ضيف منتج
    [HttpPost("items", Name = "AddToCart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartDto dto)
    {
        if (dto.Quantity < 1)
            return BadRequest("Quantity must be at least 1");

        var userId = GetUserId();
        var result = await _cartService.AddToCartAsync(userId, dto);

        if (result == null)
            return BadRequest("Product not found or insufficient stock");

        return Ok(result);
    }

    // PUT /api/Cart/items/{productId} — عدل الكمية
    [HttpPut("items/{productId}", Name = "UpdateCartItem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> UpdateCartItem(
        int productId, [FromBody] UpdateCartItemDto dto)
    {
        if (dto.Quantity < 1)
            return BadRequest("Quantity must be at least 1");

        var userId = GetUserId();
        var result = await _cartService.UpdateCartItemAsync(userId, productId, dto);

        if (result == null)
            return NotFound(
                "Item not found or insufficient stock");

        return Ok(result);
    }

    // DELETE /api/Cart/items/{productId} — امسح item معين
    [HttpDelete("items/{productId}", Name = "RemoveCartItem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCartItem(int productId)
    {
        var userId = GetUserId();
        var result = await _cartService
            .RemoveCartItemAsync(userId, productId);

        if (!result)
            return NotFound("Item not found in cart");

        return Ok("Item removed from cart");
    }

    // DELETE /api/Cart — امسح الـ Cart كلها
    [HttpDelete(Name = "ClearCart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        var result = await _cartService.ClearCartAsync(userId);

        if (!result)
            return NotFound("Cart not found");

        return Ok("Cart cleared successfully");
    }

    // ✅ Helper — نجيب الـ userId من الـ JWT في كل request
    private int GetUserId() =>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}