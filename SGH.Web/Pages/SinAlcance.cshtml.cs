using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Domain.Alcance;

namespace SGH.Web.Pages;

[Authorize]
public class SinAlcanceModel : PageModel
{
    public string Mensaje { get; private set; } = ReglasAlcance.SinCampamento;

    public void OnGet(string? motivo)
    {
        Mensaje = motivo switch
        {
            "mandante" => ReglasAlcance.SinEmpresaMandante,
            _ => ReglasAlcance.SinCampamento
        };
    }
}
