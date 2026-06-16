using Ecommerce.core.DTos.Payments;
using Ecommerce.core.interfaces;
using Ecommerce.core.interfaces.Services;
using Ecommerce.Repository.unitofwork;
using Ecommerce.Service.Services;
using Ecommerce_Backend.Authorization;
using Ecommerce_Backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string? connectionString =
    configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Connection string not found.");
    return;
}

var options =
    new DbContextOptionsBuilder<ECommerceDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using (var context = new ECommerceDbContext(options))
{
    if (context.Database.CanConnect())
    {
        Console.WriteLine("✅ Connected successfully to Database.");
    }
}

var builder = WebApplication.CreateBuilder(args);


// ==================================================
// JWT Authentication
// ==================================================

var secretKey =
    builder.Configuration["JWT_SECRET_KEY"];

if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new Exception(
        "JWT secret key is not configured.");
}

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = "EcommerceApi",
                ValidAudience = "EcommerceUsers",

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey))
            };
    });


// ==================================================
// Authorization
// ==================================================

// في Program.cs — لازم تضيف ده
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOwnerOrAdmin", policy =>
        policy.Requirements.Add(
            new UserOwnerOrAdminRequirement()));
});


builder.Services.AddSingleton<IAuthorizationHandler, UserOwnerOrAdminHandler>();

// ==================================================
// Rate Limiting
// ==================================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthLimiter", httpContext =>
    {
        var ip =
            httpContext.Connection
            .RemoteIpAddress?
            .ToString() ?? "unknown";

        return RateLimitPartition
            .GetFixedWindowLimiter(
                partitionKey: ip,
                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
    });
});


// ==================================================
// Controllers
// ==================================================

builder.Services.AddControllers();


// ==================================================
// DbContext
// ==================================================

builder.Services.AddDbContext<ECommerceDbContext>(
    options =>
    {
        options.UseSqlServer(connectionString);
    });


// ==================================================
// Unit Of Work
// ==================================================

builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();


// ==================================================
// Services
// ==================================================

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IAuthorizationHandler, UserOwnerOrAdminHandler>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// ضيف الـ Stripe Secret Key
//StripeConfiguration.ApiKey =
//    builder.Configuration["Stripe:SecretKey"];

// لما تعملهم بعدين
// builder.Services.AddScoped<ICartService, CartService>();
// builder.Services.AddScoped<IProductService, ProductService>();
// builder.Services.AddScoped<IOrderService, OrderService>();
// builder.Services.AddScoped<IWishlistService, WishlistService>();


// ==================================================
// Swagger
// ==================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "Bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Enter: Bearer {your token}"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});


// ==================================================
// CORS
// ==================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ECommerceCorsPolicy",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// ==================================================
// Build
// ==================================================

var app = builder.Build();


// ==================================================
// Development
// ==================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// ==================================================
// Middleware
// ==================================================

app.UseHttpsRedirection();

//app.UseStaticFiles();

app.UseRateLimiter();

app.UseCors("ECommerceCorsPolicy");

app.UseAuthentication();

app.UseAuthorization();


// ==================================================
// Log Forbidden Requests
// ==================================================

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode ==
        StatusCodes.Status403Forbidden)
    {
        var userId =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? "anonymous";

        var ip =
            context.Connection
            .RemoteIpAddress?
            .ToString()
            ?? "unknown";

        var path =
            context.Request.Path.ToString();

        app.Logger.LogWarning(
            "Forbidden access. UserId={UserId}, Path={Path}, IP={IP}",
            userId,
            path,
            ip);
    }
});


// ==================================================
// Endpoints
// ==================================================

app.MapControllers();

app.Run();