using Ecommerce.core.DTos.Category;
using Ecommerce.core.DTos.common;
using Ecommerce.core.interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IAuthorizationEvaluator _authorizationEvaluator;

    public CategoriesController(ICategoryService categoryService, IAuthorizationEvaluator authorizationEvaluator)
    {
        _categoryService = categoryService;
        _authorizationEvaluator = authorizationEvaluator;
    }

    // GET /api/Categories — الكل يشوف
    [HttpGet(Name = "GetAllCategories")]
    public async Task<ActionResult<PagedResponse<CategoryDto>>> GetAllCategories(
     [FromQuery] CategoryQueryParameters parameters)
    {
        var result = await _categoryService
            .GetAllCategoriesAsync(parameters);

        if (!result.Data.Any())
            return NotFound("No categories found");

        return Ok(result);
    }

    // GET /api/Categories/{id} — الكل يشوف
    [HttpGet("{id}", Name = "GetCategoryById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
    {
        if (id < 1)
            return BadRequest($"Invalid ID {id}");

        var result = await _categoryService
            .GetCategoryByIdAsync(id);

        if (result == null)
            return NotFound($"Category with ID {id} not found");

        return Ok(result);
    }

    // POST /api/Categories — Admin فقط
    [Authorize(Roles = "Admin")]
    [HttpPost(Name = "CreateCategory")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody] CreateCategoryDto dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.Name))
            return BadRequest("Invalid category data");

        var result = await _categoryService
            .CreateCategoryAsync(dto);

        return CreatedAtRoute(
            "GetCategoryById",
            new { id = result.Id },
            result);
    }

    // PUT /api/Categories/{id} — Admin فقط
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}", Name = "UpdateCategory")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        int id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoryService
            .UpdateCategoryAsync(id, dto);

        if (!result)
            return NotFound($"Category with ID {id} not found");

        return Ok(dto);
    }

    // DELETE /api/Categories/{id} — Admin فقط
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}", Name = "DeleteCategory")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService
            .DeleteCategoryAsync(id);

        if (!result)
            return NotFound($"Category with ID {id} not found");

        return Ok($"Category with ID {id} deleted successfully");
    }
}