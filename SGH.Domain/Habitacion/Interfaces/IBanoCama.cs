namespace SGH.Domain.Habitacion.Interfaces;

public sealed record BanoListado(
    int Id,
    string CodigoNombre,
    string Ubicacion,
    string TipoSexo,
    string Estado,
    bool IsActive,
    int HabitacionesVinculadas);

public sealed record DatosGuardarBano(
    int Id,
    string CodigoNombre,
    string Ubicacion,
    string TipoSexo,
    string Estado,
    bool IsActive,
    int UsuarioId);

public sealed record ResultadoGuardarBano(bool Exito, string Mensaje, int Id);

public sealed record HabitacionVinculadaBano(int Id, string Nombre);

public sealed record TipoCamaListado(
    int Id,
    string Nombre,
    short AnchoCm,
    short LargoCm,
    bool IsActive,
    int CamasVinculadas);

public sealed record DatosGuardarTipoCama(
    int Id,
    string Nombre,
    short AnchoCm,
    short LargoCm,
    bool IsActive,
    int UsuarioId);

public sealed record ResultadoGuardarTipoCama(bool Exito, string Mensaje, int Id);

public sealed record CamaListada(
    int Id,
    int HabitacionId,
    string CodigoCama,
    bool Habilitada,
    int Orden,
    bool IsActive,
    int? TipoCamaId,
    string? TipoCamaNombre,
    short? TipoCamaAnchoCm,
    short? TipoCamaLargoCm,
    DateTime? FechaColchon,
    string? ColchonObservacion,
    DateTime? FechaCambioRopa,
    string? RopaCamaObservacion);

public sealed record DatosGuardarCama(
    int Id,
    int HabitacionId,
    string CodigoCama,
    bool Habilitada,
    int Orden,
    bool IsActive,
    int? TipoCamaId,
    DateTime? FechaColchon,
    string? ColchonObservacion,
    DateTime? FechaCambioRopa,
    string? RopaCamaObservacion,
    int UsuarioId);

public sealed record ResultadoGuardarCama(bool Exito, string Mensaje, int Id);

public interface IConsultaBano
{
    Task<IReadOnlyList<BanoListado>> ListarAsync(
        int campamentoId, int soloActivos, int banoId = 0, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HabitacionVinculadaBano>> ListarHabitacionesActivasPorBanoAsync(
        int banoId, int campamentoId, CancellationToken cancellationToken);
}

public interface IComandoBano
{
    Task<ResultadoGuardarBano> GuardarAsync(DatosGuardarBano datos, int campamentoId, CancellationToken cancellationToken);
}

public interface IConsultaCama
{
    Task<IReadOnlyList<CamaListada>> ListarPorHabitacionAsync(
        int habitacionId, int campamentoId, int soloActivas, CancellationToken cancellationToken);

    Task<int?> ObtenerHabitacionIdDeCamaAsync(int camaId, int campamentoId, CancellationToken cancellationToken);
}

public interface IComandoCama
{
    Task<ResultadoGuardarCama> GuardarAsync(DatosGuardarCama datos, int campamentoId, CancellationToken cancellationToken);
}

public interface IConsultaTipoCama
{
    Task<IReadOnlyList<TipoCamaListado>> ListarAsync(
        int campamentoId, int soloActivos, int tipoCamaId = 0, CancellationToken cancellationToken = default);
}

public interface IComandoTipoCama
{
    Task<ResultadoGuardarTipoCama> GuardarAsync(DatosGuardarTipoCama datos, int campamentoId, CancellationToken cancellationToken);
}