using SGH.Domain.Habitacion.Interfaces;

namespace SGH.Application.Habitacion.Interfaces;

public interface IGestionHabitaciones
{
    Task<IReadOnlyList<HabitacionListada>> ListarAsync(FiltroListadoHabitacion filtro, int usuarioId, CancellationToken cancellationToken);
    Task<HabitacionDetalle?> ObtenerAsync(int id, int campamentoId, CancellationToken cancellationToken);
    Task<CatalogoHabitacion> CatalogoAsync(int campamentoId, int empresaMandanteId, CancellationToken cancellationToken);
    Task<ResultadoGuardarHabitacion> GuardarAsync(
        DatosGuardarHabitacion datos, int campamentoId, int empresaMandanteId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BanoListado>> ListarBanosAsync(int campamentoId, int soloActivos, CancellationToken cancellationToken);
    Task<ResultadoGuardarBano> GuardarBanoAsync(DatosGuardarBano datos, int campamentoId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CamaListada>> ListarCamasAsync(int habitacionId, int campamentoId, CancellationToken cancellationToken);
    Task<ResultadoGuardarCama> GuardarCamaAsync(DatosGuardarCama datos, int campamentoId, CancellationToken cancellationToken);
}