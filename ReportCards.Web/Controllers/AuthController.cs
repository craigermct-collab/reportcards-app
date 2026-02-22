using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportCards.Web.Data;
using System.Security.Claims;

namespace ReportCards.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly SchoolDbContext _db;

    public AuthController(SchoolDbContext db)
    {
        _db = db;
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var props = new AuthenticationProperties { RedirectUri = "/auth/google-callback" };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email == null) return Redirect("/access-denied");

        var appUser = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (appUser == null) return Redirect("/access-denied");

        // Re-sign in with role claim added
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, appUser.Role),
            new(ClaimTypes.Name, User.FindFirst(ClaimTypes.Name)?.Value ?? email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Redirect("/");
    }

    [HttpGet("/login")]
    public IActionResult Login()
    {
        return Redirect("/auth/google-login");
    }

    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }
}