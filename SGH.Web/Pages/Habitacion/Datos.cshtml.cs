using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Web.Autorizacion;

namespace SGH.Web.Pages.Habitacion;

[Authorize(Policy = PoliticasAplicacion.GestionarHabitaciones)]
public sealed class DatosModel : PageModel
{
    public void OnGet()
    {
    }
}