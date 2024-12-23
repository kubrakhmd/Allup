using Allup.DAL;
using Allup.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Allup.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Allup.Controllers
{
    public class BasketController : Controller
    {
            private readonly AppDBContext _context;
            private readonly UserManager<AppUser> _usermanager;


            public BasketController(AppDBContext context, UserManager<AppUser> usermanager)
            {
                _context = context;
                _usermanager = usermanager;
            }
            public async Task<IActionResult> Index()
            {
                List<BasketItemVM> basketvm = new();

                if (User.Identity.IsAuthenticated)
                {
                    basketvm = await _context.BasketItems.Where(b => b.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                        .Select(b => new BasketItemVM
                        {
                            ProductId = b.ProductId,
                            Name = b.Product.Name,
                            Price = b.Product.Price,
                            Count = b.Count,
                            Image = b.Product.ProductImages.FirstOrDefault(i => i.IsPrimary == true).Image,
                            SubTotal = b.Product.Price * b.Count,

                        }).ToListAsync();
                }

                else
                {
                    List<BasketCookieItemVM> cookievm;

                    string cookie = Request.Cookies["basket"];

                    if (cookie is null)
                    {
                        return View(basketvm);
                    }
                    cookievm = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);

                    foreach (var item in cookievm)
                    {
                        Product product = await _context.Products.Include(p => p.ProductImages.Where(i => i.IsPrimary == true))
                            .FirstOrDefaultAsync(p => p.Id == item.Id);

                        if (product is not null)
                        {
                            basketvm.Add(new BasketItemVM
                            {
                                ProductId = product.Id,
                                Name = product.Name,
                                Price = product.Price,
                                Count = item.Count,
                                Image = product.ProductImages[0].Image,
                                SubTotal = product.Price * item.Count,

                            });

                        }
                    }

                }
                return View(basketvm);

            }

            public async Task<IActionResult> Addbasket(int? id)
            {
                if (id is null || id < 1) return BadRequest();

                bool result = await _context.Products.AnyAsync(p => p.Id == id);
                if (!result) return NotFound();

                if (User.Identity.IsAuthenticated)
                {
                    var user = await _usermanager.Users.Include(u => u.BasketItems).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                    var item = user.BasketItems.FirstOrDefault(b => b.ProductId == id);

                    if (item is not null)
                    {
                        item.Count++;
                    }
                    else
                    {
                        user.BasketItems.Add(new BasketItems { ProductId = id.Value, Count = 1 });
                    }
                    await _context.SaveChangesAsync();

                }
                else
                {
                    List<BasketCookieItemVM> basket;
                    string cookie = Request.Cookies["basket"];

                    if (cookie != null)
                    {
                        basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);
                        var product = basket.FirstOrDefault(b => b.Id == id);

                        if (product is not null)
                        {
                            product.Count++;
                        }
                        else
                        {
                            basket.Add(new BasketCookieItemVM { Id = id.Value, Count = 1 });
                        }
                    }
                    else
                    {
                        basket = new();
                        basket.Add(new BasketCookieItemVM { Id = id.Value, Count = 1 });
                    }

                    string json = JsonConvert.SerializeObject(basket);
                    Response.Cookies.Append("basket", json);
                }


                return RedirectToAction("Index", "Home");
            }

            public async Task<IActionResult> Delete(int? id)
            {
                if (id is null || id < 1) return BadRequest();
                bool result = await _context.Products.AnyAsync(p => p.Id == id);
                if (!result) return NotFound();

                if (User.Identity.IsAuthenticated)
                {
                    var user = await _usermanager.Users.Include(u => u.BasketItems)
                       .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                    var item = user.BasketItems.FirstOrDefault(b => b.ProductId == id);

                    if (item != null)
                    {
                        user.BasketItems.Remove(item);
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    List<BasketCookieItemVM> cookievm;

                    string cookie = Request.Cookies["basket"];

                    if (cookie != null)
                    {
                        cookievm = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);
                        var item = cookievm.FirstOrDefault(c => c.Id == id);

                        if (item is not null)
                        {
                            cookievm.Remove(item);
                        }


                        string json = JsonConvert.SerializeObject(cookievm);
                        Response.Cookies.Append("basket", json);
                    }

                }
                return RedirectToAction("Index", "Basket");
            }

       

            [Authorize(Roles = "User")]
            public async Task<IActionResult> Checkout()
            {
                OrderVM ordervm = new OrderVM
                {
                    BasketinOrders = await _context.BasketItems.Include(p => p.Product)
                    .Select(b => new BasketInOrdersVM
                    {
                        Name = b.Product.Name,
                        Price = b.Product.Price,
                        Count = b.Count,
                        Subtotal = b.Count * b.Product.Price,
                    }).ToListAsync()
                };
                return View(ordervm);
            }

            [HttpPost]
            public async Task<IActionResult> Checkout(OrderVM ordervm)
            {
                var basketitems = await _context.BasketItems.Include(b => b.Product)
                    .Where(bi => bi.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).ToListAsync();

                if (!ModelState.IsValid)
                {
                    ordervm.BasketinOrders = basketitems.Select(b => new BasketInOrdersVM
                    {
                        Name = b.Product.Name,
                        Price = b.Product.Price,
                        Count = b.Count,
                        Subtotal = b.Count * b.Product.Price

                    }).ToList();
                    return View(ordervm);
                }

                Order order = new Order
                {
                    Address = ordervm.Address,
                    Number = ordervm.Number,
                  CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    Status = null,
                    AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),

                    OrderItems = basketitems.Select(b => new OrderItem
                    {
                        Count = b.Count,
                        Price = b.Product.Price,
                        ProductId = b.Product.Id,
                    }).ToList(),

                    Subtotal = basketitems.Sum(b => b.Product.Price * b.Count),

                };
                _context.Orders.Add(order);
                _context.BasketItems.RemoveRange(basketitems);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
        }
 
}
