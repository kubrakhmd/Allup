using Allup.DAL;
using Allup.Models;
using Allup.Utilities.Enums;
using Allup.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDBContext _context;

        public ShopController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<ActionResult> Index(int page = 1, int key = 1)
        {
            IQueryable<Product> query = _context.Products.Include(p => p.ProductImages.Where(i => i.IsPrimary != null));

            switch (key)
            {
                case (int)SortType.Name:
                    query = query.OrderBy(i => i.Name);
                    break;
                case (int)SortType.Price:
                    query = query.OrderByDescending(i => i.Price);
                    break;
                case (int)SortType.Date:
                    query = query.OrderByDescending(i => i.CreatedAt);
                    break;
            }

            int count = query.Count();
            double total = Math.Ceiling((double)count / 2);

            query = query.Skip((page - 1) * 2).Take(2);
            ShopVM shopvm = new ShopVM
            {
                TotalPage = total,
                CurrectPage = page,
                Key = key,
                ProductVM = await query.Select(q => new GetProductVM
                {
                    Id = q.Id,
                    Name = q.Name,
                    Price = q.Price,
                    Image = q.ProductImages.FirstOrDefault(q => q.IsPrimary == true).Image,


                }).ToListAsync()
            };
            return View(shopvm);
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || id <= 0) return BadRequest();
            Product product = await _context.Products.Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductColors)
                .ThenInclude(pc => pc.Color)
                .Include(p => p.ProductSizes)
                .ThenInclude(ps => ps.Size)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();
            DetailVM detailVM = new DetailVM
            {
                Product = product,
                RelatedProducts = await _context.Products
            .Where(p => p.CategoryId == product.CategoryId && p.Id != id && p.BrandId == product.BrandId)
            .Include(p => p.ProductImages
            .Where(pi => pi.IsPrimary != null))
            .Take(8)
           .ToListAsync()
            };

            return View(detailVM);
        }
    }
}
