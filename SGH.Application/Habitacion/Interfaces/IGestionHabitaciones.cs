using SGH.Domain.Habitacion.Interfaces;

namespace SGH.Application.Habitacion.Interfaces;

public interface IGestionHabitaciones
{
    Task<IReadOnlyList<HabitacionListada>> ListarAsync(FiltroListadoHabitacion filtro, int usuarioId, CancellationToken cancellationToken);
    Task<HabitacionDetalle?> ObtenerAsync(int id, int campamentoId, CancellationToken cancellationToken);
    Task<CatalogoHabitacion> CatalogoAsync(int campamentoId, int empresaMandanteId, CancellationToken cancellationToken);
    Task<ResultadoGuardarHabitacion> GuardarAsync(DatosGuardarHabitacion datos, int campamentoId, CancellationToken cancellationToken);
}