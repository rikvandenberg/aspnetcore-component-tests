using System.Collections.Generic;
using System.Linq;
using Api.DataLayer;
using Microsoft.AspNetCore.Mvc;

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
            return _repository.AllRecords.ToList();
        }
    }
}