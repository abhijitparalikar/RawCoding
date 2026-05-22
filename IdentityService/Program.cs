using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<IdentityDbContext>(o => o.UseInMemoryDatabase("iDb"));
            builder.Services.AddIdentity<IdentityUser,  IdentityRole>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
                o.Password.RequiredLength = 4;
            })
             .AddEntityFrameworkStores<IdentityDbContext>()
             .AddDefaultTokenProviders();
            
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();


            using (var scope = app.Services.CreateScope())
            {
                var usrMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (!await roleMgr.RoleExistsAsync("admin"))
                {
                    await roleMgr.CreateAsync(new IdentityRole("admin"));
                }

                var usr = await usrMgr.FindByEmailAsync("test@test.com");
                if (usr is null)
                {
                    usr = new IdentityUser() { UserName = "test@test.com", Email = "test@test.com" };
                    await usrMgr.CreateAsync(usr, password: "password");
                }

                if (!await usrMgr.IsInRoleAsync(usr, "admin"))
                {
                    await usrMgr.AddToRoleAsync(usr, "admin");
                }
            }

            app.MapGet("/", () => "Hello World!");

            app.MapGet("/login", async Task (SignInManager<IdentityUser> signInManager) =>
            {
                await signInManager.PasswordSignInAsync("test@test.com", "password", false, false);
                //return "login success";
            });

            app.MapGet("/secure", () => "Secure Info")
                .RequireAuthorization(policy => policy.RequireRole("admin"));


            app.Run();
        }
    }
}
