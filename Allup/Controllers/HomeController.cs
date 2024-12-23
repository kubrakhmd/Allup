using Allup.DAL;
using Allup.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Allup.Controllers
{
	public class HomeController : Controller
	{
		private readonly AppDBContext _context;

		public HomeController(AppDBContext context)
		{
			_context = context;

		}
		
        public IActionResult Index()
		{
            HomeVM homeVM = new HomeVM
            {
                Slides = _context.Slides.OrderBy(s => s.Order).ToList(),
            };

            return View(homeVM);
		}
    } }