using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;

const string AuthScheme = "cookie";
const string AuthScheme2 = "cookie2";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(AuthScheme).AddCookie(AuthScheme, configureOptions =>
{
    configureOptions.LoginPath = "/unauthorized";
}).AddCookie(AuthScheme2);


builder.Services.AddAuthorization(authorizationOptions =>
{
    authorizationOptions.AddPolicy("eu passport", pb =>
    {
        pb.RequireAuthenticatedUser().AddAuthenticationSchemes(AuthScheme).RequireClaim("passport_type", "eur");
    });
});

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/un-secure", (HttpContext ctx) => ctx?.User?.FindFirst("usr")?.Value ?? "empty");

app.MapGet("/sweden", () => "allowed").RequireAuthorization("eu passport"); ;

app.MapGet("/unauthorized", () => "unauthorized");

app.MapGet("/login", async (HttpContext ctx) =>
{
    var claims = new List<Claim> { new Claim("usr", "zaman"), new Claim("passport_type", "eur") };
    var identity = new ClaimsIdentity(claims, AuthScheme);

    var user = new ClaimsPrincipal(identity);
    await ctx.SignInAsync(AuthScheme, user);
}).AllowAnonymous();

app.Run();
