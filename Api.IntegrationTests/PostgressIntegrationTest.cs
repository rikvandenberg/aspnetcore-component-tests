using System.Net;
using System.Net.Http.Json;
using Api.DataLayer;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Api.IntegrationTests;

public class PostgressIntegrationTest : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly ITestOutputHelper _testOutputHelper;

    public IContainer Container { get; }

    public PostgressIntegrationTest(ITestOutputHelper testOutputHelper)
    {
        _webApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(CustomizeWebHostBuilder);
        _testOutputHelper = testOutputHelper;
        Container = new ContainerBuilder()
            .WithImage("postgres:latest")
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithEnvironment("POSTGRES_DB", "orders")
            .WithPortBinding(5432, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    private void CustomizeWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
        webHostBuilder
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddXunit(_testOutputHelper);
            })
            .ConfigureTestServices(services =>
            {
                // Depending on the LifetimeScope of your options, it might either must be Scoped or Singleton. Default it's Scoped
                services.AddSingleton((serviceProvider) =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>().
                        UseNpgsql($"Host={Container.Hostname};Port={Container.GetMappedPublicPort(5432)};Database=orders;Username=postgres;Password=postgres");
                    Startup.ConfigureDbContextOptions(optionsBuilder);
                    return optionsBuilder.Options;
                });
            });
    }

    [Fact]
    public async Task When_get_order_is_called_should_return_ok()
    {
        // Arrange

        // Act
        HttpResponseMessage response = await SystemUnderTest.GetAsync("/api/v1/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<Order[]>();
        orders.Length.Should().Be(0);
    }

    [Fact]
    public async Task When_get_order_is_called_twice_should_return_orders()
    {
        // Act #1
        HttpResponseMessage getAllResponse = await SystemUnderTest.GetAsync("/api/v1/orders");

        // Assert #1
        var orders = await getAllResponse.Content.ReadFromJsonAsync<Order[]>();
        orders.Length.Should().Be(0);

        // Arrange #2
        var ctx = _webApplicationFactory.Services.CreateScope().ServiceProvider.GetRequiredService<OrderDbContext>();
        ctx.Order.Add(new Order()
        {
            Date = DateTime.UtcNow,
            TotalAmount = 50,
        });
        await ctx.SaveChangesAsync();

        // Act #2
        getAllResponse = await SystemUnderTest.GetAsync("/api/v1/orders");

        // Assert #2
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        orders = await getAllResponse.Content.ReadFromJsonAsync<Order[]>();
        orders.Length.Should().Be(1);
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();
    }

    public HttpClient SystemUnderTest =>
        _webApplicationFactory.CreateClient();
}
