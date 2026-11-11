using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Cuenta;
using SGH.Web.Mid;

namespace SGH.Web.Controllers;

[Authorize(Policy = PoliticasAplicacion.GestionarCensos)]
public sealed class CensoInformesController(MidInformesCenso mid, IWebHostEnvironment ambiente, ILogger<CensoInformesController> logger) : Controller
{
    public const string MidRuta = "/Censo/Mid/MidInformesCenso.cs";

    [HttpPost(MidRuta)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ejecutar([FromBody] FiltroPeticion? filtro, CancellationToken cancellationToken) =>
        await ResponderAsync(filtro, cancellationToken);

    private async Task<IActionResult> ResponderAsync(FiltroPeticion? filtro, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filtro?.Opcion))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, "El Mid requiere la propiedad opcion."));
        if (MidInformesCenso.Opciones.All(item => item.Opcion != filtro.Opcion))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está en Mid/MidInformesCenso.cs."));

        try
        {
            var sesion = LectorSesion.Leer(User);
            return RespuestaDeMid.De(await mid.EjecutarAsync(filtro, new DatosSesion(sesion.UsuarioId, sesion.CampamentoId, sesion.EmpresaMandanteId), cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Fallo la opción {Opcion} de informes de censo.", filtro.Opcion);
            return RespuestaDeMid.DeExcepcion(ex, ambiente.IsDevelopment());
        }
    }
}
