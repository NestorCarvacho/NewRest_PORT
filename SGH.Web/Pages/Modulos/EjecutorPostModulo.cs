using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Cuenta;
using SGH.Web.Mid;

namespace SGH.Web.Pages.Modulos;

public sealed class EjecutorPostModulo(
    IWebHostEnvironment ambiente,
    ILogger<EjecutorPostModulo> logger) : IEjecutorPostModulo
{
    public const string ClaveAvisoExito = "ModuloAvisoExito";
    public const string ClaveAvisoError = "ModuloAvisoError";

    public async Task<IActionResult> EjecutarAsync(
        PageModel page,
        FiltroPeticion filtro,
        IReadOnlyList<AccionApi> opcionesPermitidas,
        Func<FiltroPeticion, DatosSesion, CancellationToken, Task<EjecucionMid>> ejecutarMid,
        string modulo,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filtro.Opcion))
        {
            page.TempData[ClaveAvisoError] = "El Mid requiere la propiedad opcion.";
            return Redirigir(page);
        }

        if (opcionesPermitidas.All(item => item.Opcion != filtro.Opcion))
        {
            page.TempData[ClaveAvisoError] = $"La opción '{filtro.Opcion}' no está disponible en este módulo.";
            return Redirigir(page);
        }

        var contexto = LectorSesion.Leer(page.User);
        var sesion = new DatosSesion(contexto.UsuarioId, contexto.CampamentoId, contexto.EmpresaMandanteId);

        try
        {
            var ejecucion = await ejecutarMid(filtro, sesion, cancellationToken);

            if (ejecucion.EsArchivo)
            {
                return new FileContentResult(ejecucion.Archivo!, ejecucion.TipoArchivo!)
                {
                    FileDownloadName = ejecucion.NombreArchivo
                };
            }

            if (ejecucion.Status != EjecucionMid.OkStatus)
            {
                page.TempData[ClaveAvisoError] = ejecucion.Mensaje ?? "No se pudo completar la operación.";
                return Redirigir(page);
            }

            page.TempData[ClaveAvisoExito] = ExtraerMensajeExito(ejecucion) ?? "Operación realizada.";
            return Redirigir(page);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Fallo POST módulo Razor. Modulo {Modulo}, Opcion {Opcion}, UsuarioId {UsuarioId}",
                modulo, filtro.Opcion, sesion.UsuarioId);

            var fallo = RespuestaDeMid.DeExcepcion(ex, ambiente.IsDevelopment());
            if (fallo is JsonResult json && json.Value is not null)
            {
                var mensaje = ExtraerMensajeJsonFallo(json.Value);
                page.TempData[ClaveAvisoError] = mensaje ?? "No se pudo completar la operación.";
            }
            else
                page.TempData[ClaveAvisoError] = "No se pudo completar la operación.";

            return Redirigir(page);
        }
    }

    private static IActionResult Redirigir(PageModel page) =>
        page.RedirectToPage();

    private static string? ExtraerMensajeExito(EjecucionMid ejecucion)
    {
        if (ejecucion.Registros is null)
            return null;

        var tipo = ejecucion.Registros.GetType();
        var propMensaje = tipo.GetProperty("mensaje", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (propMensaje?.GetValue(ejecucion.Registros) is string texto && !string.IsNullOrWhiteSpace(texto))
            return texto;

        return null;
    }

    private static string? ExtraerMensajeJsonFallo(object cuerpo)
    {
        var tipo = cuerpo.GetType();
        var propRegistros = tipo.GetProperty("registros");
        var registros = propRegistros?.GetValue(cuerpo);
        if (registros is null)
            return null;

        var propMensaje = registros.GetType().GetProperty("mensaje", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return propMensaje?.GetValue(registros) as string;
    }
}