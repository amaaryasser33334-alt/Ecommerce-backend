using Ecommerce.core.interfaces.Repositories;
using Ecommerce_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ecommerce.core.interfaces
{
    public interface IUnitOfWork : IDisposable
    {

        IGenericRepository<User> Users { get; }

        IGenericRepository<Role> Roles { get; }

        IGenericRepository<Product> Products { get; }

        IGenericRepository<Category> Categories { get; }

        IGenericRepository<ProductImage> ProductImages { get; }

        IGenericRepository<Cart> Carts { get; }

        IGenericRepository<CartItem> CartItems { get; }

        IGenericRepository<Order> Orders { get; }

        IGenericRepository<OrderItem> OrderItems { get; }

        IGenericRepository<Payment> Payments { get; }

        IGenericRepository<Shipping> Shippings { get; }

        IGenericRepository<Address> Addresses { get; }

        IGenericRepository<Review> Reviews { get; }

        IGenericRepository<Wishlist> Wishlists { get; }

        IGenericRepository<RefreshToken> RefreshTokens { get; }

        Task<int> SaveChangesAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
