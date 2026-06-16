using Ecommerce.core.DTos.Category;
using Ecommerce.core.DTos.common;
using Ecommerce.core.interfaces;
using Ecommerce.core.interfaces.Services;
using Ecommerce_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Service.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<PagedResponse<CategoryDto>> GetAllCategoriesAsync(CategoryQueryParameters parameters)
        {
            var query = _unitOfWork.Categories
                .GetAllQueryable()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name);

            var totalRecords = await query.CountAsync();

            var categories = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl,
                    ProductCount = x.Products.Count(p => !p.IsDeleted)
                })
                .ToListAsync();

            return new PagedResponse<CategoryDto>
            {
                Data = categories,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(
                    totalRecords / (double)parameters.PageSize)
            };
        }


        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            return await _unitOfWork.Categories
                .GetAllQueryable()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl,
                    ProductCount = x.Products
                        .Count(p => !p.IsDeleted)
                })
                .FirstOrDefaultAsync();
        }


        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsDeleted = false
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ProductCount = 0
            };
        }

        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            // ✅ Tracked Entity — EF هيعمل UPDATE تلقائي
            var category = await _unitOfWork.Categories
                .GetAllQueryable()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (category == null)
                return false;

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.ImageUrl = dto.ImageUrl;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.Categories
                .GetAllQueryable()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (category == null)
                return false;

            // ✅ Soft Delete
            category.IsDeleted = true;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
