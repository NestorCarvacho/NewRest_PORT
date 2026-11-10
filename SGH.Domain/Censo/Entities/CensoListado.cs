namespace SGH.Domain.Censo.Entities;

public sealed record CensoListado(
    string Id,
    string Fecha,
    string Modulo,
    string Piso,
    string Habitacion,
    string HabitacionId,
    string EstadoCenso,
    string EstadoHabitacion,
    string NumeroCamas,
    string CamasOcupadas,
    string CamasCheckIn,
    string Turno);

public sealed record CensoUsuarioListado(
    int Id,
    string Fecha,
    string Campamento,
    string Modulo,
    string Piso,
    string Habitacion,
    string NumeroCamas,
    string CamasOcupadas,
    string Turno,
    string TurnoNombre);

public sealed record OpcionCenso(int Id, string Etiqueta, int PadreId);

public sealed record CatalogoCenso(
    IReadOnlyList<OpcionCenso> Modulos,
    IReadOnlyList<OpcionCenso> Pisos,
    IReadOnlyList<OpcionCenso> Habitaciones);

public sealed record ResultadoFilaCenso(int Resultado, string? Mensaje);

public sealed record FilaCensoGuardar(string Id, int HabitacionId, int Cantidad, string Turno);
