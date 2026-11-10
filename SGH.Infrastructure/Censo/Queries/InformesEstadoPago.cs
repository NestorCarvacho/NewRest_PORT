using System.Data;
using Dapper;
using SGH.Domain.Censo.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Censo.Queries;

public sealed class InformesEstadoPago(IFabricaConexionSql fabrica) : IInformesEstadoPago
{
    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> EstadoPagoAsync(FiltroEstadoPago f, CancellationToken ct) =>
        EjecutarAsync("INF_EstadoPago", new { empresaMandante = f.EmpresaMandanteId, campamento = f.CampamentoId, fechaDesde = f.Desde, fechaHasta = f.Hasta, minera = f.MineraId }, ct);

    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> EstadoPagoPantallaAsync(FiltroEstadoPago f, CancellationToken ct) =>
        EjecutarAsync("INF_EstadoPagoPantalla", new { empresaMandante = f.EmpresaMandanteId, campamento = f.CampamentoId, fechaDesde = f.Desde, fechaHasta = f.Hasta, minera = f.MineraId }, ct);

    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> EstadoPagoServicioAsync(FiltroEstadoPagoServicio f, CancellationToken ct) =>
        EjecutarAsync("INF_EstadoPagoServicio", new { empresaMandante = f.EmpresaMandanteId, campamento = f.CampamentoId, fechaDesde = f.Desde, fechaHasta = f.Hasta, habitacion = f.HabitacionId, modulo = f.ModuloId, piso = f.PisoId, servicio = f.Servicio, minera = f.MineraId }, ct);

    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> EstadoPagoServicioPantallaAsync(FiltroEstadoPagoServicio f, CancellationToken ct) =>
        EjecutarAsync("INF_EstadoPagoServicioPantalla", new { empresaMandante = f.EmpresaMandanteId, campamento = f.CampamentoId, fechaDesde = f.Desde, fechaHasta = f.Hasta, habitacion = f.HabitacionId, modulo = f.ModuloId, piso = f.PisoId, servicio = f.Servicio, minera = f.MineraId }, ct);

    private async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> EjecutarAsync(string procedure, object parameters, CancellationToken ct)
    {
        using var connection = fabrica.Crear();
        var rows = await connection.QueryAsync(new CommandDefinition(procedure, parameters, commandType: CommandType.StoredProcedure, commandTimeout: 6000, cancellationToken: ct));
        return rows.Select(row => (IReadOnlyDictionary<string, object?>)((IDictionary<string, object>)row)
            .ToDictionary(item => item.Key, item => (object?)item.Value, StringComparer.OrdinalIgnoreCase)).ToList();
    }
}
