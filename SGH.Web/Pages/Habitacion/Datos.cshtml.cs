using Microsoft.AspNetCore.Authorization;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Pages.Modulos;

namespace SGH.Web.Pages.Habitacion;

[Authorize(Policy = PoliticasAplicacion.GestionarHabitaciones)]
public sealed class DatosModel(
    IEjecutorPostModulo ejecutorPost,
    MidHabitaciones mid) : ModuloDatosPageModel(ejecutorPost)
{
    protected override IReadOnlyList<AccionApi> OpcionesMid() => MidHabitaciones.Opciones;
    protected override string NombreModulo() => "Habitaciones";
    protected override Task<EjecucionMid> EjecutarMidAsync(
        FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken) =>
        mid.EjecutarAsync(filtro, sesion, cancellationToken);
}