using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SGH.Web.Pages;

[AllowAnonymous]
public class AccesoDenegadoModel : PageModel
{
    public string Mensaje { get; private set; } = "Tu perfil no puede abrir esta pantalla.";

    public void OnGet()
    {
        if (TempData.TryGetValue("AccesoDenegado.Mensaje", out var aviso) && aviso is string texto
            && !string.IsNullOrWhiteSpace(texto))
        {
            Mensaje = texto;
        }
    }
}
