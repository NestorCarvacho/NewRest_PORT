namespace SGH.Domain.Reserva;

public sealed record FiltroReserva(
    int CampamentoId,
    int Id,
    int AreaId,
    string Origen,
    int SolicitudId,
    string Estado,
    DateTime? FechaReserva,
    DateTime? FechaLlegada,
    DateTime? FechaSalida,
    DateTime? CheckIn,
    DateTime? CheckOut,
    int PasajeroId,
    int HabitacionId,
    string TieneAgua);
