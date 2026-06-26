using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VolunTrack.Data;
using VolunTrack.Enums;
using VolunTrack.Models;
using VolunTrack.Repositories;
using VolunTrack.Services;
using VolunTrack.Tests.Fixtures;

namespace VolunTrack.Tests.Integration
{
    [Collection("PostgreSql")]
    public class ParticipationServiceTests : IAsyncLifetime
    {
        private readonly UserFixture _userFixture;
        private readonly CategoryFixture _categoryFixture;
        private readonly ParticipationFixture _participationFixture;
        private readonly EventFixture _eventFixture;
        private readonly PostgreSqlContainerFixture _containerFixture;

        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _dbContext;
        private readonly IParticipationService _participationService;
        private readonly IParticipationRepository _participationRepository;

        // Testing data (copies from fixture)
        private List<User> _testUsers = null!;
        private List<Category> _testCategories = null!;
        private List<Participation> _testParticipations = null!;
        private List<Event> _testEvents = null!;

        public ParticipationServiceTests(PostgreSqlContainerFixture containerFixture) 
        {
            _containerFixture = containerFixture;
            _userFixture = new UserFixture();
            _categoryFixture = new CategoryFixture();
            _participationFixture = new ParticipationFixture();
            _eventFixture = new EventFixture();

            _scope = _containerFixture.ServiceProvider.CreateScope();

            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _participationService = _scope.ServiceProvider.GetRequiredService<IParticipationService>();
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

            await _participationFixture.SeedAsync(_dbContext, _testEvents, _testUsers);
            _testParticipations = _participationFixture.GetCopyOfTestParticipations();
        }

        public Task DisposeAsync()
        {
            _scope?.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task JoinAsync_ValidEvent_ShouldCreateParticipation()
        {
            var userId = _testUsers[1].Id;
            var @event = _testEvents[0];

            var result = await _participationService.JoinAsync(userId, @event.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(JoinEventStatus.Success, result.Status);

            var participation = await _participationRepository.GetByUserAndEventAsync(userId, @event.Id);

            Assert.NotNull(participation);
            Assert.Equal(@event.Id, participation.EventId);
            Assert.Equal(ParticipationStatus.Pending, participation.Status);
            Assert.Equal(0, participation.TotalHours);
            Assert.False(participation.IsConfirmedByCoordinator);
            Assert.False(participation.IsConfirmedByLeader);
            Assert.False(participation.IsManualCheckOut);
            Assert.False(participation.HoursModerated);
        }

        [Fact]
        public async Task JoinAsync_EventNotFound_ShouldReturnEventNotFound()
        {
            var userId = _testUsers[1].Id;
            var nonexistentId = 500;

            var result = await _participationService.JoinAsync(userId, nonexistentId);

            Assert.False(result.IsSuccess);
            Assert.Equal(JoinEventStatus.EventNotFound, result.Status);

            var participation = await _participationRepository.GetByUserAndEventAsync(userId, nonexistentId);

            Assert.Null(participation);
        }

        [Fact]
        public async Task JoinAsync_EventCancelled_ShouldReturnEventCancelled()
        {
            var userId = _testUsers[1].Id;
            var cancelledEvent = _testEvents.First(e => e.Status == EventStatus.Cancelled);

            var result = await _participationService.JoinAsync(userId, cancelledEvent.Id);

            Assert.False(result.IsSuccess);
            Assert.Equal(JoinEventStatus.EventCancelled, result.Status);

            var participation = await _participationRepository.GetByUserAndEventAsync(userId, cancelledEvent.Id);

            Assert.Null(participation);
        }

        [Fact]
        public async Task JoinAsync_EventNotPublished_ShouldReturnEventNotPublished()
        {
            var userId = _testUsers[1].Id;
            var draftEvent = _testEvents.First(e => e.Status == EventStatus.Draft);

            var result = await _participationService.JoinAsync(userId, draftEvent.Id);

            Assert.False(result.IsSuccess);
            Assert.Equal(JoinEventStatus.EventNotPublished, result.Status);

            var participation = await _participationRepository.GetByUserAndEventAsync(userId, draftEvent.Id);

            Assert.Null(participation);
        }

        [Fact]
        public async Task JoinAsync_EventAlreadyStarted_ShouldReturnEventAlreadyStarted()
        {
            var userId = _testUsers[1].Id;
            var startedEvent = _testEvents.First(e => e.StartDateTime <= DateTime.UtcNow);

            var result = await _participationService.JoinAsync(userId, startedEvent.Id);

            Assert.False(result.IsSuccess);
            Assert.Equal(JoinEventStatus.EventAlreadyStarted, result.Status);

            var participation = await _participationRepository.GetByUserAndEventAsync(userId, startedEvent.Id);

            Assert.Null(participation);
        }

        [Fact]
        public async Task JoinAsync_AlreadyJoined_ShouldReturnAlreadyJoined()
        {
            var userId = _testParticipations[0].UserId;
            var eventId = _testParticipations[0].EventId;

            var result = await _participationService.JoinAsync(userId, eventId);

            Assert.False(result.IsSuccess);
            Assert.Equal(JoinEventStatus.AlreadyJoined, result.Status);

            var participation = await _participationRepository.GetByUserAndEventAsync(userId, eventId);

            Assert.NotNull(participation);
        }
    }
}
