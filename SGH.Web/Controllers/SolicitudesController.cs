using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Cuenta;
using SGH.Web.Mid;

namespace SGH.Web.Controllers;

[Authorize(Policy = PoliticasAplicacion.GestionarReservas)]
public sealed class SolicitudesController(MidSolicitudes mid, IWebHostEnvironment ambiente, ILogger<SolicitudesController> logger) : Controller
{
    public const string MidRuta = "/Solicitud/Mid/MidSolicitudes.cs";

    [HttpGet("/Solicitud/PlantillaPasajeros")]
    public async Task<IActionResult> Plantilla(CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        return RespuestaDeMid.De(await mid.EjecutarAsync(
            new FiltroPeticion { Opcion = "plantilla" },
            new DatosSesion(sesion.UsuarioId, sesion.CampamentoId, sesion.EmpresaMandanteId),
            cancellationToken));
    }

    [HttpPost(MidRuta)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ejecutar(CancellationToken cancellationToken)
    {
        if (Request.HasFormContentType)
            return await CargarAsync(cancellationToken);

        FiltroPeticion? filtro;
        try
        {
            filtro = await Request.ReadFromJsonAsync<FiltroPeticion>(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, "El Mid no pudo leer la petición."));
        }

        if (string.IsNullOrWhiteSpace(filtro?.Opcion))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, "El Mid requiere la propiedad opcion."));
        if (MidSolicitudes.Opciones.All(item => item.Opcion != filtro.Opcion))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está en Mid/MidSolicitudes.cs."));

        try
        {
            var sesion = LectorSesion.Leer(User);
            return RespuestaDeMid.De(await mid.EjecutarAsync(filtro, new DatosSesion(sesion.UsuarioId, sesion.CampamentoId, sesion.EmpresaMandanteId), cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Fallo la opción {Opcion} de solicitudes.", filtro.Opcion);
            return RespuestaDeMid.DeExcepcion(ex, ambiente.IsDevelopment());
        }
    }

    private async Task<IActionResult> CargarAsync(CancellationToken cancellationToken)
    {
        var archivo = Request.Form.Files["archivo"];
        if (!int.TryParse(Request.Form["id"], out var id) || archivo is null || archivo.Length == 0
            || !string.Equals(Path.GetExtension(archivo.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, "Seleccione una solicitud y una planilla .xlsx válida."));

        try
        {
            var sesion = LectorSesion.Leer(User);
            await using var contenido = archivo.OpenReadStream();
            return RespuestaDeMid.De(await mid.CargarAsync(id, contenido, new DatosSesion(sesion.UsuarioId, sesion.CampamentoId, sesion.EmpresaMandanteId), cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la carga masiva de pasajeros de la solicitud {SolicitudId}.", id);
            return RespuestaDeMid.DeExcepcion(ex, ambiente.IsDevelopment());
        }
    }
}
