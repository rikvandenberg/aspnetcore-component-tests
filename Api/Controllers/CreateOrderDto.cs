using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers
{
    public class CreateOrderDto
    {
        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public string[] ProductNumbers { get; set; }
    }
}