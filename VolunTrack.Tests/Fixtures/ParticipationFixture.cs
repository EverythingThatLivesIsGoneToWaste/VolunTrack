using VolunTrack.Data;
using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.Tests.Fixtures
{
    public class ParticipationFixture
    {
        public List<Participation> TestParticipations { get; private set; } = [];

        public List<Participation> GetCopyOfTestParticipations()
        {
            return [.. TestParticipations.Select(p => new Participation
            {
                Id = p.Id,
                UserId = p.UserId,
                EventId = p.EventId,
                Status = p.Status,
                CheckInTime = p.CheckInTime,
                CheckOutTime = p.CheckOutTime,
                TotalHours = p.TotalHours,
                IsManualCheckOut = p.IsManualCheckOut,
                IsConfirmedByCoordinator = p.IsConfirmedByCoordinator,
                IsConfirmedByLeader = p.IsConfirmedByLeader,
                HoursModerated = p.HoursModerated,
                CreatedAtUtc = p.CreatedAtUtc
            })];
        }

        public async Task SeedAsync(ApplicationDbContext context, List<Event> events, List<User> users) {
            TestParticipations =
            [
                new()
                {
                    UserId = users[0].Id,
                    EventId = events[0].Id,
                    Status = ParticipationStatus.Pending,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    UserId = users[0].Id,
                    EventId = events[3].Id,
                    Status = ParticipationStatus.Approved,
                    CheckInTime = events[3].StartDateTime,
                    CheckOutTime = events[3].EndDateTime,
                    TotalHours = 3,
                    IsConfirmedByCoordinator = true,
                    IsConfirmedByLeader = true,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-7)
                }
            ];

            await context.Participations.AddRangeAsync(TestParticipations);
            await context.SaveChangesAsync();
        }
    }
}
