using System.Data;
using Dapper;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Habitacion.Queries;

public sealed class ConsultaBano(IFabricaConexionSql fabrica) : IConsultaBano
{
    public async Task<IReadOnlyList<BanoListado>> ListarAsync(
        int campamentoId, int soloActivos, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<BanoListado>(new CommandDefinition(
            "dbo.sp_Bano_Listar",
            new { CampamentoId = campamentoId, SoloActivos = soloActivos },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 120,
            cancellationToken: cancellationToken));
        return filas.AsList();
    }
}