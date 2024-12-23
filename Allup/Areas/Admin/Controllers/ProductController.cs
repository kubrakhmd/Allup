using Allup.Areas.ViewModels;
using Allup.DAL;
using Allup.Models;
using Allup.Utilities.Enums;
using Allup.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _env;
        public readonly string root = Path.Combine("assets", "images");
        public ProductController(AppDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }



        public async Task<IActionResult> Index()
        {
            List<GetProductAdminVM> productVM = await _context.Products
                .Where(p => p.IsDeleted == false)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages.Where(i => i.IsPrimary == true))
                 .Select(p => new GetProductAdminVM
                 {
                     Id = p.Id,
                     Name = p.Name,
                     Price = p.Price,
                     ProductCode = p.ProductCode,
                     CategoryName = p.Category.Name,
                     BrandName = p.Brand.Name,
                     Image = p.ProductImages.FirstOrDefault(i => i.IsPrimary == true).Image
                 }

                 ).ToListAsync();


            return View(productVM);
        }

        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
                Categories = await _context.Categories.ToListAsync(),
                Brands = await _context.Brands.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),


            };
            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Brands = await _context.Brands.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(productVM.MainPhoto), "Wrong Type!");
                return View(productVM);
            }
            if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError(nameof(productVM.MainPhoto), "Image size must contains max 2 MG");
                return View(productVM);
            }


            if (!productVM.HoverPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(productVM.HoverPhoto), "Wrong Type!");
                return View(productVM);
            }
            if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError(nameof(productVM.HoverPhoto), "Image size must contains max 2 MB");
                return View(productVM);
            }


            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(productVM.CategoryId), "Wrong Category!");
                return View(productVM);
            }
            if (productVM.TagIds is not null)
            {
                bool tagresult = productVM.TagIds.Any(ti => !productVM.Tags.Exists(t => t.Id == ti));
                if (tagresult)
                {
                    ModelState.AddModelError(nameof(CreateProductVM.TagIds), "This Tag does not exist!");
                    return View(productVM);
                }
            }
            ProductImage main = new()
            {
                Image = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, root),
                IsDeleted = false,
                CreatedAt = DateTime.Now,
                IsPrimary = true

            };
            ProductImage hover = new()
            {
                Image = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, root),
                CreatedAt = DateTime.Now,
                IsPrimary = false

            };

            Product product = new()
            {
                Name = productVM.Name,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                Price = productVM.Price.Value,
                Description = productVM.Description,
                ProductCode = productVM.ProdcutCode,
                IsAvailable = productVM.IsAvailable,
                CategoryId = productVM.CategoryId.Value,
                BrandId = productVM.BrandId.Value,

                ProductImages = new List<ProductImage> { main, hover }

            };
            if (productVM.TagIds is not null)
            {
                product.ProductTags = productVM.TagIds.Select(ti => new ProductTag { TagId = ti }).ToList();
            }

            if (productVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;

                foreach (var file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text += $"<div class=\"alert alert-warning\" role=\"alert\">\r\n {file.FileName} type is not correct!!\r\n</div>";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 2))
                    {
                        text += $"<div class=\"alert alert-warning\" role=\"alert\">\r\n {file.FileName} size is not correct!!\r\n</div>";
                        continue;
                    }

                    product.ProductImages.Add(new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, root),
                        IsDeleted = false,
                        CreatedAt = DateTime.Now,
                        IsPrimary = null

                    });

                    TempData["FileErrors"] = text;
                }
            }
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            Product product = await _context.Products.Include(p => p.ProductTags).Include(p => p.ProductImages).Include(p => p.ProductTags).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

            UpdateProductVM productVM = new()
            {
                Name = product.Name,
                Price = product.Price,
                ProdcutCode = product.ProductCode,
                IsAvailable = product.IsAvailable,
                Description = product.Description,

                CategoryId = product.CategoryId,
                Categories = await _context.Categories.ToListAsync(),

                BrandId = product.BrandId,
                Brands = await _context.Brands.ToListAsync(),

                Tags = await _context.Tags.ToListAsync(),
                TagIds = product.ProductTags.Select(p => p.TagId).ToList(),

                ProductImages = product.ProductImages,
            };
            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateProductVM productVM, int? id)
        {
            if (id is null || id < 1) return BadRequest();
            Product existed = await _context.Products.Include(p => p.ProductTags).Include(p => p.ProductImages).Include(p => p.ProductTags).FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();

            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Brands = await _context.Brands.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            productVM.ProductImages = existed.ProductImages;


            if (!ModelState.IsValid)
            {
                return View(productVM);
            }

            if (productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.MainPhoto), "Wrong Format!");
                    return View(productVM);
                }
                if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.MainPhoto), "Wrong Size!");
                    return View(productVM);
                }
            }
            if (productVM.HoverPhoto is not null)
            {

                if (!productVM.HoverPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.HoverPhoto), "Wrong Format!");
                    return View(productVM);
                }
                if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.HoverPhoto), "Wrong Size!");
                    return View(productVM);
                }

            }

            if (existed.CategoryId != productVM.CategoryId)
            {
                bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
                if (!result)
                {
                    ModelState.AddModelError(nameof(Category.Id), "This category does not exist!");
                    return View(productVM);
                }
            }

            if (existed.BrandId != productVM.BrandId)
            {
                bool result = await _context.Brands.AnyAsync(c => c.Id == productVM.BrandId);
                if (!result)
                {
                    ModelState.AddModelError(nameof(Brand.Id), "This brand does not exist!");
                    return View(productVM);
                }
            }

            if (productVM.TagIds is not null)
            {
                bool tagresult = productVM.TagIds.Any(ti => !productVM.Tags.Exists(t => t.Id == ti));
                if (tagresult)
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.TagIds), "This Tag Does not Exist");
                    return View(productVM);
                }

            }


            if (productVM.TagIds is null)
            {
                productVM.TagIds = new();
            }
            else
            {
                productVM.TagIds = productVM.TagIds.Distinct().ToList();
            }

            _context.ProductTags.RemoveRange(existed.ProductTags
            .Where(pt => !productVM.TagIds
            .Exists(t => t == pt.TagId)).ToList());

            _context.ProductTags.AddRange(productVM.TagIds
                .Where(ti => !existed.ProductTags
                .Exists(pt => pt.TagId == ti))
                .Select(ti => new ProductTag { TagId = ti, ProductId = existed.Id })
                .ToList());

            if (productVM.MainPhoto is not null)
            {
                string file = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, root);
                ProductImage main = existed.ProductImages.FirstOrDefault(i => i.IsPrimary == true);
                main.Image.DeleteFile(_env.WebRootPath, root);
                existed.ProductImages.Remove(main);

                existed.ProductImages.Add(new ProductImage
                {
                    Image = file,
                    IsDeleted = false,
                    IsPrimary = true,
                    CreatedAt = DateTime.Now,

                });
            }

            if (productVM.HoverPhoto is not null)
            {
                string file = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, root);
                ProductImage hover = existed.ProductImages.FirstOrDefault(i => i.IsPrimary == false);
                hover.Image.DeleteFile(_env.WebRootPath, root);
                existed.ProductImages.Remove(hover);

                existed.ProductImages.Add(new ProductImage
                {
                    Image = file,
                    IsDeleted = false,
                    IsPrimary = false,
                    CreatedAt = DateTime.Now,
                });
            }

            if (productVM.ImageIds is null)
            {
                productVM.ImageIds = new List<int>();
            }
            var deletedfiles = existed.ProductImages.Where(i => !productVM.ImageIds.Exists(id => id == i.Id) && i.IsPrimary == null).ToList();
            deletedfiles.ForEach(di => di.Image.DeleteFile(_env.WebRootPath, root));

            _context.ProductImages.RemoveRange(deletedfiles);

            if (productVM.AdditionalPhotos is not null)
            {
                String text = string.Empty;
                foreach (var image in productVM.AdditionalPhotos)
                {
                    if (!image.ValidateType("image/"))
                    {
                        text += $"<div class=\"alert alert-warning\" role=\"alert\">\r\n {image.FileName} Type is not correct!!\r\n</div>";
                        continue;
                    }
                    if (!image.ValidateSize(FileSize.MB, 2))
                    {
                        text += $"<div class=\"alert alert-warning\" role=\"alert\">\r\n {image.FileName} Size is not correct!!\r\n</div>";
                        continue;
                    }

                    existed.ProductImages.Add(new ProductImage
                    {
                        Image = await image.CreateFileAsync(_env.WebRootPath, root),
                        IsDeleted = false,
                        IsPrimary = null,
                        CreatedAt = DateTime.Now,
                    });

                }
                TempData["ErrorMessage"] = text;
            }
            existed.Name = productVM.Name;
            existed.Description = productVM.Description;
            existed.Price = productVM.Price.Value;
            existed.IsAvailable = productVM.IsAvailable;
            existed.ProductCode = productVM.ProdcutCode;
            existed.CategoryId = productVM.CategoryId.Value;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            product.IsDeleted = true;
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

    }
}
