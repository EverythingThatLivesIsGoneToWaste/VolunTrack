using BCrypt.Net;
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
    public class UserRepositoryTests : IAsyncLifetime
    {
        private readonly UserFixture _userFixture;
        private readonly PostgreSqlContainerFixture _containerFixture;

        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserRepository _repository;

        // Testing data (copies from fixture)
        private List<User> _testUsers = null!;

        public UserRepositoryTests(PostgreSqlContainerFixture containerFixture)
        {
            _userFixture = new UserFixture();
            _containerFixture = containerFixture;

            _scope = _containerFixture.ServiceProvider.CreateScope();

            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _repository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
        }

        public async Task InitializeAsync()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.MigrateAsync();

            await _userFixture.SeedAsync(_dbContext);

            _testUsers = _userFixture.GetCopyOfTestUsers();
        }

        public Task DisposeAsync()
        {
            _scope?.Dispose();
            return Task.CompletedTask;
        }

        // Tests for GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
        {
            var expectedUser = _testUsers.First();

            var userInDb = await _repository.GetByIdAsync(expectedUser.Id);

            Assert.NotNull(userInDb);
            Assert.Equal(expectedUser.Id, userInDb.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            var nonexistentId = 500;

            var userInDb = await _repository.GetByIdAsync(nonexistentId);

            Assert.Null(userInDb);
        }

        // Tests for GetByLoginAsync
        [Fact]
        public async Task GetByLoginAsync_WhenUserExists_ShouldReturnUser()
        {
            var expectedUser = _testUsers.First();

            var userInDb = await _repository.GetByLoginAsync(expectedUser.Login);

            Assert.NotNull(userInDb);
            Assert.Equal(expectedUser.Login, userInDb.Login);
        }

        [Fact]
        public async Task GetByLoginAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            var nonexistentLogin = "Frog";

            var userInDb = await _repository.GetByLoginAsync(nonexistentLogin);

            Assert.Null(userInDb);
        }

        // Tests for SearchAsync
        [Fact]
        public async Task SearchAsync_WhenSearchTerm_ShouldReturnMatchingUsers()
        {
            var searchTerm = "Ni";
            var expectedCount = _testUsers
                .Count(u =>
                    u.Login.Contains(searchTerm) ||
                    u.FullName.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    u.Phone.Contains(searchTerm));

            var users = await _repository.SearchAsync(searchTerm);

            Assert.Equal(expectedCount, users.Count);
        }

        [Fact]
        public async Task SearchAsync_WhenRole_ShouldReturnUsersWithRole()
        {
            var role = UserRole.EventCoordinator;
            var expectedCount = _testUsers.Count(u => u.Role == role);

            var users = await _repository.SearchAsync(null, role);

            Assert.Equal(expectedCount, users.Count);
        }

        [Fact]
        public async Task SearchAsync_WhenIsActive_ShouldReturnActiveUsers()
        {
            var isActive = true;
            var expectedCount = _testUsers.Count(u => u.IsActive == isActive);

            var users = await _repository.SearchAsync(null, null, isActive);

            Assert.Equal(expectedCount, users.Count);
        }

        // Tests for ExistsByLoginAsync
        [Fact]
        public async Task ExistsByLoginAsync_WhenUserExists_ShouldReturnTrue()
        {
            var existingLogin = _testUsers.First().Login;

            var exists = await _repository.ExistsByLoginAsync(existingLogin);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByLoginAsync_WhenUserDoesNotExist_ShouldReturnFalse()
        {
            var nonexistentLogin = "WrongLogin";

            var exists = await _repository.ExistsByLoginAsync(nonexistentLogin);

            Assert.False(exists);
        }

        // Tests for ExistsByEmailAsync
        [Fact]
        public async Task ExistsByEmailAsync_WhenUserExists_ShouldReturnTrue()
        {
            var existingEmail = _testUsers.First().Email;

            var exists = await _repository.ExistsByEmailAsync(existingEmail);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByEmailAsync_WhenUserDoesNotExist_ShouldReturnFalse()
        {
            var nonexistentEmail = "WrongEmail@gmail.com";

            var exists = await _repository.ExistsByEmailAsync(nonexistentEmail);

            Assert.False(exists);
        }

        // Tests for AddAsync
        [Fact]
        public async Task AddAsync_WhenUserValid_ShouldAddUser()
        {
            var userToAdd = new User()
            {
                Id = 1,
                Login = "ChromeUser",
                FullName = "Freya Karr",
                Phone = "+7(982)545-12-22",
                Email = "IloveChrome2000@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(
                    "strongpassword",
                    HashType.SHA512,
                    workFactor: 12
                ),
                Role = UserRole.Volunteer,
                IsActive = true,
                CreatedAtUtc = new DateTime(2025, 7, 2, 7, 12, 45).ToUniversalTime()
            };

            await _repository.AddAsync(userToAdd);

            var addedUser = await _repository.GetByLoginAsync(userToAdd.Login);

            Assert.NotNull(addedUser);
            Assert.Equal(userToAdd.Login, addedUser.Login);
            Assert.Equal(userToAdd.FullName, addedUser.FullName);
            Assert.Equal(userToAdd.Email, addedUser.Email);
            Assert.Equal(userToAdd.Phone, addedUser.Phone);
            Assert.Equal(userToAdd.Role, addedUser.Role);
            Assert.Equal(userToAdd.IsActive, addedUser.IsActive);
            Assert.Equal(userToAdd.CreatedAtUtc, addedUser.CreatedAtUtc);
            Assert.True(BCrypt.Net.BCrypt.EnhancedVerify(
                "strongpassword",
                addedUser.PasswordHash,
                HashType.SHA512));
        }

        // Tests for UpdateAsync
        [Fact]
        public async Task UpdateAsync_WhenUserValid_ShouldUpdateUser()
        {
            var originalUser = _testUsers.First();
            var userToUpdate = await _repository.GetByLoginAsync(originalUser.Login);
            Assert.NotNull(userToUpdate);

            var newLogin = "NewAndImproved";
            var newFullName = "Updated Name";
            var newPhone = "+7(999)999-99-99";

            userToUpdate.Login = newLogin;
            userToUpdate.FullName = newFullName;
            userToUpdate.Phone = newPhone;

            await _repository.UpdateAsync(userToUpdate);

            var updatedUser = await _repository.GetByLoginAsync(newLogin);
            Assert.NotNull(updatedUser);

            Assert.Equal(newLogin, updatedUser.Login);
            Assert.Equal(newFullName, updatedUser.FullName);
            Assert.Equal(newPhone, updatedUser.Phone);

            Assert.Equal(originalUser.Email, updatedUser.Email);
            Assert.Equal(originalUser.Role, updatedUser.Role);
            Assert.Equal(originalUser.IsActive, updatedUser.IsActive);
            Assert.Equal(originalUser.CreatedAtUtc, updatedUser.CreatedAtUtc);

            var oldLoginUser = await _repository.GetByLoginAsync(originalUser.Login);
            Assert.Null(oldLoginUser);
        }

        // Tests for RemoveAsync
        [Fact]
        public async Task RemoveAsync_WhenUserValid_ShouldRemoveUser()
        {
            var originalUser = _testUsers.First();
            var userToRemove = await _repository.GetByLoginAsync(originalUser.Login);
            Assert.NotNull(userToRemove);
            var userId = userToRemove.Id;

            await _repository.RemoveAsync(userToRemove);

            var userById = await _repository.GetByIdAsync(userId);
            var userByLogin = await _repository.GetByLoginAsync(originalUser.Login);
            var existsByLogin = await _repository.ExistsByLoginAsync(originalUser.Login);

            Assert.Null(userById);
            Assert.Null(userByLogin);
            Assert.False(existsByLogin);
        }
    }
}
