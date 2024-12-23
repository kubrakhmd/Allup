namespace Allup.ViewModels
{
    public class ShopVM
    {
        public int Key { get; set; }
        public double TotalPage { get; set; }
        public int CurrectPage { get; set; }

        public List<GetProductVM> ProductVM { get; set; }

    }
}
