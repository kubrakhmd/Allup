using Allup.Areas.Admin.ViewModels;
using Allup.DAL;
using Allup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandController : Controller
    {
        private readonly AppDBContext _context;

        public BrandController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<GetBrandVM> brandVMs = await _context.Brands.Where(c => !c.IsDeleted)
                .Include(c => c.Products)
                .Select(c => new GetBrandVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = c.Products.Count
                }).ToListAsync();
            return View(brandVMs);
        }
        public IActionResult Create()
        {


            return View();

        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateBrandVM brandVM)
        {
            if (!ModelState.IsValid)
            {

                return View();
            }

            bool result = await _context.Brands.AnyAsync(c => c.Name.Trim() == brandVM.Name.Trim());

            if (result)
            {
                ModelState.AddModelError("Name", "Name Already exists");
                return View();

            }
           Brand brand =new()
            {
                Name = brandVM.Name
            };
            brand.CreatedAt = DateTime.Now;
            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Brand brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);

            if (brand is null) return NotFound();

            return View(brand);

        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, Brand brand)
        {
            if (id == null || id < 1) return BadRequest();

            Brand existed = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);

            if (brand is null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = await _context.Brands.AnyAsync(b => b.Name.Trim() == brand.Name.Trim() && b.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(Brand.Name), "Brand already exists");
                return View();
            }
            existed.Name = brand.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Brand brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);

            if (brand is null) return NotFound();


            brand.IsDeleted = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

    }
}
