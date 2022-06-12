using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Api.BusinessLayer;
using Api.DataLayer;
using Api.Shopify;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;

namespace Microsoft.Extensions.DependencyInjection;

internal static class StartupUtilities
{
    internal static void ConfigureDbContextOptions(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();
    }

    internal static void ConfigurePolly(this IServiceCollection services)
    {
        PolicyRegistry registry = new()
        {
            {
                "defaultJsonResponse", HttpPolicyExtensions.HandleTransientHttpError()
                    .OrResult(res => res.StatusCode == HttpStatusCode.Forbidden)
                    .FallbackAsync(_ => Task.FromResult(new HttpResponseMessage { Content = new StringContent("{}", Encoding.UTF8) }))
            }
        };
        services.AddPolicyRegistry(registry);
    }

    internal static void ConfigureApiLayer(this IServiceCollection services)
    {
        services.AddControllers();
    }

    internal static void ConfigureBusinessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ShopifySettings>()
            .Bind(configuration.GetSection("Shopify"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHttpClient<IProductsService, ShopifyProductsClient>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<ShopifySettings>>();
            client.BaseAddress = settings.Value.BaseUrl;
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", settings.Value.ApiKey);
        }).AddPolicyHandlerFromRegistry("defaultJsonResponse");
    }

    private sealed class ShopifySettings
    {
        [Required]
        public Uri BaseUrl { get; init; } = default!;
        [Required]
        public string ApiKey { get; init; } = default!;
    }

    internal static void ConfigureDataLayer(this IServiceCollection services)
    {
        services.AddDbContext<OrderDbContext>(
            optionsBuilder => ConfigureDbContextOptions(optionsBuilder.UseSqlite("Data Source=orders.db")),
            optionsLifetime: ServiceLifetime.Singleton);
        services.AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>));
    }

    internal static string ConfigureNotNullOrEmpty(this IConfiguration configuration, string configurationKey)
    {
        string value = configuration[configurationKey];
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(configurationKey);
        }
        return value;
    }
}