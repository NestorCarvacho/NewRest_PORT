using SGH.Application.Solicitud.DTOs;

namespace SGH.Application.Solicitud.Interfaces;

public interface INotificadorSolicitud
{
    Task EnviarReservasGeneradasAsync(
        SolicitudDetalleDto solicitud, IReadOnlyList<PasajeroSolicitudDto> pasajeros, CancellationToken cancellationToken);
}
