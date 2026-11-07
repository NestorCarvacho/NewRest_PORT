namespace SGH.Application.Sesion;

public static class AlcanceSesion
{
    public const string SinCampamento = "La sesión no tiene un campamento.";

    public static bool TieneCampamento(int campamentoId) => campamentoId > 0;
}
