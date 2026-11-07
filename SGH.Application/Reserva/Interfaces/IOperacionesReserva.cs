using SGH.Application.Reserva.DTOs;

namespace SGH.Application.Reserva.Interfaces;

public interface IBuscarReservas
{
    Task<ResultadoBusquedaReservas> EjecutarAsync(SolicitudBuscarReservas solicitud, CancellationToken cancellationToken);
}

public interface IObtenerCatalogoReserva
{
    Task<ResultadoCatalogoReserva> EjecutarAsync(int empresaMandanteId, int campamentoId, CancellationToken cancellationToken);
}

public interface ICrearReserva
{
    Task<ResultadoAltaReserva> EjecutarAsync(SolicitudCrearReserva solicitud, CancellationToken cancellationToken);
}

public interface IRegistrarMovimientoReserva
{
    Task<ResultadoMovimientoReserva> EjecutarAsync(SolicitudMovimientoReserva solicitud, CancellationToken cancellationToken);
}
