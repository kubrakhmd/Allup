using Allup.Areas.Admin.ViewModels;
using Allup.Areas.ViewModels;
using Allup.DAL;
using Allup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TagController : Controller
    {
       
      
            private readonly AppDBContext _context;

            public TagController(AppDBContext context)
            {
                _context = context;

            }
            public async Task<IActionResult> Index()
            {
                List<ListTagVM> categorieVMs = await _context.Tags.Where(t => !t.IsDeleted)
                   
                    .Select(t => new ListTagVM
                    {
                        Id = t.Id,
                        Name = t.Name,
                     
                    }).ToListAsync();
                return View(categorieVMs);
            }
            public IActionResult Create()
            {


                return View();

            }
            [HttpPost]
            public async Task<IActionResult> CreateAsync(CreateTagVM tagVM)
            {
                if (!ModelState.IsValid)
                {

                    return View();
                }

                bool result = await _context.Tags.AnyAsync(t => t.Name.Trim() == tagVM.Name.Trim());

                if (result)
                {
                    ModelState.AddModelError("Name", "Name Already exists");
                    return View();

                }
             Tag tag = new()
                {
                    Name = tagVM.Name
                };
                tag.CreatedAt = DateTime.Now;
                await _context.Tags.AddAsync(tag);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            public async Task<IActionResult> Update(int? id)
            {
                if (id == null || id < 1) return BadRequest();

            Tag tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);

                if (tag is null) return NotFound();

                return View(tag);

            }

            [HttpPost]
            public async Task<IActionResult> Update(int? id, Tag tag)
            {
                if (id == null || id < 1) return BadRequest();

                Category existed = await _context.Categories.FirstOrDefaultAsync(t => t.Id == id);

                if (tag is null) return NotFound();

                if (!ModelState.IsValid)
                {
                    return View();
                }

                bool result = await _context.Tags.AnyAsync(t => t.Name.Trim() == tag.Name.Trim() && t.Id != id);
                if (result)
                {
                    ModelState.AddModelError(nameof(Tag.Name), "Tag already exists");
                    return View();
                }



                existed.Name = tag.Name;


                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));

            }


            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null || id < 1) return BadRequest();

              Tag tag= await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);

                if (tag is null) return NotFound();


             tag.IsDeleted = true;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }
        }
    }

