using VolunTrack.Data;
using VolunTrack.Models;

namespace VolunTrack.Tests.Fixtures
{
    public class CategoryFixture
    {
        public List<Category> TestCategories { get; private set; } = [
            new Category
            {
                Id = 1,
                Name = "Экологическое волонтёрство",
                Description = "Помощь природе: уборка территорий, посадка деревьев, забота о животных",
                ColorRgb = 0xF1C40F,
                IsActive = true
            },
            new Category
            {
                Id = 2,
                Name = "Социальное волонтёрство",
                Description = "Помощь пожилым людям, детям-сиротам, людям в трудной жизненной ситуации",
                ColorRgb = 0x1ABC9C,
                IsActive = true
            },
            new Category
            {
                Id = 3,
                Name = "Событийное волонтёрство",
                Description = "Помощь в организации мероприятий: фестивалей, концертов, спортивных событий",
                ColorRgb = 0x9B59B6,
                IsActive = true
            }
        ];

        public List<Category> GetCopyOfTestCategories()
        {
            return [.. TestCategories.Select(c => new Category { 
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ColorRgb = c.ColorRgb,
                IsActive = c.IsActive
            })];
        }

        public async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Categories.AddRangeAsync(TestCategories);
            await context.SaveChangesAsync();
        }
    }
}
