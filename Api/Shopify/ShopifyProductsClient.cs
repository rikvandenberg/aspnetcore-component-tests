using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Api.BusinessLayer;

namespace Api.Shopify
{
    public class ShopifyProductsClient : IProductsService
    {
        private readonly HttpClient _httpClient;

        public ShopifyProductsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Product?> GetProductAsync(string productId)
        {
            GetProductResponseDto response =
                await _httpClient.GetFromJsonAsync<GetProductResponseDto>($"/products/{productId}.json");
            if (response?.Product != null)
            {
                return new Product
                {
                    Id = response.Product.Id,
                    Price = response.Product.Variants.First().Price,
                    Vat = 0m,
                    Quantity = 1,
                };
            }
            return null;
        }
    }
}