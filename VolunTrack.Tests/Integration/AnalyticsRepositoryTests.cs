using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunTrack.Data;
using VolunTrack.Enums;
using VolunTrack.Models;
using VolunTrack.Repositories;
using VolunTrack.Tests.Fixtures;

namespace VolunTrack.Tests.Integration
{
    [Collection("PostgreSql")]
    public class AnalyticsRepositoryTests : IAsyncLifetime
    {
        private readonly UserFixture _userFixture;
        private readonly CategoryFixture _categoryFixture;
        private readonly EventFixture _eventFixture;
        private readonly PostgreSqlContainerFixture _containerFixture;

        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEventRepository _eventRepository;
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly IParticipationRepository _participationRepository;

        // Testing data (copies from fixture)
        private List<User> _testUsers = null!;
        private List<Category> _testCategories = null!;
        private List<Event> _testEvents = null!;

        public AnalyticsRepositoryTests(PostgreSqlContainerFixture containerFixture)
        {
            _containerFixture = containerFixture;
            _userFixture = new UserFixture();
            _categoryFixture = new CategoryFixture();
            _eventFixture = new EventFixture();

            _scope = _containerFixture.ServiceProvider.CreateScope();

            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _eventRepository = _scope.ServiceProvider.GetRequiredService<IEventRepository>();
            _analyticsRepository = _scope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();
            _participationRepository = _scope.ServiceProvider.GetRequiredService<IParticipationRepository>();
        }

        public async Task InitializeAsync()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.MigrateAsync();

            await _userFixture.SeedAsync(_dbContext);
            _testUsers = _userFixture.GetCopyOfTestUsers();

            await _categoryFixture.SeedAsync(_dbContext);
            _testCategories = _categoryFixture.GetCopyOfTestCategories();

            var coordinator = _testUsers.FirstOrDefault(u => u.Role == UserRole.EventCoordinator) ??
                throw new InvalidOperationException("No coordinator found in test users");

            await _eventFixture.SeedAsync(_dbContext, _testCategories, coordinator.Id);
            _testEvents = _eventFixture.GetCopyOfTestEvents();
        }

        public Task DisposeAsync()
        {
            _scope?.Dispose();
            return Task.CompletedTask;
        }

        // Tests for GetUserHoursStatsAsync
        [Fact]
        public async Task GetUserHoursStatsAsync_WhenUserHasParticipations_ShouldReturnHoursGroupedByStatus()
        {
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);
            var event1 = _testEvents[0];
            var event2 = _testEvents[1];

            var participationApproved = new Participation
            {
                UserId = volunteer.Id,
                EventId = event1.Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 5.5m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(5.5)
            };
            await _participationRepository.AddAsync(participationApproved);

            var participationPending = new Participation
            {
                UserId = volunteer.Id,
                EventId = event2.Id,
                Status = ParticipationStatus.Pending,
                TotalHours = 3.0m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(3)
            };
            await _participationRepository.AddAsync(participationPending);

            var participationRejected = new Participation
            {
                UserId = volunteer.Id,
                EventId = event2.Id,
                Status = ParticipationStatus.Rejected,
                TotalHours = 6.2m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(6.2)
            };
            await _participationRepository.AddAsync(participationRejected);

            var result = await _analyticsRepository.GetUserHoursStatsAsync(volunteer.Id);

