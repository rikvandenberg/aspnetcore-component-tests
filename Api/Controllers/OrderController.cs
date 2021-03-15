using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.BusinessLayer;
using Api.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IRepository<Order> _repository;
        private readonly IProductsService _productsService;

        public OrderController(IRepository<Order> repository, IProductsService productsService)
        {
            _repository = repository;
            _productsService = productsService;
        }

        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return _repository.AllRecords.Include(o => o.Lines).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateOrderDto request)
        {
            Order order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Date = DateTime.UtcNow,
                TotalAmount = request.TotalAmount,
            };
            foreach (string productNumber in request.ProductNumbers)
            {
                Product? product = await _productsService.GetProductAsync(productNumber);
                if (product == null)
                {
                    return BadRequest($"Unable to find product: {productNumber}");
                }
                order.Lines.Add(new OrderLine
                {
                    OrderId = order.Id,
                    Amount = product.Price,
                    ProductNumber = product.Id,
                    Quantity = product.Quantity,
                    Vat = product.Vat
                });
            }

            await _repository.CreateAsync(order);

            return Ok(order);
        }
    }
}