using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Application.Mid;
using SGH.Application.Sesion;

namespace SGH.Web.Pages.Modulos;

public interface IEjecutorPostModulo
{
    /// <summary>
    /// Ejecuta una opción Mid a partir de un POST de formulario Razor y redirige a la misma página del módulo.
    /// Las reglas de negocio siguen en Mid / Domain; aquí solo se traduce el formulario.
    /// </summary>
    Task<IActionResult> EjecutarAsync(
        PageModel page,
        FiltroPeticion filtro,
        IReadOnlyList<AccionApi> opcionesPermitidas,
        Func<FiltroPeticion, DatosSesion, CancellationToken, Task<EjecucionMid>> ejecutarMid,
        string modulo,
        CancellationToken cancellationToken);
}