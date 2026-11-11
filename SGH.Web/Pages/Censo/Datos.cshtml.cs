using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Web.Autorizacion;

namespace SGH.Web.Pages.Censo;

[Authorize(Policy = PoliticasAplicacion.GestionarCensos)]
public sealed class DatosModel : PageModel
{
    public void OnGet()
    {
    }
}
