using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using VolunTrack.Data;
using VolunTrack.Models;
using VolunTrack.Repositories;
using VolunTrack.Services;

namespace VolunTrack.Tests.Fixtures;

/// <summary>
/// Shared PostgreSQL  container fixture for all integration tests.
/// </summary>
public class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    private readonly IServiceProvider _serviceProvider;

    public string ConnectionString => _container.GetConnectionString();
    public IServiceProvider ServiceProvider => _serviceProvider;

    public PostgresOptions DbOptions { get; private set; }

    public PostgreSqlContainerFixture()
    {
        var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.Test.json", optional: false)
               .AddEnvironmentVariables()
               .Build();

        // Loading Db configuration
        DbOptions = configuration.GetSection("Database").Get<PostgresOptions>()
            ?? throw new InvalidOperationException("Database configuration not found");

        _container = new PostgreSqlBuilder("postgres:latest")
            .WithDatabase(DbOptions.Database)
            .WithUsername(DbOptions.Username)
            .WithPassword(DbOptions.Password)
            .WithPortBinding(DbOptions.Port, true)
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();

        _serviceProvider = ConfigureServices();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        RunMigrations();
    }

    private ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddHttpContextAccessor();

        // Registering DbContext (same as main project)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(5);
            })
        );

        // Registering login
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // Registering dependencies (must match main project)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IParticipationRepository, ParticipationRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IParticipationService, ParticipationService>();
        services.AddScoped<IEventService, EventService>();

        return services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private void RunMigrations()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }

    public ApplicationDbContext CreateDbContext()
    {
        return _serviceProvider.GetRequiredService<ApplicationDbContext>();
    }
}

/// <summary>
/// Collection definition for PostgreSQL tests
/// </summary>
[CollectionDefinition("PostgreSql")]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
}
