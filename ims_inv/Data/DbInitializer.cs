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
        }
    }
}
