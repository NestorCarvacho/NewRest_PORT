namespace SGH.Domain.Alcance;

/// <summary>
/// Bloqueo transversal: cada operación usa solo el campamento (y mandante) de la sesión del usuario.
/// </summary>
public static class ReglasAlcance
{
    public const string SinEmpresaMandante = "La sesión no tiene una empresa mandante válida.";
    public const string SinCampamento = "La sesión no tiene un campamento.";
    public const string RegistroAjenoCampamento =
        "El registro no pertenece al campamento de su sesión. No puede consultarlo ni modificarlo.";
}
