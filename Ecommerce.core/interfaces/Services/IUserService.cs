using Ecommerce.core.DTos.common;
using Ecommerce.core.DTos.user;

namespace Ecommerce.core.interfaces.Services;

public interface IUserService
{
    Task<PagedResponse<UserDto>> GetAllUsersAsync(
        UserQueryParameters parameters);

    Task<UserDto?> GetUserByIdAsync(int id);

    Task<bool> UpdateUserAsync(int id, UpdateUserDto dto);

    Task<bool> DeleteUserAsync(int id);

    Task<bool> AssignRoleAsync(int userId, string roleName);
}