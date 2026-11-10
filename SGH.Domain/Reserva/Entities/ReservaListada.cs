namespace SGH.Domain.Reserva.Entities;

public sealed record ReservaListada(
    int Id,
    int AreaId,
    int PasajeroId,
    int HabitacionId,
    int TurnoId,
    string GrupoEmpresa,
    string Pasajero,
    string Habitacion,
    string Estado,
    string EstadoNumero,
    string FechaReserva,
    string Llegada,
    string Salida,
    string? CheckIn,
    string? CheckOut);
