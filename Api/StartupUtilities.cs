using System.Net;
using System.Text;
using Api.BusinessLayer;
using Api.DataLayer;
using Api.Shopify;
using Microsoft.EntityFrameworkCore;
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
        const string ShopifyBaseUrl = "Shopify:BaseUrl";
        const string ShopifyApiKey = "Shopify:ApiKey";
        configuration.ConfigureNotNullOrEmpty(ShopifyBaseUrl);
        configuration.ConfigureNotNullOrEmpty(ShopifyApiKey);
        services.AddHttpClient<IProductsService, ShopifyProductsClient>(client =>
        {
            client.BaseAddress = new Uri(configuration[ShopifyBaseUrl]);
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", configuration[ShopifyApiKey]);
        }).AddPolicyHandlerFromRegistry("defaultJsonResponse");
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