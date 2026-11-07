using SGH.Domain.Reserva.Entities;

namespace SGH.Domain.Reserva.Interfaces;

public interface IConsultaReserva
{
    Task<IReadOnlyList<ReservaListada>> BuscarAsync(FiltroReserva filtro, CancellationToken cancellationToken);

    Task<CatalogoReserva> ObtenerCatalogoAsync(int empresaMandanteId, int campamentoId, CancellationToken cancellationToken);

    Task<bool> ReservaEstaEnCampamentoAsync(int reservaId, int campamentoId, CancellationToken cancellationToken);
}
