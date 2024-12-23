using Allup.Models;

namespace Allup.Areas.Admin
{
    public class CreateCategoryVM
    {
        public string Name { get; set; }
        public List <Product>? Products   { get; set; }

    }
}
