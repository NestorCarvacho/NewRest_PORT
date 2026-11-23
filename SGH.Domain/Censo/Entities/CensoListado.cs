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

public sealed record OpcionHabitacionCenso(int Id, string Etiqueta, int PadreId, int TipoInstalacionId, int Capacidad);

public sealed record HabitacionCensoQr(
    int HabitacionId,
    string Habitacion,
    string Modulo,
    string Piso,
    int Capacidad);

public sealed record CatalogoCenso(
    IReadOnlyList<OpcionCenso> Modulos,
    IReadOnlyList<OpcionCenso> Pisos,
    IReadOnlyList<OpcionCenso> Habitaciones,
    IReadOnlyList<OpcionCenso> TiposInstalacion,
    IReadOnlyList<OpcionHabitacionCenso> HabitacionesQr);

public sealed record ResultadoFilaCenso(int Resultado, string? Mensaje);

public sealed record FilaCensoGuardar(string Id, int HabitacionId, int Cantidad, string Turno);
