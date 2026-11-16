using SGH.Application.Solicitud.DTOs;

namespace SGH.Web.Correo;

public sealed class CorreoReservasGeneradasVm
{
    public required SolicitudDetalleDto Solicitud { get; init; }
    public required IReadOnlyList<PasajeroSolicitudDto> Pasajeros { get; init; }
}