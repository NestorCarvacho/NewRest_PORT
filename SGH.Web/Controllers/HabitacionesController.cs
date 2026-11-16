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

    [HttpPost(MidRuta)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Ejecutar([FromBody] FiltroPeticion? filtro, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filtro?.Opcion))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, "El Mid requiere la propiedad opcion.")));
        if (MidHabitaciones.Opciones.All(item => item.Opcion != filtro.Opcion))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está en {MidHabitaciones.Ruta}.")));

        var sesion = LectorSesion.Leer(User);
        return ejecutor.EjecutarAsync(
            async () => RespuestaDeMid.De(await mid.EjecutarAsync(
                filtro,
                new DatosSesion(sesion.UsuarioId, sesion.CampamentoId, sesion.EmpresaMandanteId),
                cancellationToken)),
            Modulo,
            filtro.Opcion,
            sesion.UsuarioId,
            cancellationToken);
    }
}