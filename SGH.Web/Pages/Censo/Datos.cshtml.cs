using Microsoft.AspNetCore.Authorization;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Pages.Modulos;

namespace SGH.Web.Pages.Censo;

[Authorize(Policy = PoliticasAplicacion.GestionarCensos)]
public sealed class DatosModel(
    IEjecutorPostModulo ejecutorPost,
    MidCenso mid) : ModuloDatosPageModel(ejecutorPost)
{
    protected override IReadOnlyList<AccionApi> OpcionesMid() => MidCenso.Opciones;
    protected override string NombreModulo() => "Censo";
    protected override Task<EjecucionMid> EjecutarMidAsync(
        FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken) =>
        mid.EjecutarAsync(filtro, sesion, cancellationToken);
}