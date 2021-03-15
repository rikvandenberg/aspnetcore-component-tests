using System;
using System.Collections.Generic;

namespace Api.Shopify
{
    public class ProductDto
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string BodyHtml { get; set; }

        public string Vendor { get; set; }

        public string ProductType { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Handle { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime PublishedAt { get; set; }

        public object TemplateSuffix { get; set; }

        public string Status { get; set; }

        public string PublishedScope { get; set; }

        public string Tags { get; set; }

        public string AdminGraphqlApiId { get; set; }

        public List<ProductVariantDto> Variants { get; set; }
    }
}