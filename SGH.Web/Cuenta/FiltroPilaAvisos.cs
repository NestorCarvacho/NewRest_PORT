using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SGH.Web.Cuenta;

public sealed class FiltroPilaAvisos : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var ejecutado = await next();
        if (!EsIngreso(context.HttpContext.Request.Path) || ejecutado.HandlerInstance is not PageModel pagina)
            return;

        var nuevos = pagina.ModelState.Values
            .Where(entrada => entrada is not null)
            .SelectMany(entrada => entrada!.Errors)
            .Select(error => error.ErrorMessage)
            .Where(mensaje => !string.IsNullOrWhiteSpace(mensaje))
            .Distinct()
            .ToList();

        if (context.HttpContext.Items.TryGetValue(RegistroVentanaIngreso.ClaveRestantes, out var crudo)
            && crudo is int restantes
            && nuevos.Count > 0
            && nuevos[^1].Contains("intento", StringComparison.OrdinalIgnoreCase) == false)
        {
            nuevos[^1] = $"{nuevos[^1]} {RegistroVentanaIngreso.Texto(restantes)}";
        }

        if (pagina.ViewData["AvisoExito"] is string exito && !string.IsNullOrWhiteSpace(exito))
            nuevos.Add(exito);

        if (nuevos.Count == 0)
            return;

        PilaAvisos.Guardar(context.HttpContext, PilaAvisos.Leer(context.HttpContext.Request), nuevos);
    }

    private static bool EsIngreso(PathString ruta) =>
        ruta.StartsWithSegments("/Cuenta/Login") || ruta.StartsWithSegments("/Cuenta/CambiarClave");
}
