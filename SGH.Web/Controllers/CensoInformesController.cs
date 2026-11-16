using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Cuenta;
using SGH.Web.Mid;

namespace SGH.Web.Controllers;

[Authorize(Policy = PoliticasAplicacion.GestionarCensos)]
public sealed class CensoInformesController(MidInformesCenso mid, EjecutorPeticionMid ejecutor) : Controller
{
    public const string MidRuta = "/Censo/Mid/MidInformesCenso.cs";
    private const string Modulo = "CensoInformes";

    [HttpPost(MidRuta)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Ejecutar([FromBody] FiltroPeticion? filtro, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filtro?.Opcion))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, "El Mid requiere la propiedad opcion.")));
        if (MidInformesCenso.Opciones.All(item => item.Opcion != filtro.Opcion))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está en Mid/MidInformesCenso.cs.")));

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