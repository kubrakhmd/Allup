using Allup.DAL;
using Allup.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Allup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDBContext>(opt =>

            opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"))

            );

            builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 8;
                opt.User.RequireUniqueEmail = true;

                opt.Lockout.AllowedForNewUsers = true;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
                opt.Lockout.MaxFailedAccessAttempts = 3;

            }
            ).AddEntityFrameworkStores<AppDBContext>().AddDefaultTokenProviders();


            var app = builder.Build();

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                             "admin",
                             "{area:exists}/{controller=home}/{action=index}/{Id?}"

                            );
            app.MapControllerRoute(
                 "default",
                 "{controller=home}/{action=index}/{Id?}"

                );

            app.Run();
        }
    }
}

