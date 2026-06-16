using Ecommerce.core.DTos.Cart;
using Ecommerce.core.interfaces;
using Ecommerce.core.interfaces.Services;
using Ecommerce_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Service.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto?> GetCartAsync(int userId)
    {
        var cart = await _unitOfWork.Carts
            .GetAllQueryable()
            .Include(x => x.User)
            .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

       
        if (cart == null)
            return null;

        return MapToCartDto(cart);
    }

    public async Task<CartDto?> AddToCartAsync(int userId, AddToCartDto dto)
    {
        
        var product = await _unitOfWork.Products
            .GetAllQueryable()
            .FirstOrDefaultAsync(x =>
                x.Id == dto.ProductId &&
                !x.IsDeleted);

        if (product == null)
            return null;

        if (product.StockQuantity < dto.Quantity)
            return null; 

   
        var cart = await _unitOfWork.Carts
            .GetAllQueryable()
            .Include(x => x.User)
            .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
        {
            
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();

            // ✅ reload بعد الـ save عشان نجيب الـ Id
            cart = await _unitOfWork.Carts
                .GetAllQueryable()
                .Include(x => x.User)
                .Include(x => x.CartItems)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        // ✅ لو المنتج موجود في الـ Cart، زود الكمية
        var existingItem = cart!.CartItems
            .FirstOrDefault(x => x.ProductId == dto.ProductId);

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + dto.Quantity;

            if (newQuantity > product.StockQuantity)
                return null;

            existingItem.Quantity = newQuantity;
        }
        else
        {
            // ✅ ضيف item جديد
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            await _unitOfWork.CartItems.AddAsync(cartItem);
        }

        await _unitOfWork.SaveChangesAsync();

        // ✅ reload عشان نرجع البيانات محدثة
        cart = await _unitOfWork.Carts
            .GetAllQueryable()
            .Include(x => x.User)
            .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return MapToCartDto(cart!);
    }

    public async Task<CartDto?> UpdateCartItemAsync(int userId, int productId,UpdateCartItemDto dto)
    {
        if (dto.Quantity < 1)
            return null;

        var cart = await _unitOfWork.Carts
            .GetAllQueryable()
            .Include(x => x.User)
            .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
            return null;

        var item = cart.CartItems
            .FirstOrDefault(x => x.ProductId == productId);

        if (item == null)
            return null;

        // ✅ تحقق من الـ Stock
        var product = await _unitOfWork.Products
            .GetAllQueryable()
            .FirstOrDefaultAsync(x => x.Id == productId);

        if (product == null || product.StockQuantity < dto.Quantity)
            return null;

        item.Quantity = dto.Quantity;
        await _unitOfWork.SaveChangesAsync();

        return MapToCartDto(cart);
    }

    public async Task<bool> RemoveCartItemAsync(int userId, int productId)
    {
        var cart = await _unitOfWork.Carts
            .GetAllQueryable()
            .Include(x => x.CartItems)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
            return false;

        var item = cart.CartItems
            .FirstOrDefault(x => x.ProductId == productId);

        if (item == null)
            return false;

        // ✅ نجيب الـ Id من CartItems
        // CartItems PK هو (CartId, ProductId)
        // الـ GenericRepository.Delete بياخد int id
        // فهنعمل workaround مباشر
        cart.CartItems.Remove(item);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var cart = await _unitOfWork.Carts
            .GetAllQueryable()
            .Include(x => x.CartItems)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
            return false;

        cart.CartItems.Clear();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    private static CartDto MapToCartDto(Cart cart)
    {
        return new CartDto
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            UserFullName = $"{cart.User.FirstName} {cart.User.LastName}",
            CreatedAt = cart.CreatedAt,
            Items = cart.CartItems.Select(item => new CartItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                MainImageUrl = item.Product.MainImageUrl,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity
            }).ToList()
        };
    }
}