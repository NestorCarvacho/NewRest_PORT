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

public interface IActualizarReserva
{
    Task<ResultadoActualizacionReserva> EjecutarAsync(SolicitudActualizarReserva solicitud, CancellationToken cancellationToken);
}

public interface IAnularReserva
{
    Task<ResultadoAnularReserva> EjecutarAsync(SolicitudAnularReserva solicitud, CancellationToken cancellationToken);
}

public interface IFinalizarReserva
{
    Task<ResultadoFinalizarReserva> EjecutarAsync(SolicitudFinalizarReserva solicitud, CancellationToken cancellationToken);
}

public interface IReversarMovimientoReserva
{
    Task<ResultadoReversarMovimientoReserva> EjecutarAsync(SolicitudReversarMovimientoReserva solicitud, CancellationToken cancellationToken);
}

public interface IFinalizarMasivoReserva
{
    Task<ResultadoFinalizarMasivoReserva> EjecutarAsync(SolicitudFinalizarMasivoReserva solicitud, CancellationToken cancellationToken);
}