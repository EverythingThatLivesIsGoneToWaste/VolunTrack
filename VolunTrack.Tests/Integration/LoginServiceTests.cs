using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VolunTrack.Data;
using VolunTrack.Exceptions;
using VolunTrack.Models;
using VolunTrack.Services;
using VolunTrack.Tests.Fixtures;

namespace VolunTrack.Tests.Integration
{
    [Collection("PostgreSql")]
    public class LoginServiceTests : IAsyncLifetime
    {
        private readonly UserFixture _userFixture;
        private readonly PostgreSqlContainerFixture _containerFixture;

        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoginService _service;

        // Testing data (copies from fixture)
        private List<User> _testUsers = null!;

        public LoginServiceTests(PostgreSqlContainerFixture containerFixture)
        {
            _containerFixture = containerFixture;
            _userFixture = new UserFixture();

            _scope = _containerFixture.ServiceProvider.CreateScope();

            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _service = _scope.ServiceProvider.GetRequiredService<ILoginService>();
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
        public async Task AuthenticateAsync_ValidCredentials_ShouldAuthenticate()
        {
            var validUser = _testUsers[0];

            var user = await _service.AuthenticateAsync(validUser.Login, "strongpassword");

            Assert.NotNull(user);
            Assert.Equal(validUser.Id, user.Id);
        }

        [Fact]
        public async Task AuthenticateAsync_NonExistentLogin_ShouldThrowUnauthorizedException()
        {
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(
                () => _service.AuthenticateAsync("nonexistent", "password")
            );

            Assert.Equal("Invalid login or password", exception.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_InvalidPassword_ShouldThrowUnauthorizedException()
        {
            var validUser = _testUsers[0];

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(
                () => _service.AuthenticateAsync(validUser.Login, "wrong-password")
            );

            Assert.Equal("Invalid login or password", exception.Message);
        }
    }
}
