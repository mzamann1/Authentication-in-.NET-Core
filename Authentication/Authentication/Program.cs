using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDataProtection();
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<AuthService>();

builder.Services.AddAuthentication("cookie").AddCookie("cookie");

var app = builder.Build();
//app.Use((ctx, next) =>
//{
//    var idp = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
//    var protector = idp.CreateProtector("auth-cookie");
//    var authCookie = ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("auth="));

//    if (authCookie != null)
//    {
//        var protectedPayload = authCookie?.Split("=")?.Last();
//        var payload = protector.Unprotect(protectedPayload);
//        var parts = payload.Split(":");
//        var key = parts[0];
//        var value = parts[1];

//        var claims = new List<Claim>();
//        claims.Add(new Claim(key, value));
//        var identity = new ClaimsIdentity(claims);
//        ctx.User = new ClaimsPrincipal(identity);

//    }

//    return next();
//});


app.UseAuthentication();
app.MapGet("/login", async (HttpContext ctx) =>
{
    var claims = new List<Claim>();
    claims.Add(new Claim("usr", "zaman"));
    var identity = new ClaimsIdentity(claims, "cookie");

    var user = new ClaimsPrincipal(identity);
    await ctx.SignInAsync("cookie", user);
});


app.MapGet("/username", (HttpContext ctx, IDataProtectionProvider idp) =>
{

    return ctx.User.FindFirst("usr").Value;
});


app.Run();


//public class AuthService
//{
//    private readonly IDataProtectionProvider _idp;
//    private readonly IHttpContextAccessor _accessor;

//    public AuthService(IDataProtectionProvider idp, IHttpContextAccessor accessor)
//    {
//        _idp = idp;
//        _accessor = accessor;
//    }

//    public void SignIn()
//    {
//        var protection = _idp.CreateProtector("auth-cookie");
//        _accessor.HttpContext.Response.Headers["set-cookie"] = $"auth={protection.Protect("usr:anton")}";


//    }
//}
