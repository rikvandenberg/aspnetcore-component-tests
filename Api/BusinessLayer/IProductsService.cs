using System.Threading.Tasks;

namespace Api.BusinessLayer
{
    public interface IProductsService
    {
        Task<Product?> GetProductAsync(string productId);
    }
}