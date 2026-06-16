using Ecommerce.core.DTos.Category;
using Ecommerce.core.DTos.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.interfaces.Services
{
    public interface ICategoryService
    {

        Task<PagedResponse<CategoryDto>> GetAllCategoriesAsync(CategoryQueryParameters parameters);

        Task<CategoryDto?> GetCategoryByIdAsync(int id);

        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);

        Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto);

        Task<bool> DeleteCategoryAsync(int id);

    }
}
