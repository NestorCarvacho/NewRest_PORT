namespace SGH.Application.Censo.DTOs;

public sealed record CensoFilaDto(
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

public sealed record CensoUsuarioFilaDto(
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

public sealed record OpcionCensoDto(int Id, string Etiqueta, int PadreId);

public sealed record CatalogoCensoDto(
    IReadOnlyList<OpcionCensoDto> Modulos,
    IReadOnlyList<OpcionCensoDto> Pisos,
    IReadOnlyList<OpcionCensoDto> Habitaciones);

public sealed record SolicitudBuscarCensos(
    int EmpresaMandanteId,
    int CampamentoId,
    int ModuloId,
    int PisoId,
    int HabitacionId,
    DateTime? Fecha,
    string? Turno);

public sealed record SolicitudBuscarMisCensos(
    int EmpresaMandanteId,
    int CampamentoId,
    DateTime Fecha,
    int UsuarioId);

public sealed record FilaGuardarCensoDto(string? Id, int HabitacionId, string? Cantidad, string? Turno);

public sealed record SolicitudGuardarCensos(int UsuarioId, int CampamentoId, DateTime Fecha, IReadOnlyList<FilaGuardarCensoDto> Filas);

public abstract record ResultadoCatalogoCenso
{
    private ResultadoCatalogoCenso()
    {
    }

    public sealed record Listo(CatalogoCensoDto Catalogo) : ResultadoCatalogoCenso;

    public sealed record Rechazado(string Mensaje) : ResultadoCatalogoCenso;
}

public abstract record ResultadoBusquedaCenso
{
    private ResultadoBusquedaCenso()
    {
    }

    public sealed record Lista(IReadOnlyList<CensoFilaDto> Filas) : ResultadoBusquedaCenso;

    public sealed record Rechazada(string Mensaje) : ResultadoBusquedaCenso;
}

public abstract record ResultadoBusquedaMisCensos
{
    private ResultadoBusquedaMisCensos()
    {
    }

    public sealed record Lista(IReadOnlyList<CensoUsuarioFilaDto> Filas) : ResultadoBusquedaMisCensos;

    public sealed record Rechazada(string Mensaje) : ResultadoBusquedaMisCensos;
}

public abstract record ResultadoGuardadoCenso
{
    private ResultadoGuardadoCenso()
    {
    }

    public sealed record Guardado : ResultadoGuardadoCenso;

    public sealed record Rechazado(string Mensaje) : ResultadoGuardadoCenso;
}
