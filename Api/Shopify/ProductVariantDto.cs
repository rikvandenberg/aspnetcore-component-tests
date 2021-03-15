using System;

namespace Api.Shopify
{
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Sku { get; set; }
        public int Position { get; set; }
        public string InventoryPolicy { get; set; }
        public object CompareAtPrice { get; set; }
        public string FulfillmentService { get; set; }
        public string InventoryManagement { get; set; }
        public string Option1 { get; set; }
        public object Option2 { get; set; }
        public object Option3 { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Taxable { get; set; }
        public string Barcode { get; set; }
        public int Grams { get; set; }
        public int? ImageId { get; set; }
        public double Weight { get; set; }
        public string WeightUnit { get; set; }
        public int InventoryItemId { get; set; }
        public int InventoryQuantity { get; set; }
        public int OldInventoryQuantity { get; set; }
        public bool RequiresShipping { get; set; }
    }
}