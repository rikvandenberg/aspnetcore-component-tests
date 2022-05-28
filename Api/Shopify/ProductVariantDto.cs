using System;

namespace Api.Shopify
{
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; } = default!;
        public decimal Price { get; set; }
        public string Sku { get; set; } = default!;
        public int Position { get; set; }
        public string InventoryPolicy { get; set; } = default!;
        public object CompareAtPrice { get; set; } = default!;
        public string FulfillmentService { get; set; } = default!;
        public string InventoryManagement { get; set; } = default!;
        public string Option1 { get; set; } = default!;
        public object Option2 { get; set; } = default!;
        public object Option3 { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Taxable { get; set; }
        public string Barcode { get; set; } = default!;
        public int Grams { get; set; }
        public int? ImageId { get; set; }
        public double Weight { get; set; }
        public string WeightUnit { get; set; } = default!;
        public int InventoryItemId { get; set; }
        public int InventoryQuantity { get; set; }

        public int OldInventoryQuantity { get; set; }
        public bool RequiresShipping { get; set; }
    }
}