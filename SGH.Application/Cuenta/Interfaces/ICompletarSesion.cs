using SGH.Application.Cuenta.DTOs;

namespace SGH.Application.Cuenta.Interfaces;

public interface ICompletarSesion
{
    Task<ResultadoAutenticacion> EjecutarAsync(SolicitudCompletarSesion solicitud, CancellationToken cancellationToken);
}
