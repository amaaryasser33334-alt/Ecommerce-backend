using Ecommerce.core.DTos.WishlistItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.interfaces.Services
{
    public interface IWishlistService
    {
        Task<WishlistDto> GetWishlistAsync(
            string userId);

        Task AddProductAsync(
            string userId,
            int productId);

        Task RemoveProductAsync(
            string userId,
            int productId);
    }
}
