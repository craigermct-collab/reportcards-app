using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReportCards.Web.Data;
using System.Security.Claims;

namespace ReportCards.Web.Pages;

public class GoogleCallbackModel : PageModel
{
    private readonly SchoolDbContext _db;

    public GoogleCallbackModel(SchoolDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // At this point the Google middleware has processed the callback
        // and signed the user in with a cookie - we can read User.Claims
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

        return Redirect("/home");
    }
}
