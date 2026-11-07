using SGH.Application.Cuenta.DTOs;

namespace SGH.Application.Cuenta.Interfaces;

public interface IPreautenticarUsuario
{
    Task<ResultadoPreautenticacion> EjecutarAsync(SolicitudPreautenticacion solicitud, CancellationToken cancellationToken);
}
