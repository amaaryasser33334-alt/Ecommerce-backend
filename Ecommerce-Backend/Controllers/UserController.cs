using Ecommerce.core.DTos.common;
using Ecommerce.core.DTos.user;
using Ecommerce.core.interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthorizationService _authorizationService;

    public UsersController(
        IUserService userService,
        IAuthorizationService authorizationService)
    {
        _userService = userService;
        _authorizationService = authorizationService;
    }

    // GET /api/Users — Admin فقط
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<UserDto>>> GetAllUsers(
        [FromQuery] UserQueryParameters parameters)
    {
        var result = await _userService.GetAllUsersAsync(parameters);

        if (!result.Data.Any())
            return NotFound("No Users Found");

        return Ok(result);
    }

    // GET /api/Users/{id} — Owner أو Admin
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        var authResult = await _authorizationService
            .AuthorizeAsync(User, id, "UserOwnerOrAdmin");

        if (!authResult.Succeeded)
            return Forbid();

        var result = await _userService.GetUserByIdAsync(id);

        if (result == null)
            return NotFound();
        return Ok(result);
    }
   
    // PUT /api/Users/{id} — Owner أو Admin
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(
        int id, [FromBody] UpdateUserDto dto)
    {
        var authResult = await _authorizationService
            .AuthorizeAsync(User, id, "UserOwnerOrAdmin");

        if (!authResult.Succeeded)
            return Forbid();

        var result = await _userService.UpdateUserAsync(id, dto);

        if (!result)
            return NotFound();

        return Ok("User updated successfully");
    }

    // DELETE /api/Users/{id} — Admin فقط
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);

        if (!result)
            return NotFound();

        return Ok($"User with ID {id} deleted successfully");
    }

    // POST /api/Users/{id}/assign-role — Admin فقط
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/assign-role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRole(
        int id, [FromBody] string roleName)
    {
        var result = await _userService.AssignRoleAsync(id, roleName);

        if (!result)
            return BadRequest("User or Role not found");

        return Ok($"Role '{roleName}' assigned successfully");
    }
}