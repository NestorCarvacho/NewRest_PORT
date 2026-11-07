using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SGH.Web.Pages;

[AllowAnonymous]
public class AccesoDenegadoModel : PageModel
{
    public void OnGet()
    {
    }
}
