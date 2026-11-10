using SGH.Domain.Censo.Interfaces;

namespace SGH.Application.Censo.Interfaces;

public interface IConsultarEstadoPago
{
    Task<ResultadoInformeCenso> EjecutarAsync(FiltroEstadoPago filtro, CancellationToken cancellationToken);
}

public interface IConsultarEstadoPagoPantalla
{
    Task<ResultadoInformeCenso> EjecutarAsync(FiltroEstadoPago filtro, CancellationToken cancellationToken);
}

public interface IConsultarEstadoPagoServicio
{
    Task<ResultadoInformeCenso> EjecutarAsync(FiltroEstadoPagoServicio filtro, CancellationToken cancellationToken);
}

public interface IConsultarEstadoPagoServicioPantalla
{
    Task<ResultadoInformeCenso> EjecutarAsync(FiltroEstadoPagoServicio filtro, CancellationToken cancellationToken);
}
