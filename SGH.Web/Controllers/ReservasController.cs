using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Cuenta;
using SGH.Web.Mid;

namespace SGH.Web.Controllers;

[Authorize(Policy = PoliticasAplicacion.GestionarReservas)]
public sealed class ReservasController(MidReservas mid, EjecutorPeticionMid ejecutor) : Controller
{
    public const string MidRuta = "/Reserva/Mid/MidReservas.cs";
    private const string Modulo = "Reservas";

    [HttpGet("/Reserva/PlantillaReservas")]
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
            return await ImportarAsync(cancellationToken);

        FiltroPeticion? filtro;
        try
        {
            filtro = await Request.ReadFromJsonAsync<FiltroPeticion>(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, "El Mid no pudo leer la petición."));
        }

        return await ResponderAsync(filtro, cancellationToken);
    }

    private Task<IActionResult> ImportarAsync(CancellationToken cancellationToken)
    {
        var archivo = Request.Form.Files["archivo"];
        if (archivo is null || archivo.Length == 0)
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, "Seleccione una planilla Excel para importar.")));
        if (!string.Equals(Path.GetExtension(archivo.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, "El archivo debe tener formato .xlsx.")));

        var sesion = LectorSesion.Leer(User);
        return ejecutor.EjecutarAsync(
            async () =>
            {
                await using var contenido = archivo.OpenReadStream();
                return RespuestaDeMid.De(await mid.ImportarAsync(contenido, SesionDesde(sesion), cancellationToken));
            },
            Modulo,
            "importar",
            sesion.UsuarioId,
            cancellationToken);
    }

    private Task<IActionResult> ResponderAsync(FiltroPeticion? filtro, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filtro?.Opcion))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, "El Mid requiere la propiedad opcion.")));
        if (MidReservas.Opciones.All(item => item.Opcion != filtro.Opcion))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está en Mid/MidReservas.cs.")));

        var sesion = LectorSesion.Leer(User);
        return ejecutor.EjecutarAsync(
            async () => RespuestaDeMid.De(await mid.EjecutarAsync(filtro, SesionDesde(sesion), cancellationToken)),
            Modulo,
            filtro.Opcion,
            sesion.UsuarioId,
            cancellationToken);
    }

    private static DatosSesion SesionDesde(ContextoSesion sesion) =>
        new(sesion.UsuarioId, sesion.CampamentoId, sesion.EmpresaMandanteId);
}