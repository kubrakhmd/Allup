

namespace Allup.Models
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; } 
        public decimal Tax { get; set; }
        public string Description { get; set; }
        public string ProductCode { get; set; }
        public bool IsAvailable {  get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<ProductImage> ProductImages { get; set; }
        public int BrandId { get; set; }
        public Brand  Brand  { get; set; }
        public List<ProductColor> ProductColors { get; set; }
        public List<ProductSize> ProductSizes { get; set; }
        public List<ProductTag> ProductTags { get; set; }
    }

   
}
