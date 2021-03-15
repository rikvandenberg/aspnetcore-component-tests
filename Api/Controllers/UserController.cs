using System;
using System.Collections.Generic;
using System.Linq;
using Api.DataLayer;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UserController : ControllerBase
    {
        private readonly IRepository<Order> _repository;

        public UserController(IRepository<Order> repository)
        {
            _repository = repository;
        }

        [Route("/{userId}/orders")]
        [HttpGet]
        public IEnumerable<Order> Get(Guid userId)
        {
            return _repository.AllRecords.Where(order => order.UserId == userId).ToList();
        }
    }
}