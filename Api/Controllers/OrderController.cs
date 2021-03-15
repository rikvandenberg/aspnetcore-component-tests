using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Order> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Date = DateTime.Now.AddDays(index),
                TotalAmount = rng.Next(15, 513),
                Products = Enumerable.Range(1, 3).Select(i => new Product
                {
                    Number = $"Product#{i}",
                    Quantity = 1,
                    Amount = 121,
                    Vat = 21m
                }).ToList(),
            });
        }
    }
}