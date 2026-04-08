
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VolunTrack.Data;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Exceptions;
using VolunTrack.Models;
using VolunTrack.Repositories;
using VolunTrack.Services;
using VolunTrack.Tests.Fixtures;

namespace VolunTrack.Tests.Integration
{
    [Collection("PostgreSql")]
    public class EventServiceTests : IAsyncLifetime
    {
        private readonly UserFixture _userFixture;
        private readonly CategoryFixture _categoryFixture;
        private readonly EventFixture _eventFixture;
        private readonly PostgreSqlContainerFixture _containerFixture;

        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEventService _eventService;
        private readonly IEventRepository _eventRepository;
        private readonly IParticipationRepository _participationRepository;

        // Testing data (copies from fixture)
        private List<User> _testUsers = null!;
        private List<Category> _testCategories = null!;
        private List<Event> _testEvents = null!;

        public EventServiceTests(PostgreSqlContainerFixture containerFixture)
        {
            _containerFixture = containerFixture;
            _userFixture = new UserFixture();
            _categoryFixture = new CategoryFixture();
            _eventFixture = new EventFixture();

            _scope = _containerFixture.ServiceProvider.CreateScope();

            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            _eventRepository = _scope.ServiceProvider.GetRequiredService<IEventRepository>();
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

        // Tests for AddAsync
        [Fact]
        public async Task AddAsync_ValidEvent_ShouldCreateEvent()
        {
            var coordinator = _testUsers.FirstOrDefault(u => u.Role == UserRole.EventCoordinator);

            var createEventDto = new CreateEventDto()
            {
                Name = "Уборка площади",
                Description = "Уборка территории площади",
                Place = "Центральная площадь",
                StartDateTime = DateTime.UtcNow.AddDays(5),
                EndDateTime = DateTime.UtcNow.AddDays(5).AddHours(3),
                SkillsRequired = "",
                CategoryIds = [.._testCategories.Take(2).Select(c => c.Id)],
                CreatedByUserId = coordinator!.Id,
            };

            var eventDto = await _eventService.AddAsync(createEventDto);
            var eventInDb = await _eventRepository.GetByIdAsync(eventDto.Id);

            Assert.NotNull(eventInDb);
            Assert.Equal(eventDto.Id, eventInDb.Id);
            Assert.Equal(EventStatus.Draft, eventInDb.Status);
            Assert.Equal(coordinator.Id, eventInDb.CreatedByUserId);

            Assert.Equal(2, eventDto.Categories.Count);
            Assert.Contains(eventDto.Categories, c => c.Id == _testCategories[0].Id);
            Assert.Contains(eventDto.Categories, c => c.Id == _testCategories[1].Id);
        }

        [Fact]
        public async Task AddAsync_EndDateBeforeStartDate_ShouldThrowArgumentException()
        {
            var coordinator = _testUsers.FirstOrDefault(u => u.Role == UserRole.EventCoordinator);

            var createEventDto = new CreateEventDto()
            {
                Name = "Уборка площади",
                Description = "Уборка территории площади",
                Place = "Центральная площадь",
                StartDateTime = DateTime.UtcNow.AddDays(5).AddHours(3),
                EndDateTime = DateTime.UtcNow.AddDays(5),
                SkillsRequired = "",
                CategoryIds = [.. _testCategories.Take(2).Select(c => c.Id)],
                CreatedByUserId = coordinator!.Id,
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _eventService.AddAsync(createEventDto)
            );

            Assert.Equal("End date must be after start date", exception.Message);
        }

        [Fact]
        public async Task AddAsync_CategoriesNotFound_ShouldThrowArgumentException()
        {
            var coordinator = _testUsers.FirstOrDefault(u => u.Role == UserRole.EventCoordinator);

            var createEventDto = new CreateEventDto()
            {
                Name = "Уборка площади",
                Description = "Уборка территории площади",
                Place = "Центральная площадь",
                StartDateTime = DateTime.UtcNow.AddDays(5),
                EndDateTime = DateTime.UtcNow.AddDays(5).AddHours(3),
                SkillsRequired = "",
                CategoryIds = [.. _testCategories.Take(2).Select(c => c.Id)],
                CreatedByUserId = coordinator!.Id,
            };

            createEventDto.CategoryIds.Add(500);

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _eventService.AddAsync(createEventDto)
            );

            Assert.Equal("Some categories not found", exception.Message);
        }

        // Tests for UpdateStatusAsync
        [Fact]
        public async Task UpdateStatusAsync_CoordinatorUpdatesOwnEvent_ShouldUpdateStatus()
        {
            var @event = _testEvents[0];
            var creatorId = @event.CreatedByUserId;
            var result = await _eventService.UpdateStatusAsync(
                @event.Id, 
                EventStatus.Published, 
                (int)creatorId!, 
                nameof(UserRole.EventCoordinator));

            Assert.Equal(EventStatus.Published, result.Status);

            var eventInDb = await _eventRepository.GetByIdAsync(@event.Id);
            Assert.Equal(EventStatus.Published, eventInDb!.Status);
        }

