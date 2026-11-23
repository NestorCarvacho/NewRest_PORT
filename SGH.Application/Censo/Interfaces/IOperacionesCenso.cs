using SGH.Application.Censo.DTOs;

namespace SGH.Application.Censo.Interfaces;

public interface IBuscarCensos
{
    Task<ResultadoBusquedaCenso> EjecutarAsync(SolicitudBuscarCensos solicitud, CancellationToken cancellationToken);
}

public interface IBuscarMisCensos
{
    Task<ResultadoBusquedaMisCensos> EjecutarAsync(
        SolicitudBuscarMisCensos solicitud,
        CancellationToken cancellationToken);
}

public interface IObtenerCatalogoCenso
{
    Task<ResultadoCatalogoCenso> EjecutarAsync(int empresaMandanteId, int campamentoId, CancellationToken cancellationToken);
}

public interface IObtenerHabitacionCensoQr
{
    Task<ResultadoHabitacionCensoQr> EjecutarAsync(int campamentoId, int habitacionId, CancellationToken cancellationToken);
}

public interface IGuardarCensos
{
    Task<ResultadoGuardadoCenso> EjecutarAsync(SolicitudGuardarCensos solicitud, CancellationToken cancellationToken);
}
