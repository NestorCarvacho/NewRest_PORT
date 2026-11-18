namespace SGH.Domain.Habitacion.Interfaces;

public sealed record FiltroListadoHabitacion(
    int EmpresaMandanteId,
    int CampamentoId,
    int PabellonId,
    int ModuloId,
    int TipoInstalacionId,
    string? Estado,
    int SoloActivas,
    int? SoloReservables);

public sealed record HabitacionListada(
    int Id,
    string Nombre,
    string? Sexo,
    string? Estado,
    int CantidadCamas,
    int CamasHabilitadas,
    bool IsActive,
    bool IsReservable,
    int ModuloId,
    string? ModuloNombre,
    int PabellonId,
    string? PabellonNombre,
    int? BanoId,
    string? BanoNombre,
    bool PermiteUsoPorTurnos,
    bool PermiteUsoExclusivo);

public sealed record HabitacionDetalle(
    int Id,
    string Nombre,
    int ModuloId,
    string? Sexo,
    string? Estado,
    int CantidadCamas,
    bool IsActive,
    bool IsReservable,
    int TipoInstalacionId,
    int StandardHabitacionId,
    string? ObservacionBloqueo,
    decimal? M2,
    int? BanoId,
    int PersonasCantidadMin,
    int PersonasCantidadMax,
    int MinutosMin,
    int MinutosMax,
    int TiempoHigienizacionMin,
    bool TieneServiciosAlimentacion,
    bool PermiteUsoPorTurnos,
    bool PermiteUsoExclusivo);

public sealed record OpcionCatalogo(int Id, string Etiqueta, int PadreId = 0);
public sealed record OpcionLista(string Codigo, string Etiqueta);

public sealed record CatalogoHabitacion(
    IReadOnlyList<OpcionCatalogo> Pabellones,
    IReadOnlyList<OpcionCatalogo> Modulos,
    IReadOnlyList<OpcionLista> Sexos,
    IReadOnlyList<OpcionLista> EstadosHabitacion,
    IReadOnlyList<OpcionCatalogo> TiposInstalacion,
    IReadOnlyList<OpcionCatalogo> Standards,
    IReadOnlyList<OpcionCatalogo> Banos,
    IReadOnlyList<OpcionCatalogo> TiposCama);

public interface IConsultaHabitacion
{
    Task<IReadOnlyList<HabitacionListada>> ListarAsync(FiltroListadoHabitacion filtro, int usuarioId, CancellationToken cancellationToken);
    Task<HabitacionDetalle?> ObtenerAsync(int id, int campamentoId, CancellationToken cancellationToken);
    Task<int?> ObtenerIdPorNombreEnCampamentoAsync(int campamentoId, string nombre, CancellationToken cancellationToken);
    Task<CatalogoHabitacion> CatalogoAsync(int campamentoId, int empresaMandanteId, CancellationToken cancellationToken);
}