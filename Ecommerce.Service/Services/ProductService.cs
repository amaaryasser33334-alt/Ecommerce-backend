using Ecommerce.core.DTos.common;
using Ecommerce.core.DTos.product;
using Ecommerce.core.interfaces;
using Ecommerce.core.interfaces.Services;
using Ecommerce_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Service.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<ProductDto>> GetAllProductsAsync(ProductQueryParameters parameters)
    {
        var query = _unitOfWork.Products
            .GetAllQueryable()
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            query = query.Where(x =>
                x.Name.Contains(parameters.Search) ||
                (x.Description != null &&
                 x.Description.Contains(parameters.Search)));
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == parameters.CategoryId.Value);
        }

        var totalRecords = await query.CountAsync();

        var products = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                StockQuantity = x.StockQuantity,
                MainImageUrl = x.MainImageUrl,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return new PagedResponse<ProductDto>
        {
            Data = products,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(
                totalRecords / (double)parameters.PageSize)
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        return await _unitOfWork.Products
            .GetAllQueryable()
            .Include(x => x.Category)
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                StockQuantity = x.StockQuantity,
                MainImageUrl = x.MainImageUrl,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, int createdByUserId)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            MainImageUrl = dto.MainImageUrl,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            CreatedBy = createdByUserId.ToString()
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var category = _unitOfWork.Categories
            .GetAllQueryable()
            .FirstOrDefault(x => x.Id == dto.CategoryId);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            MainImageUrl = product.MainImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name ?? "",
            CreatedAt = product.CreatedAt
        };
    }

    public async Task<bool> UpdateProductAsync(
        int id, UpdateProductDto dto, int updatedByUserId)
    {
        var product = await _unitOfWork.Products
            .GetAllQueryable()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (product == null)
            return false;

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.MainImageUrl = dto.MainImageUrl;
        product.CategoryId = dto.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = updatedByUserId.ToString();

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _unitOfWork.Products
            .GetAllQueryable()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (product == null)
            return false;

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}