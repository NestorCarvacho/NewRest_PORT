using System.Data;
using Dapper;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Habitacion.Queries;

public sealed class ConsultaCama(IFabricaConexionSql fabrica) : IConsultaCama
{
    public async Task<IReadOnlyList<CamaListada>> ListarPorHabitacionAsync(
        int habitacionId, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<CamaListada>(new CommandDefinition(
            "dbo.sp_Cama_ListarPorHabitacion",
            new { HabitacionId = habitacionId, CampamentoId = campamentoId },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 120,
            cancellationToken: cancellationToken));
        return filas.AsList();
    }
}