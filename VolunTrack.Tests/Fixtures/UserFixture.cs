using BCrypt.Net;
using VolunTrack.Data;
using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.Tests.Fixtures
{
    public class UserFixture
    {
        public List<User> TestUsers { get; private set; } =
            [
                // Initial test users
                new User()
                {
                    Login = "SeaLard",
                    FullName = "Light Yagami",
                    Phone = "+7(999)645-41-04",
                    Email = "IamJustice123@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(
                        "strongpassword",
                        HashType.SHA512,
                        workFactor: 12
                    ),
                    Role = UserRole.Volunteer,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2025, 2, 12, 12, 45, 32).ToUniversalTime()
                },
                new User()
                {
                    Login = "RingP",
                    FullName = "Elenas Weber",
                    Phone = "+7(924)788-44-12",
                    Email = "Sasageyo@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(
                        "strongpassword",
                        HashType.SHA512,
                        workFactor: 12
                    ),
                    Role = UserRole.Volunteer,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 3, 1, 14, 9, 47).ToUniversalTime()
                },
                new User()
                {
                    Login = "BirchLog",
                    FullName = "Logan Paul",
                    Phone = "+7(923)623-32-11",
                    Email = "Singerrr@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(
                        "strongpassword",
                        HashType.SHA512,
                        workFactor: 12
                    ),
                    Role = UserRole.Volunteer,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 4, 6, 21, 11, 51).ToUniversalTime()
                },
                new User()
                {
                    Login = "BESTdetective",
                    FullName = "Sae Niijima",
                    Phone = "+7(927)623-65-42",
                    Email = "PoliceIsweartogod@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(
                        "strongpassword",
                        HashType.SHA512,
                        workFactor: 12
                    ),
                    Role = UserRole.EventCoordinator,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 1, 9, 17, 34, 19).ToUniversalTime()
                },
                new User()
                {
                    Login = "Lolipop",
                    FullName = "Nile Doll",
                    Phone = "+7(957)645-12-20",
                    Email = "NileNotRiver@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(
                        "strongpassword",
                        HashType.SHA512,
                        workFactor: 12
                    ),
                    Role = UserRole.RegionCoordinator,
                    IsActive = false,
                    CreatedAtUtc = new DateTime(2026, 5, 7, 21, 9, 49).ToUniversalTime()
                },
                new User()
                {
                    Login = "admin",
                    FullName = "System Administrator",
                    Phone = "+7(999)999-99-99",
                    Email = "admin@admin.com",
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(
                        "strongpassword",
                        HashType.SHA512,
                        workFactor: 12
                    ),
                    Role = UserRole.Administrator,
                    IsActive = false,
                    CreatedAtUtc = new DateTime(2026, 3, 24, 8, 56, 29).ToUniversalTime()
                }
            ];

        public List<User> GetCopyOfTestUsers()
        {
            return [.. TestUsers.Select(u => new User
            {
                Id = u.Id,
                Login = u.Login,
                FullName = u.FullName,
                Phone = u.Phone,
                Email = u.Email,
                PasswordHash = u.PasswordHash,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAtUtc = u.CreatedAtUtc
            })];
        }

        public async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Users.AddRangeAsync(TestUsers);
            await context.SaveChangesAsync();
        }
    }
}
