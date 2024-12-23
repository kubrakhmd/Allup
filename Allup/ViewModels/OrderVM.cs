namespace Allup.ViewModels
{
    public class OrderVM
    {
        public string Address { get; set; }
        public string Number { get; set; }

        public List<BasketInOrdersVM>? BasketinOrders { get; set; }
    }
}
