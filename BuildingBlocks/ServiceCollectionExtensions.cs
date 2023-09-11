using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static void AddOptionsWithValidation<TOptions>(this IServiceCollection services, string configurationSectionName)
        where TOptions : class
    {
        services.AddOptions<TOptions>().BindConfiguration(configurationSectionName).ValidateDataAnnotations().ValidateOnStart();
    }
}