            Assert.Equal(3, result.Count);
            Assert.Equal(5.5m, result["Approved"]);
            Assert.Equal(3.0m, result["Pending"]);
            Assert.Equal(6.2m, result["Rejected"]);
        }

        [Fact]
        public async Task GetUserHoursStatsAsync_WhenNoParticipations_ShouldReturnEmptyDictionary()
        {
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);

            var result = await _analyticsRepository.GetUserHoursStatsAsync(volunteer.Id);

            Assert.Empty(result);
        }

        // Tests for GetUserFavoriteCategoryAsync
        [Fact]
        public async Task GetUserFavoriteCategoryAsync_WhenUserHasParticipations_ShouldReturnTargetCategory()
        {
            var targetCategory = _testCategories.First();
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);

            var events = await _eventRepository.GetByCategoryIdsAsync([targetCategory.Id]);

            Assert.NotEmpty(events);

            var participationApproved = new Participation
            {
                UserId = volunteer.Id,
                EventId = events[0].Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 5.5m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(5.5)
            };
            await _participationRepository.AddAsync(participationApproved);

            var participationPending = new Participation
            {
                UserId = volunteer.Id,
                EventId = events[1].Id,
                Status = ParticipationStatus.Pending,
                TotalHours = 3.0m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(3)
            };
            await _participationRepository.AddAsync(participationPending);

            var result = await _analyticsRepository.GetUserFavoriteCategoryAsync(volunteer.Id);

            Assert.NotNull(result);
            Assert.Equal(targetCategory.Id, result.Id);
        }

        [Fact]
        public async Task GetUserFavoriteCategoryAsync_WhenNoParticipations_ShouldReturnNull()
        {
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);

            var result = await _analyticsRepository.GetUserFavoriteCategoryAsync(volunteer.Id);

            Assert.Null(result);
        }

        // Tests for GetTotalConfirmedHoursAsync
        [Fact]
        public async Task GetTotalConfirmedHoursAsync_WhenUserHasParticipations_ShouldReturnHoursCount()
        {
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);
            var event1 = _testEvents[0];
            var event2 = _testEvents[1];

            var participationApproved = new Participation
            {
                UserId = volunteer.Id,
                EventId = event1.Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 5.5m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(5.5)
            };
            await _participationRepository.AddAsync(participationApproved);

            var participationPending = new Participation
            {
                UserId = volunteer.Id,
                EventId = event2.Id,
                Status = ParticipationStatus.Pending,
                TotalHours = 3.0m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(3)
            };
            await _participationRepository.AddAsync(participationPending);

            var participationRejected = new Participation
            {
                UserId = volunteer.Id,
                EventId = event2.Id,
                Status = ParticipationStatus.Rejected,
                TotalHours = 6.2m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(6.2)
            };
            await _participationRepository.AddAsync(participationRejected);

            var result = await _analyticsRepository.GetTotalConfirmedHoursAsync(volunteer.Id);

            Assert.Equal(5.5m, result);
        }

        [Fact]
        public async Task GetTotalConfirmedHoursAsync_WhenNoParticipations_ShouldReturnZero()
        {
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);

            var result = await _analyticsRepository.GetTotalConfirmedHoursAsync(volunteer.Id);

            Assert.Equal(0.0m, result);
        }

        // Tests for GetUserEventsStatsAsync
        [Fact]
        public async Task GetUserEventsStatsAsync_WhenCalledByVolunteer_ShouldReturnUserStatistics()
        {
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);
            var completedEvents = _testEvents.Where(e => e.Status == EventStatus.Completed).ToList();

            var ecoCategory = _testCategories.First(c => c.Name == "Экологическое волонтёрство");
            var socialCategory = _testCategories.First(c => c.Name == "Социальное волонтёрство");
            var eventCategory = _testCategories.First(c => c.Name == "Событийное волонтёрство");

            var participation_1 = new Participation
            {
                UserId = volunteer.Id,
                EventId = completedEvents[0].Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 5.5m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(5.5)
            };
            await _participationRepository.AddAsync(participation_1);

            var participation_2 = new Participation
            {
                UserId = volunteer.Id,
                EventId = completedEvents[1].Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 3.0m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(3)
            };
            await _participationRepository.AddAsync(participation_2);

            var participation_3 = new Participation
            {
                UserId = volunteer.Id,
                EventId = completedEvents[2].Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 6.2m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(6.2)
            };
            await _participationRepository.AddAsync(participation_3);

            var result = await _analyticsRepository.GetUserEventsStatsAsync(volunteer.Id);

            var expectedCategoryCounts = new Dictionary<string, int>
            {
                { ecoCategory.Name, 1 },
                { socialCategory.Name, 2 },
                { eventCategory.Name, 1 }
            };

            foreach (var kvp in expectedCategoryCounts)
            {
                Assert.Equal(kvp.Value, result[kvp.Key].Count);
            }
        }

        [Fact]
        public async Task GetUserEventsStatsAsync_WhenCalledByAdmin_ShouldReturnGlobalStatistics()
        {
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);
            var completedEvents = _testEvents.Where(e => e.Status == EventStatus.Completed).ToList();

            var ecoCategory = _testCategories.First(c => c.Name == "Экологическое волонтёрство");
            var socialCategory = _testCategories.First(c => c.Name == "Социальное волонтёрство");
            var eventCategory = _testCategories.First(c => c.Name == "Событийное волонтёрство");

            var participation_1 = new Participation
            {
                UserId = volunteer.Id,
                EventId = completedEvents[0].Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 5.5m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(5.5)
            };
            await _participationRepository.AddAsync(participation_1);

            var participation_2 = new Participation
            {
                UserId = volunteer.Id,
                EventId = completedEvents[1].Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 3.0m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(3)
            };
            await _participationRepository.AddAsync(participation_2);

            var participation_3 = new Participation
            {
                UserId = volunteer.Id,
                EventId = completedEvents[2].Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 6.2m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(6.2)
            };
            await _participationRepository.AddAsync(participation_3);

            var result = await _analyticsRepository.GetUserEventsStatsAsync();

            var expectedCategoryCounts = new Dictionary<string, int>
            {
                { ecoCategory.Name, 1 },
                { socialCategory.Name, 2 },
                { eventCategory.Name, 1 }
            };

            foreach (var kvp in expectedCategoryCounts)
            {
                Assert.Equal(kvp.Value, result[kvp.Key].Count);
            }
        }

        [Fact]
        public async Task GetUserEventsStatsAsync_WhenNoParticipations_ShouldReturnEmptyDictionary()
        {
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);

            var result = await _analyticsRepository.GetUserEventsStatsAsync();

            Assert.Empty(result);
        }

        // Tests for GetParticipantsCountByEventsAsync
        [Fact]
        public async Task GetParticipantsCountByEventsAsync_WhenThreeParticipations_ShouldReturnParticipantsStatistics()
        {
            var volunteers = _testUsers.Where(u => u.Role == UserRole.Volunteer).ToList();

            var completedEvents = _testEvents.Where(e => e.Status == EventStatus.Completed).ToList();
            var event1 = completedEvents[0];
            var event2 = completedEvents[1];

            var participation_1 = new Participation
            {
                UserId = volunteers[0].Id,
                EventId = event1.Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 5.5m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(5.5)
            };
            await _participationRepository.AddAsync(participation_1);

            var participation_2 = new Participation
            {
                UserId = volunteers[1].Id,
                EventId = event1.Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 6.2m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(6.2)
            };
            await _participationRepository.AddAsync(participation_2);

            var participation_3 = new Participation
            {
                UserId = volunteers[0].Id,
                EventId = event2.Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 3.0m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(3)
            };
            await _participationRepository.AddAsync(participation_3);

            var result = await _analyticsRepository.GetParticipantsCountByEventsAsync();
            var resultByEventId = result.ToDictionary(k => k.Key.Id, v => v.Value);

            Assert.Equal(3, result.Count);
            Assert.Equal(2, resultByEventId[event1.Id]);
            Assert.Equal(1, resultByEventId[event2.Id]);
        }

        [Fact]
        public async Task GetParticipantsCountByEventsAsync_WhenEventsExistWithoutParticipants_ShouldReturnZeroForThoseEvents()
        {
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);

            var result = await _analyticsRepository.GetParticipantsCountByEventsAsync();

            Assert.Equal(3, result.Count);
            foreach (var value in result.Values)
            {
                Assert.Equal(0, value);
            }
        }

        // Tests for GetCompletedEventsByMonthAsync
        [Fact]
        public async Task GetCompletedEventsByMonthAsync_WhenEventsExistInMultipleMonths_ShouldReturnGroupedByMonth()
        {
            var result = await _analyticsRepository.GetCompletedEventsByMonthAsync(DateTime.UtcNow.Year);

            Assert.Equal(3, result.Count);
            foreach (var value in result.Values)
            {
                Assert.Equal(1, value);
            }
        }

        // Tests for GetTotalHoursByCategoryAsync
        [Fact]
        public async Task GetTotalHoursByCategoryAsync_WhenEventsExist_ShouldReturnCategoriesStatistics()
        {
            var volunteer = _testUsers.First(u => u.Role == UserRole.Volunteer);
            var completedEvents = _testEvents.Where(e => e.Status == EventStatus.Completed).ToList();

            var ecoCategory = _testCategories.First(c => c.Name == "Экологическое волонтёрство");
            var socialCategory = _testCategories.First(c => c.Name == "Социальное волонтёрство");
            var eventCategory = _testCategories.First(c => c.Name == "Событийное волонтёрство");

            var participation_1 = new Participation
            {
                UserId = volunteer.Id,
                EventId = completedEvents[0].Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 5.5m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(5.5)
            };
            await _participationRepository.AddAsync(participation_1);

            var participation_2 = new Participation
            {
                UserId = volunteer.Id,
                EventId = completedEvents[1].Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 3.0m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(3)
            };
            await _participationRepository.AddAsync(participation_2);

            var participation_3 = new Participation
            {
                UserId = volunteer.Id,
                EventId = completedEvents[2].Id,
                Status = ParticipationStatus.Approved,
                TotalHours = 6.2m,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow.AddHours(6.2)
            };
            await _participationRepository.AddAsync(participation_3);

            var result = await _analyticsRepository.GetTotalHoursByCategoryAsync();

            var expectedCategoryCounts = new Dictionary<string, decimal>
            {
                { ecoCategory.Name, 5.5m },
                { socialCategory.Name, 8.5m },
                { eventCategory.Name, 6.2m }
            };

            foreach (var kvp in expectedCategoryCounts)
            {
                Assert.Equal(kvp.Value, result[kvp.Key]);
            }
        }

        // Tests for GetTotalActiveUsersAsync
        [Fact]
        public async Task GetTotalActiveUsersAsync_WhenActiveUsersExist_ShouldReturnCountOfActiveUsers()
        {
            var activeUsers = _testUsers.Where(u => u.IsActive).ToList();
            var result = await _analyticsRepository.GetTotalActiveUsersAsync();

            Assert.Equal(activeUsers.Count, result);
        }

        // Tests for GetUserRegistrationsByMonthAsync
        [Fact]
        public async Task GetUserRegistrationsByMonthAsync_WhenActiveUsersExist_ShouldReturnRegistrationsGroupedByMonth()
        {
            var result = await _analyticsRepository.GetUserRegistrationsByMonthAsync(DateTime.UtcNow.Year);

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[1]);
            Assert.Equal(1, result[3]);
            Assert.Equal(1, result[4]);
        }
    }
}
