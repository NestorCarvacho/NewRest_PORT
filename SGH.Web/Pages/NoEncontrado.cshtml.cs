using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SGH.Web.Pages;

[AllowAnonymous]
public class NoEncontradoModel : PageModel
{
    public IActionResult OnGet()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return Page();
    }
}
