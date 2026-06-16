using Ecommerce.core.DTos.Auth;
using Ecommerce.core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.interfaces.Services
{
    public interface IAuthService
    {
        Task<TokenResponse?> RegisterAsync(RegisterRequest request);

        Task<TokenResponse?> LoginAsync(LoginRequest request);

        Task<TokenResponse?> RefreshTokenAsync(RefreshRequest request);

        Task<bool> LogoutAsync(LogoutRequest request);
    }
}
