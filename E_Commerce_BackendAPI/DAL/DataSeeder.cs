using E_Commerce_BackendAPI.Model;
using E_Commerce_BackendAPI.Utilities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.DAL
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            if (await context.Categories.AnyAsync())
                return;

            var electronics = new Category
            {
                Name = "Electronics",
                Description = "Phones, laptops, and gadgets",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 0
            };
            var clothing = new Category
            {
                Name = "Clothing",
                Description = "Men, women, and kids apparel",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 0
            };
            var books = new Category
            {
                Name = "Books",
                Description = "Fiction, non-fiction, and educational",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 0
            };

            context.Categories.AddRange(electronics, clothing, books);
            await context.SaveChangesAsync();

            context.Products.AddRange(
                new Product
                {
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse",
                    Price = 29.99m,
                    Stock = 100,
                    CategoryId = electronics.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = 0
                },
                new Product
                {
                    Name = "USB-C Hub",
                    Description = "7-in-1 USB-C adapter",
                    Price = 49.99m,
                    Stock = 50,
                    CategoryId = electronics.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = 0
                },
                new Product
                {
                    Name = "Cotton T-Shirt",
                    Description = "Plain cotton t-shirt, multiple colors",
                    Price = 19.99m,
                    Stock = 200,
                    CategoryId = clothing.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = 0
                },
                new Product
                {
                    Name = "Denim Jeans",
                    Description = "Classic fit denim jeans",
                    Price = 59.99m,
                    Stock = 80,
                    CategoryId = clothing.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = 0
                },
                new Product
                {
                    Name = "Programming Guide",
                    Description = "Learn C# and .NET",
                    Price = 44.99m,
                    Stock = 30,
                    CategoryId = books.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = 0
                }
            );
            await context.SaveChangesAsync();

            if (await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
                return;

            var admin = new User
            {
                Name = "Admin",
                Email = "admin@ecommerce.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 0
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
