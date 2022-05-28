using System;
using System.ComponentModel.DataAnnotations;

namespace Api.ApiLayer
{
    public class CreateOrderDto
    {
        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public string[] ProductNumbers { get; set; } = default!;
    }
}