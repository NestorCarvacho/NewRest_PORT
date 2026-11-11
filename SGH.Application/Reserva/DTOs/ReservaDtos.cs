namespace SGH.Application.Reserva.DTOs;

public sealed record ReservaFilaDto(
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

public sealed record OpcionDto(int Id, string Etiqueta);

public sealed record OpcionTextoDto(string Id, string Etiqueta);

public sealed record TurnoDto(int Id, string Etiqueta, string TipoTurno);

public sealed record CatalogoReservaDto(
    IReadOnlyList<OpcionDto> Grupos,
    IReadOnlyList<OpcionTextoDto> Origenes,
    IReadOnlyList<OpcionDto> Pasajeros,
    IReadOnlyList<OpcionDto> Habitaciones,
    IReadOnlyList<TurnoDto> Turnos);

public sealed record SolicitudBuscarReservas(
    int EmpresaMandanteId,
    int CampamentoId,
    int Id,
    int AreaId,
    string Origen,
    int SolicitudId,
    string Estado,
    DateTime? FechaReserva,
    DateTime? FechaReservaDesde,
    DateTime? FechaReservaHasta,
    DateTime? FechaLlegada,
    DateTime? FechaSalida,
    DateTime? CheckIn,
    DateTime? CheckOut,
    int PasajeroId,
    int HabitacionId,
    string TieneAgua);

public sealed record SolicitudCrearReserva(
    int EmpresaMandanteId,
    int CampamentoId,
    int UsuarioId,
    int AreaId,
    int TurnoId,
    DateTime FechaReserva,
    DateTime FechaLlegada,
    DateTime? FechaSalida,
    string? Observacion);

public sealed record SolicitudMovimientoReserva(
    int ReservaId, DateTime Fecha, int UsuarioId, string Operacion, int EmpresaMandanteId, int CampamentoId);

public sealed record SolicitudActualizarReserva(
    int ReservaId, int EmpresaMandanteId, int CampamentoId, int AreaId,
    DateTime FechaReserva, DateTime FechaLlegada, DateTime? FechaSalida,
    string? Observacion, int TurnoId, int? PasajeroId, int? HabitacionId,
    string Estado, bool TieneAgua, int UsuarioId);

public abstract record ResultadoBusquedaReservas
{
    private ResultadoBusquedaReservas()
    {
    }

    public sealed record Lista(IReadOnlyList<ReservaFilaDto> Filas) : ResultadoBusquedaReservas;

    public sealed record Rechazada(string Mensaje) : ResultadoBusquedaReservas;
}

public abstract record ResultadoCatalogoReserva
{
    private ResultadoCatalogoReserva()
    {
    }

    public sealed record Listo(CatalogoReservaDto Catalogo) : ResultadoCatalogoReserva;

    public sealed record Rechazado(string Mensaje) : ResultadoCatalogoReserva;
}

public abstract record ResultadoAltaReserva
{
    private ResultadoAltaReserva()
    {
    }

    public sealed record Creada(int Id) : ResultadoAltaReserva;

    public sealed record Rechazada(string Mensaje) : ResultadoAltaReserva;
}

public abstract record ResultadoMovimientoReserva
{
    private ResultadoMovimientoReserva()
    {
    }

    public sealed record Registrado : ResultadoMovimientoReserva;

    public sealed record Rechazado(string Mensaje) : ResultadoMovimientoReserva;
}

public abstract record ResultadoActualizacionReserva
{
    private ResultadoActualizacionReserva()
    {
    }

    public sealed record Actualizada : ResultadoActualizacionReserva;

    public sealed record Rechazada(string Mensaje) : ResultadoActualizacionReserva;
}
