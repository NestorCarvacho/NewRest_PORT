using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Cuenta;
using SGH.Web.Mid;

namespace SGH.Web.Controllers;

[Authorize(Policy = PoliticasAplicacion.GestionarCensos)]
public sealed class CensoController(MidCenso mid, EjecutorPeticionMid ejecutor) : Controller
{
    public const string MidRuta = "/Censo/Mid/MidCenso.cs";
    private const string Modulo = "Censo";

    [HttpPost(MidRuta)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Ejecutar([FromBody] FiltroPeticion? filtro, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filtro?.Opcion))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, "El Mid requiere la propiedad opcion.")));
        if (MidCenso.Opciones.All(item => item.Opcion != filtro.Opcion))
            return Task.FromResult(RespuestaDeMid.De(EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está en Mid/MidCenso.cs.")));

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