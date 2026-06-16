using Ecommerce.core.DTos.common;
using Ecommerce.core.DTos.product;
using Ecommerce.core.interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IAuthorizationService _authorizationService;

    public ProductsController(IProductService productService, IAuthorizationService authorizationService)
    {
        _productService = productService;
        _authorizationService = authorizationService;
    }

    [HttpGet(Name = "GetAllProducts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<ProductDto>>> GetAllProducts(
        [FromQuery] ProductQueryParameters parameters)
    {
        var result = await _productService
            .GetAllProductsAsync(parameters);

        if (!result.Data.Any())
            return NotFound("No products found");

        return Ok(result);
    }

    [HttpGet("{id}", Name = "GetProductById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        if (id < 1)
            return BadRequest($"Invalid ID {id}");

        var result = await _productService
            .GetProductByIdAsync(id);

        if (result == null)
            return NotFound($"Product with ID {id} not found");

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost(Name = "CreateProduct")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.Name))
            return BadRequest("Invalid product data");

        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _productService
            .CreateProductAsync(dto, userId);

        return CreatedAtRoute("GetProductById",new { id = result.Id },result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}", Name = "UpdateProduct")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        int id, [FromBody] UpdateProductDto dto)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _productService
            .UpdateProductAsync(id, dto, userId);

        if (!result)
            return NotFound($"Product with ID {id} not found");

        return Ok(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}", Name = "DeleteProduct")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService
            .DeleteProductAsync(id);

        if (!result)
            return NotFound($"Product with ID {id} not found");

        return Ok($"Product with ID {id} deleted successfully");
    }
}