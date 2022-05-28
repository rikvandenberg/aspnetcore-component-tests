using System.Linq;
using System.Net;
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
            HttpResponseMessage responseMessage =
                await _httpClient.GetAsync($"admin/api/2021-07/products/{productId}.json");
            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            responseMessage.EnsureSuccessStatusCode();
            GetProductResponseDto? response = await responseMessage.Content.ReadFromJsonAsync<GetProductResponseDto>();
            if (response?.Product is null)
            {
                return null;
            }
            return new Product(
                response.Product.Id.ToString(),
                response.Product.Variants.First().Price,
                0m,
                1);
        }
    }
}