using System.Data;
using Dapper;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Habitacion.Queries;

public sealed class ConsultaBano(IFabricaConexionSql fabrica) : IConsultaBano
{
    public async Task<IReadOnlyList<BanoListado>> ListarAsync(
        int campamentoId, int soloActivos, int banoId = 0, CancellationToken cancellationToken = default)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<BanoListado>(new CommandDefinition(
            "dbo.he_sp_bano_listar",
            new { CampamentoId = campamentoId, SoloActivos = soloActivos, BanoId = banoId },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 120,
            cancellationToken: cancellationToken));
        return filas.AsList();
    }

    public async Task<IReadOnlyList<HabitacionVinculadaBano>> ListarHabitacionesActivasPorBanoAsync(
        int banoId, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<HabitacionVinculadaBano>(new CommandDefinition(
            "dbo.he_sp_bano_habitaciones_activas_por_bano",
            new { BanoId = banoId, CampamentoId = campamentoId },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 120,
            cancellationToken: cancellationToken));
        return filas.AsList();
    }
}