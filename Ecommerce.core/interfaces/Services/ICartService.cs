using Ecommerce.core.DTos.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.interfaces.Services
{
    public interface ICartService
    {
        
        Task<CartDto?> GetCartAsync(int userId);

        Task<CartDto?> AddToCartAsync(int userId, AddToCartDto dto);

        
        Task<CartDto?> UpdateCartItemAsync(int userId, int productId, UpdateCartItemDto dto);

       
        Task<bool> RemoveCartItemAsync(int userId, int productId);

        
        Task<bool> ClearCartAsync(int userId);
    }

}
