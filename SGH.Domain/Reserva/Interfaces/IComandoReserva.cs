namespace SGH.Domain.Reserva.Interfaces;

public interface IComandoReserva
{
    Task<int> CrearAsync(
        int empresaMandanteId,
        int campamentoId,
        int areaId,
        DateTime fechaReserva,
        DateTime fechaLlegada,
        DateTime? fechaSalida,
        string? observacion,
        int turnoId,
        int usuarioId,
        CancellationToken cancellationToken);

    Task RegistrarMovimientoAsync(int reservaId, DateTime fecha, int usuarioId, string operacion, int campamentoId, CancellationToken cancellationToken);

    Task ActualizarAsync(int reservaId, int empresaMandanteId, int campamentoId, int areaId,
        DateTime fechaReserva, DateTime fechaLlegada, DateTime? fechaSalida, string? observacion,
        int turnoId, int? pasajeroId, int? habitacionId, string estado, bool tieneAgua,
        int usuarioId, CancellationToken cancellationToken);
}
