using Ecommerce.core.DTos.Auth;
using Ecommerce.core.DTOs.Auth;
using Ecommerce.core.interfaces;
using Ecommerce.core.interfaces.Services;
using Ecommerce_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Service.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<TokenResponse?> RegisterAsync(RegisterRequest request)
    {
        var exists = _unitOfWork.Users
            .GetAllQueryable()
            .Any(x => x.Email == request.Email);

        if (exists)
            return null;

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var customerRole = _unitOfWork.Roles
            .GetAllQueryable()
            .FirstOrDefault(x => x.Name == "Customer");

        if (customerRole != null)
            user.Roles = new List<Role> { customerRole };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return await GenerateTokens(user);
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        var user = _unitOfWork.Users
            .GetAllQueryable()
            .Include(x => x.Roles)
            .FirstOrDefault(x =>
                x.Email == request.Email &&
                !x.IsDeleted);

        if (user == null)
            return null;

        bool valid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!valid)
            return null;

        return await GenerateTokens(user);
    }

    public async Task<TokenResponse?> RefreshTokenAsync(RefreshRequest request)
    {
        var user = _unitOfWork.Users
            .GetAllQueryable()
            .Include(x => x.Roles)
            .FirstOrDefault(x => x.Email == request.Email);

        if (user == null)
            return null;

        var storedToken = _unitOfWork.RefreshTokens
            .GetAllQueryable()
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

        if (storedToken == null)
            return null;

        if (storedToken.RefreshTokenRevokedAt != null)
            return null;

        if (storedToken.RefreshTokenExpiresAt <= DateTime.UtcNow)
            return null;

        bool valid = BCrypt.Net.BCrypt.Verify(
            request.RefreshToken,
            storedToken.RefreshTokenHash);

        if (!valid)
            return null;

        return await GenerateTokens(user);
    }

    public async Task<bool> LogoutAsync(LogoutRequest request)
    {
        var user = _unitOfWork.Users
            .GetAllQueryable()
            .FirstOrDefault(x => x.Email == request.Email);

        if (user == null)
            return false;

        var token = _unitOfWork.RefreshTokens
            .GetAllQueryable()
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

        if (token == null)
            return false;

        token.RefreshTokenRevokedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task<TokenResponse> GenerateTokens(User user)
    {
        var roleName = user.Roles.FirstOrDefault()?.Name ?? "Customer";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, roleName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_SECRET_KEY"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: "EcommerceApi",
            audience: "EcommerceUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refreshToken = GenerateRefreshToken();

        var refreshEntity = new RefreshToken
        {
            UserId = user.Id,
            RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            RefreshTokenRevokedAt = null
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshEntity);
        await _unitOfWork.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}