using SGH.Domain.Alcance;

namespace SGH.Domain.Censo;

public static class ReglasCenso
{
    public const string TurnoDia = "D";
    public const string TurnoNoche = "N";
    public const string HabitacionAjena = ReglasAlcance.RegistroAjenoCampamento;
    public const string FiltroAjeno = ReglasAlcance.RegistroAjenoCampamento;

    public static string? ValidarBusqueda(DateTime? fecha, string? turno)
    {
        if (fecha is null)
            return "Debe ingresar la fecha de censo.";

        if (turno is not (TurnoDia or TurnoNoche))
            return "Seleccione el tipo de turno.";

        return null;
    }

    public static bool EsErrorDeFila(int resultado) => resultado > 0;
}
