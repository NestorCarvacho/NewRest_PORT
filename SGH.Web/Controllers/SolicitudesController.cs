using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Cuenta;
using SGH.Web.Mid;

namespace SGH.Web.Controllers;

[Authorize(Policy = PoliticasAplicacion.GestionarReservas)]
public sealed class SolicitudesController(MidSolicitudes mid, EjecutorPeticionMid ejecutor) : Controller
{
    public const string MidRuta = "/Solicitud/Mid/MidSolicitudes.cs";
    private const string Modulo = "Solicitudes";

    [HttpGet("/Solicitud/PlantillaPasajeros")]
    public Task<IActionResult> Plantilla(CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        return ejecutor.EjecutarAsync(
            async () => RespuestaDeMid.De(await mid.EjecutarAsync(
                new FiltroPeticion { Opcion = "plantilla" },
                SesionDesde(sesion),
                cancellationToken)),
            Modulo,
            "plantilla",
            sesion.UsuarioId,
            cancellationToken);
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

        var sesion = LectorSesion.Leer(User);
        return await ejecutor.EjecutarAsync(
            async () => RespuestaDeMid.De(await mid.EjecutarAsync(filtro, SesionDesde(sesion), cancellationToken)),
            Modulo,
            filtro.Opcion,
            sesion.UsuarioId,
            cancellationToken);
    }

    private Task<IActionResult> CargarAsync(CancellationToken cancellationToken)
    {
        var archivo = Request.Form.Files["archivo"];
        if (!int.TryParse(Request.Form["id"], out var id) || archivo is null || archivo.Length == 0
            || !string.Equals(Path.GetExtension(archivo.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, "Seleccione una solicitud y una planilla .xlsx válida.")));

        var sesion = LectorSesion.Leer(User);
        return ejecutor.EjecutarAsync(
            async () =>
            {
                await using var contenido = archivo.OpenReadStream();
                return RespuestaDeMid.De(await mid.CargarAsync(id, contenido, SesionDesde(sesion), cancellationToken));
            },
            Modulo,
            "cargar_pasajeros",
            sesion.UsuarioId,
            cancellationToken);
    }

    private static DatosSesion SesionDesde(ContextoSesion sesion) =>
        new(sesion.UsuarioId, sesion.CampamentoId, sesion.EmpresaMandanteId);
}