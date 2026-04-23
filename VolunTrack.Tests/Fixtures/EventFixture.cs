using VolunTrack.Data;
using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.Tests.Fixtures
{
    public class EventFixture
    {
        public List<Event> TestEvents { get; private set; } = [];

        public List<Event> GetCopyOfTestEvents()
        {
            return [.. TestEvents.Select(e => new Event
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Place = e.Place,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                SkillsRequired = e.SkillsRequired,
                Status = e.Status,
                CreatedByUserId = e.CreatedByUserId,
                CreatedAtUtc = e.CreatedAtUtc
            })];
        }

        public async Task SeedAsync(ApplicationDbContext context, List<Category> categories, int coordinatorId) {
            TestEvents =
            [
                new()
                {
                    Name = "Уборка парка",
                    Description = "Уборка территории парка",
                    Place = "Центральный парк",
                    StartDateTime = DateTime.UtcNow.AddDays(5),
                    EndDateTime = DateTime.UtcNow.AddDays(5).AddHours(3),
                    SkillsRequired = "",
                    Status = EventStatus.Published,
                    CreatedByUserId = coordinatorId,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Отменённый субботник",
                    Description = "Отменённое мероприятие",
                    Place = "Городской сад",
                    StartDateTime = DateTime.UtcNow.AddDays(10),
                    EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(4),
                    SkillsRequired = "",
                    Status = EventStatus.Cancelled,
                    CreatedByUserId = coordinatorId,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Черновик мероприятия",
                    Description = "Неопубликованное",
                    Place = "Школа №1",
                    StartDateTime = DateTime.UtcNow.AddDays(7),
                    EndDateTime = DateTime.UtcNow.AddDays(7).AddHours(2),
                    SkillsRequired = "",
                    Status = EventStatus.Draft,
                    CreatedByUserId = coordinatorId,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Прошедшее событие",
                    Description = "Уже началось",
                    Place = "Стадион",
                    StartDateTime = DateTime.UtcNow.AddHours(-2),
                    EndDateTime = DateTime.UtcNow.AddHours(1),
                    SkillsRequired = "",
                    Status = EventStatus.Published,
                    CreatedByUserId = coordinatorId,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-7)
                },
                new()
                {
                    Name = "Завершённое событие 1",
                    Description = "Для аналитики",
                    Place = "Место 1",
                    StartDateTime = DateTime.UtcNow.AddMonths(-3),
                    EndDateTime = DateTime.UtcNow.AddMonths(-3).AddHours(3),
                    SkillsRequired = "",
                    Status = EventStatus.Completed,
                    CreatedByUserId = coordinatorId,
                    CreatedAtUtc = DateTime.UtcNow.AddMonths(-3)
                },
                new()
                {
                    Name = "Завершённое событие 2",
                    Description = "Для аналитики",
                    Place = "Место 2",
                    StartDateTime = DateTime.UtcNow.AddMonths(-2),
                    EndDateTime = DateTime.UtcNow.AddMonths(-2).AddHours(4),
                    SkillsRequired = "",
                    Status = EventStatus.Completed,
                    CreatedByUserId = coordinatorId,
                    CreatedAtUtc = DateTime.UtcNow.AddMonths(-2)
                },
                new()
                {
                    Name = "Завершённое событие 3",
                    Description = "Для аналитики",
                    Place = "Место 3",
                    StartDateTime = DateTime.UtcNow.AddMonths(-1),
                    EndDateTime = DateTime.UtcNow.AddMonths(-1).AddHours(2),
                    SkillsRequired = "",
                    Status = EventStatus.Completed,
                    CreatedByUserId = coordinatorId,
                    CreatedAtUtc = DateTime.UtcNow.AddMonths(-1)
                }
            ];

            await context.Events.AddRangeAsync(TestEvents);
            await context.SaveChangesAsync();

            var eventCategories = new List<EventCategory>
            {
                new() { EventId = 1, CategoryId = categories[0].Id },
                new() { EventId = 1, CategoryId = categories[1].Id },
                new() { EventId = 2, CategoryId = categories[0].Id },
                new() { EventId = 3, CategoryId = categories[1].Id },
                new() { EventId = 4, CategoryId = categories[2].Id },

                new() { EventId = 5, CategoryId = categories[0].Id },
                new() { EventId = 5, CategoryId = categories[1].Id },
                new() { EventId = 6, CategoryId = categories[1].Id },
                new() { EventId = 7, CategoryId = categories[2].Id },
            };

            await context.EventCategories.AddRangeAsync(eventCategories);
            await context.SaveChangesAsync();
        }
    }
}
