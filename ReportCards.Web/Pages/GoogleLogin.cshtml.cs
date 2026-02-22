using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ReportCards.Web.Pages;

public class GoogleLoginModel : PageModel
{
    public IActionResult OnGet()
    {
        var props = new AuthenticationProperties { RedirectUri = "/auth/google-callback" };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }
}
