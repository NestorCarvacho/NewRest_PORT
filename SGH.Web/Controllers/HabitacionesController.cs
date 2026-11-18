using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Cuenta;
using SGH.Web.Mid;

namespace SGH.Web.Controllers;

[Authorize(Policy = PoliticasAplicacion.GestionarHabitaciones)]
public sealed class HabitacionesController(MidHabitaciones mid, EjecutorPeticionMid ejecutor) : Controller
{
    public const string MidRuta = "/Habitacion/Mid/MidHabitaciones.cs";
    private const string Modulo = "Habitaciones";

    [HttpGet("/Habitacion/PlantillaCamas")]
    public Task<IActionResult> PlantillaCamas(CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        return ejecutor.EjecutarAsync(
            async () => RespuestaDeMid.De(await mid.PlantillaCamasAsync(SesionDesde(sesion), cancellationToken)),
            Modulo,
            "plantillacamas",
            sesion.UsuarioId,
            cancellationToken);
    }

    [HttpPost(MidRuta)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ejecutar(CancellationToken cancellationToken)
    {
        if (Request.HasFormContentType)
            return await CargarCamasAsync(cancellationToken);

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
        if (MidHabitaciones.Opciones.All(item => item.Opcion != filtro.Opcion))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está en {MidHabitaciones.Ruta}."));

        var sesion = LectorSesion.Leer(User);
        return await ejecutor.EjecutarAsync(
            async () => RespuestaDeMid.De(await mid.EjecutarAsync(
                filtro,
                SesionDesde(sesion),
                cancellationToken)),
            Modulo,
            filtro.Opcion,
            sesion.UsuarioId,
            cancellationToken);
    }

    private Task<IActionResult> CargarCamasAsync(CancellationToken cancellationToken)
    {
        var archivo = Request.Form.Files["archivo"];
        if (archivo is null || archivo.Length == 0
            || !string.Equals(Path.GetExtension(archivo.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, "Seleccione una planilla .xlsx válida.")));

        var sesion = LectorSesion.Leer(User);
        return ejecutor.EjecutarAsync(
            async () =>
            {
                await using var contenido = archivo.OpenReadStream();
                return RespuestaDeMid.De(await mid.CargarCamasAsync(contenido, SesionDesde(sesion), cancellationToken));
            },
            Modulo,
            "cargarcamas",
            sesion.UsuarioId,
            cancellationToken);
    }

    private static DatosSesion SesionDesde(ContextoSesion sesion) =>
        new(sesion.UsuarioId, sesion.CampamentoId, sesion.EmpresaMandanteId);
}