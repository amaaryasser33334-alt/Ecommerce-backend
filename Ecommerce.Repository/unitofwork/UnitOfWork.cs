using Ecommerce.core.interfaces;
using Ecommerce.core.interfaces.Repositories;
using Ecommerce.Repository.Repositiries;
using Ecommerce_Backend.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Repository.unitofwork
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly ECommerceDbContext _context;

        public IGenericRepository<User> Users { get; private set; }

        public IGenericRepository<Role> Roles { get; private set; }

        public IGenericRepository<Category> Categories { get; private set; }

        public IGenericRepository<Product> Products { get; private set; }

        public IGenericRepository<ProductImage> ProductImages { get; private set; }

        public IGenericRepository<Cart> Carts { get; private set; }

        public IGenericRepository<CartItem> CartItems { get; private set; }

        public IGenericRepository<Order> Orders { get; private set; }

        public IGenericRepository<OrderItem> OrderItems { get; private set; }

        public IGenericRepository<Payment> Payments { get; private set; }

        public IGenericRepository<Shipping> Shippings { get; private set; }

        public IGenericRepository<Address> Addresses { get; private set; }

        public IGenericRepository<Review> Reviews { get; private set; }

        public IGenericRepository<Wishlist> Wishlists { get; private set; }

        public IGenericRepository<RefreshToken> RefreshTokens { get; private set; }

        public UnitOfWork(ECommerceDbContext context)
        {
            _context = context;

            Users = new GenericRepository<User>(_context);
            Roles = new GenericRepository<Role>(_context);
            Categories = new GenericRepository<Category>(_context);
            Products = new GenericRepository<Product>(_context);
            ProductImages = new GenericRepository<ProductImage>(_context);
            Carts = new GenericRepository<Cart>(_context);
            CartItems = new GenericRepository<CartItem>(_context);
            Orders = new GenericRepository<Order>(_context);
            OrderItems = new GenericRepository<OrderItem>(_context);
            Payments = new GenericRepository<Payment>(_context);
            Shippings = new GenericRepository<Shipping>(_context);
            Addresses = new GenericRepository<Address>(_context);
            Reviews = new GenericRepository<Review>(_context);
            Wishlists = new GenericRepository<Wishlist>(_context);
            RefreshTokens = new GenericRepository<RefreshToken>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
