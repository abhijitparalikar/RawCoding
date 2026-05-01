using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthBasics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDataProtection()
                .SetApplicationName("AuthBasics");

            var app = builder.Build();

            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Path == "/user")
                {
                    var idp = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
                    var protector = idp.CreateProtector("auth-cookie");
                    var authCookie = ctx.Request.Cookies.Keys.FirstOrDefault(x => x == "auth");

                    if (authCookie is null)
                    {
                        ctx.Response.Redirect("/login");
                        return;
                    }

                    List<Claim> claims = [new Claim("user", "abhi")];
                    ClaimsIdentity identity = new(claims);
                    ClaimsPrincipal principal = new(identity);
                    ctx.User = principal;
                }
                
                await next();
            });

            app.MapGet("/", () => "Hello World!");

            app.MapGet("/user", (HttpContext ctx, IDataProtectionProvider idp) =>
            {
                return ctx.User.FindFirst("user")?.Value ?? "Authentication Failure; No user found";
            });

            app.MapGet("/login", (HttpContext ctx, IDataProtectionProvider idp) =>
            {
                var protector = idp.CreateProtector("auth-cookie");
                ctx.Response.Cookies.Append("auth",$"{protector.Protect("usr:abhi")}");
                return "Login Successful";
            });

            app.Run();
        }
    }
}
