using SGH.Domain.Censo.Interfaces;

namespace SGH.Application.Censo.Interfaces;

public interface IConsultarGestionDiaria
{
    Task<ResultadoInformeCenso> EjecutarAsync(FiltroGestionDiaria filtro, CancellationToken cancellationToken);
}

public interface IGenerarInformeGeneral
{
    Task<ResultadoInformeCenso> EjecutarAsync(FiltroInformeGeneral filtro, CancellationToken cancellationToken);
}

public interface IGenerarInformeOcupabilidadCenso
{
    Task<ResultadoInformeCenso> EjecutarAsync(FiltroOcupabilidadCenso filtro, CancellationToken cancellationToken);
}

public sealed record ResultadoInformeCenso(IReadOnlyList<IReadOnlyDictionary<string, object?>> Filas);
