using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAllQueryable();
       
        Task<T?> GetByIdAsync(int id);

        Task AddAsync(T entity);

        Task Update(int id, T entity);

        Task Delete(int id);


    }
}
