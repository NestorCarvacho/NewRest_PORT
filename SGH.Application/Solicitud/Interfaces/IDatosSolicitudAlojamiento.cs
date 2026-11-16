using SGH.Application.Solicitud.DTOs;

namespace SGH.Application.Solicitud.Interfaces;

public interface IConsultaSolicitudAlojamiento
{
    Task<IReadOnlyList<SolicitudListadaDto>> BuscarAsync(FiltroSolicitudesDto filtro, CancellationToken cancellationToken);
    Task<SolicitudDetalleDto?> ObtenerAsync(int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PasajeroSolicitudDto>> ListarPasajerosAsync(int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken);
    Task<IReadOnlyList<HabitacionDisponibleSolicitudDto>> HabitacionesDisponiblesAsync(int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken);
    Task<CatalogoSolicitudDto> CatalogoAsync(int empresaMandanteId, int grupoId, CancellationToken cancellationToken);
    Task<CatalogoOrganizacionSolicitudDto> OrganizacionAsync(int empresaMandanteId, int unidadId, int divisionId, CancellationToken cancellationToken);
}

public interface IComandoSolicitudAlojamiento
{
    Task AgregarCuposAsync(int solicitudId, int empresaMandanteId, int campamentoId, int usuarioId, int cantidad, CancellationToken cancellationToken);
    Task<int> GuardarPasajeroAsync(GuardarPasajeroSolicitudDto pasajero, CancellationToken cancellationToken);
    Task AsegurarPeriodoImportacionAsync(int solicitudId, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken);
    Task<ResultadoProcedimientoCargaDto> CargarPasajeroAsync(int solicitudId, int empresaMandanteId, int campamentoId, int usuarioId, FilaCargaPasajeroDto fila, CancellationToken cancellationToken);
    Task<int> GenerarReservasAsync(int solicitudId, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken);
    Task AsignarHabitacionAsync(int solicitudId, int pasajeroId, int habitacionId, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken);
    Task EliminarPasajeroAsync(int solicitudId, int pasajeroId, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken);
    Task<int> GuardarAsync(DatosSolicitudDto solicitud, CancellationToken cancellationToken);
    Task AnularAsync(int id, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken);
}
