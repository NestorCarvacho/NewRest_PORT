using SGH.Application.Censo.Interfaces;
using SGH.Application.Sesion;
using SGH.Domain.Censo.Interfaces;

namespace SGH.Application.Censo.UseCases;

public sealed class ConsultarEstadoPago(IInformesEstadoPago informes) : IConsultarEstadoPago
{
    public async Task<ResultadoInformeCenso> EjecutarAsync(FiltroEstadoPago filtro, CancellationToken cancellationToken)
    {
        Validar(filtro.EmpresaMandanteId, filtro.CampamentoId, filtro.Desde, filtro.Hasta);
        return new(await informes.EstadoPagoAsync(filtro, cancellationToken));
    }

    internal static void Validar(int empresaMandanteId, int campamentoId, DateTime? desde, DateTime? hasta)
    {
        AlcanceSesion.ExigirAlcance(empresaMandanteId, campamentoId);
        if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date) throw new ArgumentException("El período informado no es válido.");
    }
}

public sealed class ConsultarEstadoPagoPantalla(IInformesEstadoPago informes) : IConsultarEstadoPagoPantalla
{
    public async Task<ResultadoInformeCenso> EjecutarAsync(FiltroEstadoPago filtro, CancellationToken cancellationToken)
    {
        ConsultarEstadoPago.Validar(filtro.EmpresaMandanteId, filtro.CampamentoId, filtro.Desde, filtro.Hasta);
        return new(await informes.EstadoPagoPantallaAsync(filtro, cancellationToken));
    }
}

public sealed class ConsultarEstadoPagoServicio(IInformesEstadoPago informes) : IConsultarEstadoPagoServicio
{
    public async Task<ResultadoInformeCenso> EjecutarAsync(FiltroEstadoPagoServicio filtro, CancellationToken cancellationToken)
    {
        ConsultarEstadoPago.Validar(filtro.EmpresaMandanteId, filtro.CampamentoId, filtro.Desde, filtro.Hasta);
        if (string.IsNullOrWhiteSpace(filtro.Servicio)) throw new ArgumentException("Debe indicar el servicio.");
        return new(await informes.EstadoPagoServicioAsync(filtro, cancellationToken));
    }
}

public sealed class ConsultarEstadoPagoServicioPantalla(IInformesEstadoPago informes) : IConsultarEstadoPagoServicioPantalla
{
    public async Task<ResultadoInformeCenso> EjecutarAsync(FiltroEstadoPagoServicio filtro, CancellationToken cancellationToken)
    {
        ConsultarEstadoPago.Validar(filtro.EmpresaMandanteId, filtro.CampamentoId, filtro.Desde, filtro.Hasta);
        if (string.IsNullOrWhiteSpace(filtro.Servicio)) throw new ArgumentException("Debe indicar el servicio.");
        // Para esta variante se utiliza INF_EstadoPagoServicioPantalla en lugar de INF_EstadoPagoServicio.
        return new(await informes.EstadoPagoServicioPantallaAsync(filtro, cancellationToken));
    }
}
