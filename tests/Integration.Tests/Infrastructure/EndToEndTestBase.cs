using GamificationEngine.Infrastructure.Storage.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GamificationEngine.Integration.Tests.Infrastructure;

[Trait("Category", "E2E")]
public abstract class EndToEndTestBase : IntegrationTestBase
{
    protected HttpClient HttpClient;
    protected GamificationEngineDbContext DbContext;

    protected EndToEndTestBase()
    {
        HttpClient = CreateJsonClient();
        var sc = GetService<IServiceCollection>();

        ConfigureTestServices(sc);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        // Add Entity Framework services
        services.AddDbContext<GamificationEngineDbContext>(options =>
        {
            options.UseInMemoryDatabase($"test-db-{Guid.NewGuid()}");
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
    }
}