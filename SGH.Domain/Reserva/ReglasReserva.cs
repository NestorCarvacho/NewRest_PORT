using SGH.Domain.Alcance;

namespace SGH.Domain.Reserva;

public static class ReglasReserva
{
    public const int TablaEstado = 6;
    public const int TablaOrigen = 5;
    public const int TablaTipoTurno = 10;
    public const string EstadoPendiente = "P";
    public const string OrigenManual = "R";
    public const string CheckIn = "I";
    public const string CheckOut = "O";
    public const string ReservaAjena = ReglasAlcance.RegistroAjenoCampamento;

    public static string? ValidarAlta(int areaId, int turnoId, DateTime llegada, DateTime? salida)
    {
        if (areaId <= 0)
            return "Seleccione Área";

        if (turnoId <= 0)
            return "Seleccione Turno";

        if (salida.HasValue && salida.Value.Date < llegada.Date)
            return "Fecha Salida debe ser mayor o igual a la fecha de Llegada";

        return null;
    }

    public static string? ValidarMovimiento(int reservaId, DateTime? fecha, string? operacion)
    {
        if (reservaId <= 0)
            return "Seleccione una reserva.";

        if (fecha is null)
            return "Seleccione la fecha.";

        if (operacion is not (CheckIn or CheckOut))
            return "La operación no corresponde a un check-in ni a un check-out.";

        return null;
    }
}
