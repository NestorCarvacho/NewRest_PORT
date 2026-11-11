using SGH.Domain.Alcance;

namespace SGH.Application.Sesion;

public static class AlcanceSesion
{
    public const string SinCampamento = ReglasAlcance.SinCampamento;

    public static bool TieneCampamento(int campamentoId) => campamentoId > 0;

    /// <summary>Valida mandante y campamento de la sesión. Devuelve el mensaje de bloqueo o null si puede operar.</summary>
    public static string? ValidarAlcance(int empresaMandanteId, int campamentoId)
    {
        if (empresaMandanteId <= 0)
            return ReglasAlcance.SinEmpresaMandante;
        if (!TieneCampamento(campamentoId))
            return ReglasAlcance.SinCampamento;
        return null;
    }

  public static void ExigirAlcance(int empresaMandanteId, int campamentoId)
  {
    var mensaje = ValidarAlcance(empresaMandanteId, campamentoId);
    if (mensaje is not null)
      throw new InvalidOperationException(mensaje);
  }
}
