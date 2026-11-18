using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SGH.Web.Pages.Cuenta;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class SalirModel : PageModel
{
    public Task<IActionResult> OnGetAsync() => CerrarSesionYRedirigirAsync();

    public Task<IActionResult> OnPostAsync() => CerrarSesionYRedirigirAsync();

    private async Task<IActionResult> CerrarSesionYRedirigirAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToPage("/Cuenta/Login");
    }
}
