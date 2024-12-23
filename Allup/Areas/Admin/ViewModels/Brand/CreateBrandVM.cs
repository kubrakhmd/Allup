using Allup.Models;

namespace Allup.Areas.Admin.ViewModels
{
    public class CreateBrandVM
    {
        public string Name { get; set; }
        public List <Product>? Products   { get; set; }

    }
}
