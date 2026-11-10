namespace SGH.Domain.Censo.Interfaces;

public sealed record FiltroEstadoPago(int EmpresaMandanteId, int CampamentoId, DateTime? Desde, DateTime? Hasta, int MineraId);

public sealed record FiltroEstadoPagoServicio(
    int EmpresaMandanteId, int CampamentoId, DateTime? Desde, DateTime? Hasta,
    int HabitacionId, int ModuloId, int PisoId, string Servicio, int MineraId);

public interface IInformesEstadoPago
{
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> EstadoPagoAsync(FiltroEstadoPago filtro, CancellationToken cancellationToken);
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> EstadoPagoPantallaAsync(FiltroEstadoPago filtro, CancellationToken cancellationToken);
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> EstadoPagoServicioAsync(FiltroEstadoPagoServicio filtro, CancellationToken cancellationToken);
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> EstadoPagoServicioPantallaAsync(FiltroEstadoPagoServicio filtro, CancellationToken cancellationToken);
}
