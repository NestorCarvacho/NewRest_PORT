using SGH.Application.Cuenta.DTOs;

namespace SGH.Application.Cuenta.Interfaces;

public interface ICambiarContrasena
{
    Task<ResultadoCambioContrasena> EjecutarAsync(SolicitudCambioContrasena solicitud, CancellationToken cancellationToken);
}
