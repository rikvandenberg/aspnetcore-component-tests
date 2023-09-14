
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Api.IntegrationTests;

public abstract class ContainerFixture : IAsyncLifetime
{
    protected ContainerFixture()
    {
        Container = BuildContainer();
    }

    protected abstract IContainer BuildContainer();

    // Expose container as property so you can use it to build your URLS and Connection strings.
    public IContainer Container { get; }

    public virtual Task InitializeAsync()
    {
        // Make sure the container starts so it can be used
        return Container.StartAsync();
    }

    public virtual async Task DisposeAsync()
    {
        // Clean up our container nicely.
        if (Container is not null)
        {
            await Container.StopAsync();
            await Container.DisposeAsync();
        }
    }
}

public class PostgresContainerFixture : ContainerFixture
{
    protected override IContainer BuildContainer()
    {
        return new ContainerBuilder()
            // Which image?
            .WithImage("postgres:latest")
            // Some basic env variables
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithEnvironment("POSTGRES_DB", "orders")
            // Random ports if we need to
            .WithPortBinding(5432, assignRandomHostPort: true)
            // We wait until postgres is started up
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }
}