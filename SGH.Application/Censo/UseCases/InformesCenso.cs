using SGH.Application.Censo.Interfaces;
using SGH.Domain.Censo.Interfaces;
using SGH.Application.Sesion;

namespace SGH.Application.Censo.UseCases;

public sealed class ConsultarGestionDiaria : IConsultarGestionDiaria
{
    private readonly IInformesCenso _informes;
    public ConsultarGestionDiaria(IInformesCenso informes) => _informes = informes;

    public async Task<ResultadoInformeCenso> EjecutarAsync(FiltroGestionDiaria filtro, CancellationToken cancellationToken)
    {
        AlcanceSesion.ExigirAlcance(filtro.EmpresaMandanteId, filtro.CampamentoId);
        if (string.IsNullOrWhiteSpace(filtro.Turno) || filtro.Fecha == default)
            throw new ArgumentException("Debe ingresar fecha y turno.");
        return new ResultadoInformeCenso(await _informes.GestionDiariaAsync(filtro, cancellationToken));
    }
}

public sealed class GenerarInformeGeneral : IGenerarInformeGeneral
{
    private readonly IInformesCenso _informes;
    public GenerarInformeGeneral(IInformesCenso informes) => _informes = informes;

    public async Task<ResultadoInformeCenso> EjecutarAsync(FiltroInformeGeneral filtro, CancellationToken cancellationToken)
    {
        AlcanceSesion.ExigirAlcance(filtro.EmpresaMandanteId, filtro.CampamentoId);
        if (filtro.Desde.Date > filtro.Hasta.Date)
            throw new ArgumentException("El período informado no es válido.");
        return new ResultadoInformeCenso(await _informes.InformeGeneralAsync(filtro, cancellationToken));
    }

}

public sealed class GenerarInformeOcupabilidadCenso : IGenerarInformeOcupabilidadCenso
{
    private readonly IInformesCenso _informes;
    public GenerarInformeOcupabilidadCenso(IInformesCenso informes) => _informes = informes;

    public async Task<ResultadoInformeCenso> EjecutarAsync(FiltroOcupabilidadCenso filtro, CancellationToken cancellationToken)
    {
        AlcanceSesion.ExigirAlcance(filtro.EmpresaMandanteId, filtro.CampamentoId);
        if (filtro.Desde.Date > filtro.Hasta.Date)
            throw new ArgumentException("El período informado no es válido.");
        if (string.IsNullOrWhiteSpace(filtro.Tipo) || string.IsNullOrWhiteSpace(filtro.Nivel))
            throw new ArgumentException("Debe indicar tipo y nivel.");
        return new ResultadoInformeCenso(await _informes.OcupabilidadAsync(filtro, cancellationToken));
    }

}
