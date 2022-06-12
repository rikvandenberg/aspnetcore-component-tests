using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Api.Tests;

/// <summary>
/// Custom web application factory class.
/// </summary>
public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddJsonFile("appsettings.json");

            // Component test appsettings.json
            string folder = Path.GetDirectoryName(GetType().Assembly.Location);
            string file = "ComponentTest/component.test.json";
            configurationBuilder.AddJsonFile(Path.Combine(folder, file));
        });
        return base.CreateHost(builder);
    }
}
