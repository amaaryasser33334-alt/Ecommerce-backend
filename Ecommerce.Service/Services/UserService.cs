using Ecommerce.core.DTos.common;
using Ecommerce.core.DTos.user;
using Ecommerce.core.interfaces;
using Ecommerce.core.interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Service.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<UserDto>> GetAllUsersAsync(
        UserQueryParameters parameters)
    {
        var query = _unitOfWork.Users
            .GetAllQueryable()
            .Include(x => x.Roles)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            query = query.Where(x =>
                x.FirstName.Contains(parameters.Search) ||
                x.LastName.Contains(parameters.Search));
        }

        var totalRecords = await query.CountAsync();

        var users = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new UserDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Role = x.Roles
                    .Select(r => r.Name)
                    .FirstOrDefault() ?? ""
            })
            .ToListAsync();

        return new PagedResponse<UserDto>
        {
            Data = users,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(
                totalRecords / (double)parameters.PageSize)
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Users
            .GetAllQueryable()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Roles
                .Select(r => r.Name)
                .FirstOrDefault() ?? ""
        };
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _unitOfWork.Users
            .GetAllQueryable()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (user == null)
            return false;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _unitOfWork.Users
            .GetAllQueryable()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (user == null)
            return false;

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRoleAsync(int userId, string roleName)
    {
        var user = await _unitOfWork.Users
            .GetAllQueryable()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);

        if (user == null)
            return false;

        var role = _unitOfWork.Roles
            .GetAllQueryable()
            .FirstOrDefault(x => x.Name == roleName);

        if (role == null)
            return false;

        user.Roles.Clear();
        user.Roles.Add(role);

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}