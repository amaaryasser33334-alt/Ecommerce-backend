using Ecommerce.core.DTos.common;
using Ecommerce.core.DTos.product;
using Ecommerce.core.DTos.user;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.interfaces.Services
{


    public interface IProductService
    {
        Task<PagedResponse<ProductDto>> GetAllProductsAsync(
            ProductQueryParameters parameters);

        Task<ProductDto?> GetProductByIdAsync(int id);

        Task<ProductDto> CreateProductAsync(
            CreateProductDto dto, int createdByUserId);

        Task<bool> UpdateProductAsync(
            int id, UpdateProductDto dto, int updatedByUserId);

        Task<bool> DeleteProductAsync(int id);
    }
}