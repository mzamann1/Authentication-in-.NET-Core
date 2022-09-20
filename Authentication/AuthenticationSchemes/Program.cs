using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddScheme<CookieAuthenticationOptions, VisitorAuthHandler>("visitor", o =>
    {
    })
    .AddCookie("visitor")
    .AddCookie("patreon-cookie")
    .AddOAuth("external-auth", configureOptions =>
    {
        configureOptions.SignInScheme = "patreon-cookie"
        configureOptions.AuthorizationEndpoint = "https://oauth.mocklab.io/oauth/authorize";
        configureOptions.TokenEndpoint = " https://oauth.mocklab.io/oauth/token";
        configureOptions.UserInformationEndpoint = "https://oauth.mocklab.io/userinfo";
        configureOptions.Scope.Add("profile");
        configureOptions.SaveTokens = true;
        configureOptions.CallbackPath = "/cb-patreon";
        // configureOptions. https://oauth.mocklab.io/.well-known/jwks.json;
    });

builder.Services.AddAuthorization(b =>
{
    b.AddPolicy("customer", p =>
    {
        p.AddAuthenticationSchemes("local", "visitor").RequireAuthenticatedUser();
    });
    b.AddPolicy("user", p =>
    {
        p.AddAuthenticationSchemes("local").RequireAuthenticatedUser();
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Task.FromResult("Hello world")).RequireAuthorization("customer");

app.MapGet("/login-local", async (ctx) =>
{
    var claims = new List<Claim> { new Claim("usr", "zaman") };
    var identity = new ClaimsIdentity(claims, "cookie");
    var user = new ClaimsPrincipal(identity);

    await ctx.SignInAsync("local", user);
});

app.MapGet("/login-patreon", async (ctx) => await ctx.ChallengeAsync(new AuthenticationProperties()
{
    RedirectUri = "/"
})).RequireAuthorization("user");

app.Run();

public class VisitorAuthHandler : CookieAuthenticationHandler
{
    public VisitorAuthHandler(IOptionsMonitor<CookieAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {

        var result = await base.HandleAuthenticateAsync();
        if (result.Succeeded)
        {
            return result;
        }

        var claims = new List<Claim> { new Claim("usr", "visitor") };
        var identity = new ClaimsIdentity(claims, "visitor");
        var user = new ClaimsPrincipal(identity);


        await Context.SignInAsync("visitor", user);
        return AuthenticateResult.Success(new AuthenticationTicket(user, "visitor"));
    }
}
