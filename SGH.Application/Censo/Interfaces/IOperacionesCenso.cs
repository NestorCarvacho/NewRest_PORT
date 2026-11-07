using SGH.Application.Censo.DTOs;

namespace SGH.Application.Censo.Interfaces;

public interface IBuscarCensos
{
    Task<ResultadoBusquedaCenso> EjecutarAsync(SolicitudBuscarCensos solicitud, CancellationToken cancellationToken);
}

public interface IObtenerCatalogoCenso
{
    Task<CatalogoCensoDto> EjecutarAsync(int campamentoId, CancellationToken cancellationToken);
}

public interface IGuardarCensos
{
    Task<ResultadoGuardadoCenso> EjecutarAsync(SolicitudGuardarCensos solicitud, CancellationToken cancellationToken);
}
