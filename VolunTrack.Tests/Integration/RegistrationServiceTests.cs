using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VolunTrack.Data;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Models;
using VolunTrack.Repositories;
using VolunTrack.Services;
using VolunTrack.Tests.Fixtures;

namespace VolunTrack.Tests.Integration
{
    [Collection("PostgreSql")]
    public class RegistrationServiceTests : IAsyncLifetime
    {
        private readonly UserFixture _userFixture;
        private readonly PostgreSqlContainerFixture _containerFixture;

        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _dbContext;
        private readonly IRegistrationService _service;
        private readonly IUserRepository _repository;

        // Testing data (copies from fixture)
        private List<User> _testUsers = null!;

        public RegistrationServiceTests(PostgreSqlContainerFixture containerFixture) {
            _userFixture = new UserFixture();
            _containerFixture = containerFixture;

            _scope = _containerFixture.ServiceProvider.CreateScope();

            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _service = _scope.ServiceProvider.GetRequiredService<IRegistrationService>();
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

        [Fact]
        public async Task RegisterAsync_WhenDtoValid_ShouldRegisterUser()
        {
            var registerDto = new RegisterDto { 
                Login = "Romestos",
                FullName = "Роман Нессельроде",
                Phone = "+7 (927)545-11-22",
                Email = "KurVIP@gmail.com",
                Password = "verystrongpassword",
                CategoryIds = [1, 2, 3]
            };

            var registrationResponse = await _service.RegisterAsync(registerDto);

            Assert.True(registrationResponse.IsSuccess);
            Assert.Equal(RegistrationStatus.Success, registrationResponse.Status);
            Assert.NotNull(registrationResponse.User);

            Assert.Equal(registerDto.Login, registrationResponse.User.Login);
            Assert.Equal(registerDto.FullName, registrationResponse.User.FullName);

            var registeredUser = await _repository.GetByLoginAsync(registerDto.Login);
            Assert.NotNull(registeredUser);
            Assert.NotEqual(registerDto.Password, registeredUser.PasswordHash);
            Assert.Equal(registerDto.FullName, registeredUser.FullName);

            Assert.True(registeredUser.CreatedAtUtc <= DateTime.UtcNow);

            var userCategories = await _repository.GetUserCategoriesAsync(registeredUser.Id);

            Assert.Equal(3, userCategories.Count);
            Assert.Contains(userCategories, c => c.Id == 1);
            Assert.Contains(userCategories, c => c.Id == 2);
            Assert.Contains(userCategories, c => c.Id == 3);
        }

        [Fact]
        public async Task RegisterAsync_WhenLoginExists_ShouldReturnLoginAlreadyExistsErrorResponse()
        {
            var existingLogin = _testUsers.First().Login;

            var registerDto = new RegisterDto
            {
                Login = existingLogin,
                FullName = "Роман Нессельроде",
                Phone = "+7 (927)545-11-22",
                Email = "KurVIP@gmail.com",
                Password = "verystrongpassword",
                CategoryIds = [1, 2, 3]
            };

            var registrationResponse = await _service.RegisterAsync(registerDto);

            Assert.False(registrationResponse.IsSuccess);
            Assert.Equal(RegistrationStatus.LoginAlreadyExists, registrationResponse.Status);
            Assert.Null(registrationResponse.User);

            var existingUser = await _repository.GetByLoginAsync(registerDto.Login);
            Assert.NotNull(existingUser);
            Assert.Equal(_testUsers[0].Login, existingUser.Login);
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailExists_ShouldReturnEmailAlreadyExistsErrorResponse()
        {
            var existingEmail = _testUsers.First().Email;

            var registerDto = new RegisterDto
            {
                Login = "Romestos",
                FullName = "Роман Нессельроде",
                Phone = "+7 (927)545-11-22",
                Email = existingEmail,
                Password = "verystrongpassword",
                CategoryIds = [1, 2, 3]
            };

            var registrationResponse = await _service.RegisterAsync(registerDto);

            Assert.False(registrationResponse.IsSuccess);
            Assert.Equal(RegistrationStatus.EmailAlreadyExists, registrationResponse.Status);
            Assert.Null(registrationResponse.User);

            var existingUser = await _repository.GetByLoginAsync(registerDto.Login);
            Assert.Null(existingUser);
        }

        [Fact]
        public async Task RegisterAsync_WhenLoginInvalid_ShouldReturnInvalidLoginErrorResponse()
        {
            var invalidLogin = "Jo";

            var registerDto = new RegisterDto
            {
                Login = invalidLogin,
                FullName = "Роман Нессельроде",
                Phone = "+7 (927)545-11-22",
                Email = "KurVIP@gmail.com",
                Password = "verystrongpassword",
                CategoryIds = [1, 2, 3]
            };

            var registrationResponse = await _service.RegisterAsync(registerDto);

            Assert.False(registrationResponse.IsSuccess);
            Assert.Equal(RegistrationStatus.InvalidLogin, registrationResponse.Status);
            Assert.Null(registrationResponse.User);

            var existingUser = await _repository.GetByLoginAsync(registerDto.Login);
            Assert.Null(existingUser);
        }

        [Fact]
        public async Task RegisterAsync_WhenPasswordInvalid_ShouldReturnInvalidPasswordErrorResponse()
        {
            var invalidPassword = "strng";

            var registerDto = new RegisterDto
            {
                Login = "Romestos",
                FullName = "Роман Нессельроде",
                Phone = "+7 (927)545-11-22",
                Email = "KurVIP@gmail.com",
                Password = invalidPassword,
                CategoryIds = [1, 2, 3]
            };

            var registrationResponse = await _service.RegisterAsync(registerDto);

            Assert.False(registrationResponse.IsSuccess);
            Assert.Equal(RegistrationStatus.InvalidPassword, registrationResponse.Status);
            Assert.Null(registrationResponse.User);

            var existingUser = await _repository.GetByLoginAsync(registerDto.Login);
            Assert.Null(existingUser);
        }
    }
}
