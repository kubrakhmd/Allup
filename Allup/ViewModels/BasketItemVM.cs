namespace Allup.ViewModels
{
    public class BasketItemVM
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public int Count { get; set; }
    }
}
