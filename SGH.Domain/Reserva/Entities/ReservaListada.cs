namespace SGH.Domain.Reserva.Entities;

public sealed record ReservaListada(
    int Id,
    string GrupoEmpresa,
    string Pasajero,
    string Habitacion,
    string Estado,
    string EstadoNumero,
    string Llegada,
    string Salida,
    string? CheckIn,
    string? CheckOut);
