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
    public const string EstadoAnulada = "N";
    public const string EstadoFinalizada = "F";
    public const string EstadoAsignada = "A";
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

    public static string? ValidarAnular(int reservaId, string? estado)
    {
        if (reservaId <= 0)
            return "Seleccione una reserva.";

        if (estado is EstadoAnulada or EstadoFinalizada)
            return "La reserva ya está anulada o finalizada.";

        return null;
    }

    public static string? ValidarFinalizar(int reservaId, string? estado, string? checkOut)
    {
        if (reservaId <= 0)
            return "Seleccione una reserva.";

        if (estado != EstadoAsignada)
            return "Solo se puede finalizar una reserva asignada.";

        if (string.IsNullOrWhiteSpace(checkOut))
            return "Registre el check-out antes de finalizar la reserva.";

        return null;
    }
}
