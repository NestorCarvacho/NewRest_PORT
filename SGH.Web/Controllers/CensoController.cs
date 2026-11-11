using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Web.Autorizacion;
using SGH.Web.Cuenta;
using SGH.Web.Mid;

namespace SGH.Web.Controllers;

[Authorize(Policy = PoliticasAplicacion.GestionarCensos)]
public sealed class CensoController(MidCenso mid, IWebHostEnvironment ambiente, ILogger<CensoController> logger) : Controller
{
    public const string MidRuta = "/Censo/Mid/MidCenso.cs";

    [HttpPost(MidRuta)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ejecutar([FromBody] FiltroPeticion? filtro, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filtro?.Opcion))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, "El Mid requiere la propiedad opcion."));
        if (MidCenso.Opciones.All(item => item.Opcion != filtro.Opcion))
            return RespuestaDeMid.De(EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está en Mid/MidCenso.cs."));

        try
        {
            var sesion = LectorSesion.Leer(User);
            return RespuestaDeMid.De(await mid.EjecutarAsync(filtro, new DatosSesion(sesion.UsuarioId, sesion.CampamentoId, sesion.EmpresaMandanteId), cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Fallo la opción {Opcion} de censo.", filtro.Opcion);
            return RespuestaDeMid.DeExcepcion(ex, ambiente.IsDevelopment());
        }
    }
}
