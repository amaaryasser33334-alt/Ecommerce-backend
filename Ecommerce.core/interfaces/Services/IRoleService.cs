using Ecommerce.core.DTos.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.interfaces.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RolesDtos>> GetAllRolesAsync();

        Task<RolesDtos?> GetRoleByIdAsync(int id);

        Task<bool> CreateRoleAsync(string roleName);

        Task<bool> DeleteRoleAsync(int id);
    }
}
