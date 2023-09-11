using System.ComponentModel.DataAnnotations;

namespace Microsoft.Extensions.DependencyInjection;

internal static partial class StartupUtilities
{
    internal sealed class ShopifySettings
    {
        [Required]
        public Uri BaseUrl { get; init; } = default!;
        [Required]
        public string ApiKey { get; init; } = default!;
    }
}