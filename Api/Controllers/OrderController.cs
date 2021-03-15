using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public OrderController(IRepository<Order> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return _repository.AllRecords.Include(o => o.Lines).ToList();
        }

        [HttpPost]
        public async Task Post(CreateOrderDto request)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Date = DateTime.UtcNow,
                TotalAmount = request.TotalAmount,
            };
            foreach (string product in request.ProductNumbers)
            {
                order.Lines.Add(new OrderLine
                {
                    Amount = 0,
                    OrderId = order.Id,
                    ProductNumber = product,
                    Quantity = 1,
                    Vat = 0,
                });
            }

            await _repository.CreateAsync(order);
        }
    }
}