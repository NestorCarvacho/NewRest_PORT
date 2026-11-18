using Microsoft.AspNetCore.Authorization;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Pages.Modulos;

namespace SGH.Web.Pages.Solicitud;

[Authorize(Policy = PoliticasAplicacion.GestionarReservas)]
public sealed class DatosModel(
    IEjecutorPostModulo ejecutorPost,
    MidSolicitudes mid) : ModuloDatosPageModel(ejecutorPost)
{
    protected override IReadOnlyList<AccionApi> OpcionesMid() => MidSolicitudes.Opciones;
    protected override string NombreModulo() => "Solicitud";
    protected override Task<EjecucionMid> EjecutarMidAsync(
        FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken) =>
        mid.EjecutarAsync(filtro, sesion, cancellationToken);
}