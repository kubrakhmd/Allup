namespace Allup.Models
{
    public class BasketItems
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int Count { get; set; }
    }
}
