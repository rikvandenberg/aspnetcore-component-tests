using System;
using System.Collections.Generic;

namespace Api.Shopify
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = default!;

        public string BodyHtml { get; set; } = default!;

        public string Vendor { get; set; } = default!;

        public string ProductType { get; set; } = default!;

        public DateTime CreatedAt { get; set; }

        public string Handle { get; set; } = default!;

        public DateTime UpdatedAt { get; set; }

        public DateTime PublishedAt { get; set; }

        public object TemplateSuffix { get; set; } = default!;

        public string Status { get; set; } = default!;

        public string PublishedScope { get; set; } = default!;

        public string Tags { get; set; } = default!;

        public string AdminGraphqlApiId { get; set; } = default!;

        public ICollection<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
    }
}