using SGH.Domain.Censo.Entities;

namespace SGH.Domain.Censo.Interfaces;

public interface IConsultaCenso
{
    Task<IReadOnlyList<CensoListado>> BuscarAsync(FiltroCenso filtro, CancellationToken cancellationToken);

    Task<CatalogoCenso> ObtenerCatalogoAsync(int campamentoId, CancellationToken cancellationToken);

    Task<bool> FiltroEstaEnCampamentoAsync(int campamentoId, int moduloId, int pisoId, int habitacionId, CancellationToken cancellationToken);
}
