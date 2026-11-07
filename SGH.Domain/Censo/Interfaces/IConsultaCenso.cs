using SGH.Domain.Censo.Entities;

namespace SGH.Domain.Censo.Interfaces;

public interface IConsultaCenso
{
    Task<IReadOnlyList<CensoListado>> BuscarAsync(FiltroCenso filtro, CancellationToken cancellationToken);

    Task<CatalogoCenso> ObtenerCatalogoAsync(int campamentoId, CancellationToken cancellationToken);
}
