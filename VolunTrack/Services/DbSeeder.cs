using Microsoft.EntityFrameworkCore;
using VolunTrack.Data;
using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.Services
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
            {
                new() { Name = "Экологическое волонтёрство", 
                    Description = "Помощь природе: уборка территорий, посадка деревьев, забота о животных", 
                    ColorRgb = 0xF1C40F, IsActive = true },
                new() { Name = "Социальное волонтёрство", 
                    Description = "Помощь пожилым людям, детям-сиротам, людям в трудной жизненной ситуации", 
                    ColorRgb = 0x1ABC9C, IsActive = true },
                new() { Name = "Событийное волонтёрство", 
                    Description = "Помощь в организации мероприятий: фестивалей, концертов, спортивных событий", 
                    ColorRgb = 0x9B59B6, IsActive = true }
            };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            var adminConfig = configuration.GetSection("Administrator");
            var adminLogin = adminConfig["Login"];

            if (!await context.Users.AnyAsync(u => u.Login == adminLogin))
            {
                var admin = new User
                {
                    Login = adminLogin,
                    FullName = adminConfig["FullName"],
                    Phone = adminConfig["Phone"],
                    Email = adminConfig["Email"],
                    PasswordHash = scope.ServiceProvider
                        .GetRequiredService<IPasswordHasher>()
                        .HashPassword(adminConfig["Password"]),
                    Role = UserRole.Administrator,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}
