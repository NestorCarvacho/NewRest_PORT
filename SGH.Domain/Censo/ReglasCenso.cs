using SGH.Domain.Alcance;

namespace SGH.Domain.Censo;

public static class ReglasCenso
{
    public const string TurnoDia = "D";
    public const string TurnoNoche = "N";
    public const string TurnoAmbos = "A";
    public const int MaxCamasAbsoluto = 4;
    public const string MensajeSumaCamasAmbos = "La suma de camas día y noche no puede superar la capacidad de la habitación.";
    public const string HabitacionAjena = ReglasAlcance.RegistroAjenoCampamento;
    public const string FiltroAjeno = ReglasAlcance.RegistroAjenoCampamento;

    public static int CapacidadEfectiva(int capacidadTotal) =>
        Math.Clamp(capacidadTotal, 0, MaxCamasAbsoluto);

    public static bool RadioDiaHabilitado(int valor, int capacidadTotal)
    {
        var cap = CapacidadEfectiva(capacidadTotal);
        return valor >= 0 && valor <= cap;
    }

    public static bool RadioNocheHabilitado(int valor, int capacidadTotal, int camasDiaSeleccionadas)
    {
        var maxNoche = Math.Max(0, CapacidadEfectiva(capacidadTotal) - camasDiaSeleccionadas);
        return valor >= 0 && valor <= maxNoche;
    }

    public static int AjustarCamasNoche(int camasNoche, int capacidadTotal, int camasDia) =>
        Math.Min(camasNoche, Math.Max(0, CapacidadEfectiva(capacidadTotal) - camasDia));

    public static string? ValidarOcupacionCamas(int capacidadTotal, string? turno, int camasDia, int camasNoche)
    {
        if (capacidadTotal < 0)
            return "La capacidad de la habitación no es válida.";

        var cap = CapacidadEfectiva(capacidadTotal);
        if (cap == 0 && (camasDia > 0 || camasNoche > 0))
            return "La habitación no tiene camas habilitadas.";

        if (turno == TurnoDia)
            return camasDia >= 0 && camasDia <= cap
                ? null
                : $"Las camas ocupadas de día deben estar entre 0 y {cap}.";

        if (turno == TurnoNoche)
            return camasNoche >= 0 && camasNoche <= cap
                ? null
                : $"Las camas ocupadas de noche deben estar entre 0 y {cap}.";

        if (turno == TurnoAmbos)
        {
            if (!RadioDiaHabilitado(camasDia, capacidadTotal))
                return $"Las camas ocupadas de día deben estar entre 0 y {cap}.";
            if (!RadioNocheHabilitado(camasNoche, capacidadTotal, camasDia))
                return MensajeSumaCamasAmbos;
            if (camasDia + camasNoche > cap)
                return MensajeSumaCamasAmbos;
            return null;
        }

        return "Seleccione un turno válido.";
    }

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
