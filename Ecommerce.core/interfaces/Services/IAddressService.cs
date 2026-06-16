using Ecommerce.core.DTos.Address;
using Ecommerce_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.interfaces.Services
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetUserAddressesAsync(string userId);

        Task AddAddressAsync(string userId,CreateAddressDto dto);

        Task DeleteAddressAsync(int addressId);
    }
}
