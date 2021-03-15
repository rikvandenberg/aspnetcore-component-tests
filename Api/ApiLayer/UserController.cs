using System;
using System.Collections.Generic;
using System.Linq;
using Api.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.ApiLayer
{
    [ApiController]
    public class UserController
    {
        private readonly IRepository<Order> _repository;

        public UserController(IRepository<Order> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Route("api/v1/users/{userId}/orders")]
        public IEnumerable<Order> Get(Guid userId)
        {
            return _repository.AllRecords.Include(o => o.Lines).Where(order => order.UserId == userId).ToList();
        }
    }
}