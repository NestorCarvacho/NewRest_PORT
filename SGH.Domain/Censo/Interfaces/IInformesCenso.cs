namespace SGH.Domain.Censo.Interfaces;

public sealed record FiltroGestionDiaria(int EmpresaMandanteId, int CampamentoId, string Turno, DateTime Fecha);

public sealed record FiltroInformeGeneral(
    string Tipo,
    int CampamentoId,
    int ModuloId,
    int PisoId,
    int HabitacionId,
    DateTime Desde,
    DateTime Hasta,
    string TipoTurno,
    int EmpresaMandanteId);

public sealed record FiltroOcupabilidadCenso(
    string Tipo,
    string SubTipo,
    string Nivel,
    DateTime Desde,
    DateTime Hasta,
    int EmpresaMandanteId,
    int CampamentoId,
    int MineraId);


public interface IInformesCenso
{
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> GestionDiariaAsync(
        FiltroGestionDiaria filtro, CancellationToken cancellationToken);

    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> InformeGeneralAsync(
        FiltroInformeGeneral filtro, CancellationToken cancellationToken);

    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> OcupabilidadAsync(
        FiltroOcupabilidadCenso filtro, CancellationToken cancellationToken);

}
