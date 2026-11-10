using System.Data;
using Dapper;
using SGH.Domain.Censo.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Censo.Queries;

public sealed class InformesCenso : IInformesCenso
{
    private readonly IFabricaConexionSql _fabrica;
    public InformesCenso(IFabricaConexionSql fabrica) => _fabrica = fabrica;

    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> GestionDiariaAsync(FiltroGestionDiaria f, CancellationToken ct) =>
        EjecutarAsync("INF_GestionDia", new { EmpresaMandante = f.EmpresaMandanteId, Campamento = f.CampamentoId, f.Turno, FechaConsulta = f.Fecha }, ct);

    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> InformeGeneralAsync(FiltroInformeGeneral f, CancellationToken ct) =>
        EjecutarAsync("INF_Censo", new { f.Tipo, Campamento = f.CampamentoId, Modulo = f.ModuloId, Piso = f.PisoId, Habitacion = f.HabitacionId, dateFrom = f.Desde, dateTo = f.Hasta, TipoTurno = f.TipoTurno, Minera = f.EmpresaMandanteId }, ct);

    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> OcupabilidadAsync(FiltroOcupabilidadCenso f, CancellationToken ct) =>
        EjecutarAsync("INF_Ocupabilidad", new { f.Tipo, f.SubTipo, f.Nivel, dateFrom = f.Desde, dateTo = f.Hasta, EmpresaMandante = f.EmpresaMandanteId, Campamento = f.CampamentoId, Minera = f.MineraId }, ct);

    private async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> EjecutarAsync(string procedure, object parameters, CancellationToken ct)
    {
        using var connection = _fabrica.Crear();
        var rows = await connection.QueryAsync(new CommandDefinition(procedure, parameters, commandType: CommandType.StoredProcedure, commandTimeout: 120, cancellationToken: ct));
        return rows.Select(row =>
        {
            var valores = (IDictionary<string, object>)row;
            return (IReadOnlyDictionary<string, object?>)valores.ToDictionary(
                item => item.Key,
                item => (object?)item.Value,
                StringComparer.OrdinalIgnoreCase);
        }).ToList();
    }
}
