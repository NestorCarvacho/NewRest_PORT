using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Cuenta;
using SGH.Web.Mid;

namespace SGH.Web.Controllers;

[Authorize(Policy = PoliticasAplicacion.GestionarReservas)]
public sealed class ReservasController(MidReservas mid, IWebHostEnvironment ambiente, ILogger<ReservasController> logger) : Controller
{
    public const string MidRuta = "/Reserva/Mid/MidReservas.cs";

    [HttpGet("/Reserva/PlantillaReservas")]
    public async Task<IActionResult> Plantilla(CancellationToken cancellationToken) =>
        RespuestaDeMid.De(await mid.EjecutarAsync(new FiltroPeticion { Opcion = "plantilla" }, Sesion(), cancellationToken));

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

    private async Task<IActionResult> ImportarAsync(CancellationToken cancellationToken)
    {
        var archivo = Request.Form.Files["archivo"];
        if (archivo is null || archivo.Length == 0)
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, "Seleccione una planilla Excel para importar."));
        if (!string.Equals(Path.GetExtension(archivo.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, "El archivo debe tener formato .xlsx."));

        try
        {
            await using var contenido = archivo.OpenReadStream();
            return RespuestaDeMid.De(await mid.ImportarAsync(contenido, Sesion(), cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la importación de reservas desde Excel.");
            return RespuestaDeMid.DeExcepcion(ex, ambiente.IsDevelopment());
        }
    }

    private async Task<IActionResult> ResponderAsync(FiltroPeticion? filtro, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filtro?.Opcion))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, "El Mid requiere la propiedad opcion."));
        if (MidReservas.Opciones.All(item => item.Opcion != filtro.Opcion))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está en Mid/MidReservas.cs."));

        try
        {
            return RespuestaDeMid.De(await mid.EjecutarAsync(filtro, Sesion(), cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Fallo la opción {Opcion} de reservas.", filtro.Opcion);
            return RespuestaDeMid.DeExcepcion(ex, ambiente.IsDevelopment());
        }
    }

    private DatosSesion Sesion()
    {
        var sesion = LectorSesion.Leer(User);
        return new DatosSesion(sesion.UsuarioId, sesion.CampamentoId, sesion.EmpresaMandanteId);
    }
}
