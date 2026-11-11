using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Web.Autorizacion;

namespace SGH.Web.Pages.Reserva;

[Authorize(Policy = PoliticasAplicacion.GestionarReservas)]
public sealed class DatosModel : PageModel
{
    public void OnGet()
    {
    }
}
