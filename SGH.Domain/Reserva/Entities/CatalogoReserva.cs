namespace SGH.Domain.Reserva.Entities;

public sealed record OpcionNumero(int Id, string Etiqueta);

public sealed record OpcionTexto(string Id, string Etiqueta);

public sealed record TurnoDisponible(int Id, string Nombre, string TipoTurno);

public sealed record CatalogoReserva(
    IReadOnlyList<OpcionNumero> Grupos,
    IReadOnlyList<OpcionTexto> Origenes,
    IReadOnlyList<OpcionNumero> Pasajeros,
    IReadOnlyList<OpcionNumero> Habitaciones,
    IReadOnlyList<TurnoDisponible> Turnos);
