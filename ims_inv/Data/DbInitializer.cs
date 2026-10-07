using ims_inv.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ims_inv.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WebAppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync())
            {
                var hasher = new PasswordHasher<User>();
                var admin = new User
                {
                    Name = "Admin",
                    Email = "admin@example.com",
                    Password = hasher.HashPassword(new User(), "Admin@123")
                };

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
                logger.LogInformation("Admin user seeded successfully: admin@example.com (Password: Admin@123)");
            }

            if (!await context.Warehouses.AnyAsync())
            {
                var defaultWarehouse = new Warehouse
                {
                    Name = "Main Warehouse",
                    Address = "Central Storage Hub, 100 Industrial Parkway",
                    Phone = "+1-555-0199",
                    Email = "main@warehouse.local",
                    IsActive = true,
                    Capacity = "50000 sq ft"
                };

                await context.Warehouses.AddAsync(defaultWarehouse);
                await context.SaveChangesAsync();
                logger.LogInformation("Default warehouse seeded successfully: Main Warehouse");
            }

            if (!await context.Units.AnyAsync())
            {
                var defaultUnit = new Unit
                {
                    Name = "Pieces",
                    Symbol = "pcs",
                    Type = "count"
                };

                await context.Units.AddAsync(defaultUnit);
                await context.SaveChangesAsync();
                logger.LogInformation("Default unit seeded successfully: Pieces (pcs)");
            }
        }
    }
}
