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

public sealed record CamaListada(
    int Id,
    int HabitacionId,
    string CodigoCama,
    bool Habilitada,
    int Orden,
    bool IsActive);

public sealed record DatosGuardarCama(
    int Id,
    int HabitacionId,
    string CodigoCama,
    bool Habilitada,
    int Orden,
    bool IsActive,
    int UsuarioId);

public sealed record ResultadoGuardarCama(bool Exito, string Mensaje, int Id);

public interface IConsultaBano
{
    Task<IReadOnlyList<BanoListado>> ListarAsync(int campamentoId, int soloActivos, CancellationToken cancellationToken);
    Task<IReadOnlyList<HabitacionVinculadaBano>> ListarHabitacionesActivasPorBanoAsync(
        int banoId, int campamentoId, CancellationToken cancellationToken);
}

public interface IComandoBano
{
    Task<ResultadoGuardarBano> GuardarAsync(DatosGuardarBano datos, int campamentoId, CancellationToken cancellationToken);
}

public interface IConsultaCama
{
    Task<IReadOnlyList<CamaListada>> ListarPorHabitacionAsync(int habitacionId, int campamentoId, CancellationToken cancellationToken);
}

public interface IComandoCama
{
    Task<ResultadoGuardarCama> GuardarAsync(DatosGuardarCama datos, int campamentoId, CancellationToken cancellationToken);
}