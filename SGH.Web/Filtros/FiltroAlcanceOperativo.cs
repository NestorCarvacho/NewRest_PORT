using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Application.Sesion;
using SGH.Domain.Cuenta;
using SGH.Web.Cuenta;

namespace SGH.Web.Filtros;

public sealed class FiltroAlcanceOperativo : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (context.HandlerInstance is not PageModel || !EsModuloOperativo(context.HttpContext.Request.Path))
        {
            await next();
            return;
        }

        var usuario = context.HttpContext.User;
        if (usuario.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        if (usuario.IsInRole(ReglasCuenta.RolIntegracion))
        {
            await next();
            return;
        }

        var sesion = LectorSesion.Leer(usuario);
        var bloqueo = AlcanceSesion.ValidarAlcance(sesion.EmpresaMandanteId, sesion.CampamentoId);
        if (bloqueo is not null)
        {
            var motivo = bloqueo == ReglasAlcance.SinEmpresaMandante ? "mandante" : "campamento";
            context.Result = new RedirectToPageResult("/SinAlcance", new { motivo });
            return;
        }

        await next();
    }

    private static bool EsModuloOperativo(PathString ruta) =>
        ruta.StartsWithSegments("/Reserva")
        || ruta.StartsWithSegments("/Solicitud")
        || ruta.StartsWithSegments("/Censo");
}
