using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SGH.Web.Pages;

[AllowAnonymous]
public class CookiesModel : PageModel
{
    public void OnGet()
    {
    }
}
