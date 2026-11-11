using SGH.Application.Sesion;

namespace SGH.Application.Mid;

public static class AlcanceMid
{
  public static EjecucionMid? BloquearSiSinAlcance(DatosSesion sesion)
  {
    var mensaje = AlcanceSesion.ValidarAlcance(sesion.EmpresaMandanteId, sesion.CampamentoId);
    return mensaje is null ? null : EjecucionMid.Fallo(400, mensaje);
  }
}
