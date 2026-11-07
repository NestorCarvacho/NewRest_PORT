namespace SGH.Domain.Censo;

public static class ReglasCenso
{
    public const string TurnoDia = "D";
    public const string TurnoNoche = "N";
    public const string HabitacionAjena = "La habitación no corresponde al campamento de la sesión.";
    public const string FiltroAjeno = "El filtro no corresponde al campamento de la sesión.";

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
