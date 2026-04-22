using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    }
}