        [Fact]
        public async Task UpdateStatusAsync_AdminUpdatesAnyEvent_ShouldUpdate()
        {
            var admin = _testUsers.First(u => u.Role == UserRole.Administrator);

            var @event = _testEvents[1];
            var result = await _eventService.UpdateStatusAsync(
                @event.Id, 
                EventStatus.Published,
                admin.Id,
                nameof(UserRole.Administrator));

            Assert.Equal(EventStatus.Published, result.Status);

            var eventInDb = await _eventRepository.GetByIdAsync(@event.Id);
            Assert.Equal(EventStatus.Published, eventInDb!.Status);
        }

        [Fact]
        public async Task UpdateStatusAsync_EventDoesNotExist_ShouldThrowNotFoundException()
        {
            var nonexistentEventId = 500;

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _eventService.UpdateStatusAsync(
                    nonexistentEventId,
                    EventStatus.Published,
                    999,
                    nameof(UserRole.Administrator)
                ));

            Assert.Equal($"Event {nonexistentEventId} not found", exception.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_DifferentCoordinatorUpdatesEvent_ShouldThrowUnauthorizedAccessException()
        {
            var @event = _testEvents[1];

            var exception = await Assert.ThrowsAsync<Exceptions.UnauthorizedAccessException>(
                () => _eventService.UpdateStatusAsync(
                    @event.Id,
                    EventStatus.Published,
                    999,
                    nameof(UserRole.EventCoordinator)
                ));

            Assert.Equal("You don't have permission to change this event status", exception.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_VolunteerUpdatesEvent_ShouldThrowUnauthorizedAccessException()
        {
            var @event = _testEvents[1];

            var exception = await Assert.ThrowsAsync<Exceptions.UnauthorizedAccessException>(
                () => _eventService.UpdateStatusAsync(
                    @event.Id,
                    EventStatus.Published,
                    999,
                    nameof(UserRole.Volunteer)
                ));

            Assert.Equal("You don't have permission to change this event status", exception.Message);
        }

        // Tests for GetEventsAsync
        [Fact]
        public async Task GetEventsAsync_RequestingUpcoming_ShouldReturnUpcomingEvents()
        {
            var type = "upcoming";
            var user = _testUsers.First(u => u.Role == UserRole.Volunteer);

            var upcomingEvents = await _eventService.GetEventsAsync(type, user.Id, nameof(user.Role));

            foreach (var e in upcomingEvents)
            {
                var isJoined = await _participationRepository.ExistsAsync(user.Id, e.Id);
                Assert.Equal(isJoined, e.IsJoined);
            }

            var count = _testEvents.Where(e => e.StartDateTime > DateTime.UtcNow 
            && e.Status == EventStatus.Published).ToList().Count;
            Assert.Equal(upcomingEvents.Count, count);

            Assert.DoesNotContain(upcomingEvents, e => e.Status == EventStatus.Draft);
            Assert.DoesNotContain(upcomingEvents, e => e.Status == EventStatus.Cancelled);
        }

        [Fact]
        public async Task GetEventsAsync_CoordinatorRequestingAll_ShouldReturnEventsByCoordinator()
        {
            var type = "my";
            var user = _testUsers.First(u => u.Role == UserRole.EventCoordinator);

            var eventsByCoordinator = await _eventService.GetEventsAsync(type, user.Id, nameof(user.Role));

            var count = _testEvents.Where(e => e.CreatedByUserId == user.Id).ToList().Count;
            Assert.Equal(eventsByCoordinator.Count, count);
            Assert.All(eventsByCoordinator, e => Assert.Equal(user.Id, e.CreatedByUserId));
        }

        [Fact]
        public async Task GetEventsAsync_RegionCoordinatorRequestingAll_ShouldReturnAllEvents()
        {
            var type = "my";
            var user = _testUsers.First(u => u.Role == UserRole.RegionCoordinator);

            var allEvents = await _eventService.GetEventsAsync(type, user.Id, nameof(user.Role));

            Assert.Equal(_testEvents.Count, allEvents.Count);
        }

        [Fact]
        public async Task GetEventsAsync_RequestingAll_ShouldReturnAllEvents()
        {
            var type = "my";
            var user = _testUsers.First(u => u.Role == UserRole.Administrator);

            var allEvents = await _eventService.GetEventsAsync(type, user.Id, nameof(user.Role));

            var count = _testEvents.Count;
            Assert.Equal(allEvents.Count, count);
        }

        // Tests for AssignEventLeaderAsync
        [Fact]
        public async Task AssignEventLeaderAsync_WhenTargetIsVolunteer_ShouldAssignLeader()
        {
            var @event = _testEvents[0];
            var targetUserId = _testUsers.FirstOrDefault(u => u.Role == UserRole.Volunteer)!.Id;
            var currentUserId = (int)@event.CreatedByUserId!;
            var userRole = nameof(UserRole.EventCoordinator);

            var assignedUser = await _eventService.AssignEventLeaderAsync(@event.Id, targetUserId, currentUserId, userRole);

            Assert.NotNull(assignedUser);
            Assert.Equal(targetUserId, assignedUser.Id);

            var isLeader = await _eventRepository.IsUserLeaderOfEventAsync(@event.Id, targetUserId);
            Assert.True(isLeader);
        }

        [Fact]
        public async Task AssignEventLeaderAsync_WhenTargetIsEventCoordinator_ShouldThrowArgumentException()
        {
            var targetUser = _testUsers.FirstOrDefault(u => u.Role == UserRole.EventCoordinator)!;
            var targetUserId = targetUser.Id;

            var @event = _testEvents[0];
            var currentUserId = (int)@event.CreatedByUserId!;
            var currentUserRole = nameof(UserRole.EventCoordinator);

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _eventService.AssignEventLeaderAsync(@event.Id, targetUserId, currentUserId, currentUserRole)
            );

            Assert.Equal("Administrators and event coordinators cannot be assigned as leaders", exception.Message);
        }

        [Fact]
        public async Task AssignEventLeaderAsync_WhenCoordinatorNotAuthor_ShouldThrowUnauthorizedAccessException()
        {
            var targetUser = _testUsers.FirstOrDefault(u => u.Role == UserRole.Volunteer)!;
            var targetUserId = targetUser.Id;

            var @event = _testEvents[0];
            var idNotAuthoredByCurrentUser = 500;
            var currentUserId = idNotAuthoredByCurrentUser;
            var currentUserRole = nameof(UserRole.EventCoordinator);

            var exception = await Assert.ThrowsAsync<Exceptions.UnauthorizedAccessException>(
                () => _eventService.AssignEventLeaderAsync(@event.Id, targetUserId, currentUserId, currentUserRole)
            );

            Assert.Equal("No permission to assign leaders to this event", exception.Message);
        }

        [Fact]
        public async Task AssignEventLeaderAsync_WhenTargetIsAlreadyLeader_ShouldThrowAlreadyLeaderException()
        {
            var @event = _testEvents[0];
            var targetUserId = _testUsers.FirstOrDefault(u => u.Role == UserRole.Volunteer)!.Id;
            var currentUserId = (int)@event.CreatedByUserId!;
            var currentUserRole = nameof(UserRole.EventCoordinator);

            var assignedUser = await _eventService.AssignEventLeaderAsync(@event.Id, targetUserId, currentUserId, currentUserRole);

            Assert.NotNull(assignedUser);
            Assert.Equal(targetUserId, assignedUser.Id);

            var isLeader = await _eventRepository.IsUserLeaderOfEventAsync(@event.Id, targetUserId);
            Assert.True(isLeader);

            var exception = await Assert.ThrowsAsync<AlreadyLeaderException>(
                ()=> _eventService.AssignEventLeaderAsync(@event.Id, targetUserId, currentUserId, currentUserRole)
            );

            Assert.Equal($"User {targetUserId} is already a leader of event {@event.Id}", exception.Message);
        }

        [Fact]
        public async Task AssignEventLeaderAsync_WhenLeaderLimitExceeded_ShouldThrowEventLeaderLimitExceededException()
        {
            var testVolunteers = _testUsers.Where(u => u.Role == UserRole.Volunteer).ToList();

            var @event = _testEvents[0];
            var currentUserId = (int)@event.CreatedByUserId!;
            var currentUserRole = nameof(UserRole.EventCoordinator);

            for (int i = 0; i < 2; i++)
            {
                var assignedUser = await _eventService.AssignEventLeaderAsync(@event.Id, testVolunteers[i].Id, currentUserId, currentUserRole);

                Assert.NotNull(assignedUser);
                Assert.Equal(testVolunteers[i].Id, assignedUser.Id);

                var isLeader = await _eventRepository.IsUserLeaderOfEventAsync(@event.Id, testVolunteers[i].Id);
                Assert.True(isLeader);
            }

            // Third leader assignment throws error
            var thirdVolunteerId = testVolunteers[2].Id;
            var exception = await Assert.ThrowsAsync<EventLeaderLimitExceededException>(
                () => _eventService.AssignEventLeaderAsync(@event.Id, thirdVolunteerId, currentUserId, currentUserRole)
            );

            Assert.Equal($"Event {@event.Id} already has the maximum allowed number of leaders (2)", exception.Message);
        }
    }
}