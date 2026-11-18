using System.Data;
using Dapper;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Habitacion.Queries;

public sealed class ConsultaTipoCama(IFabricaConexionSql fabrica) : IConsultaTipoCama
{
    public async Task<IReadOnlyList<TipoCamaListado>> ListarAsync(
        int campamentoId, int soloActivos, int tipoCamaId = 0, CancellationToken cancellationToken = default)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<TipoCamaListado>(new CommandDefinition(
            "dbo.he_sp_tipo_cama_listar",
            new { CampamentoId = campamentoId, SoloActivos = soloActivos, TipoCamaId = tipoCamaId },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 120,
            cancellationToken: cancellationToken));
        return filas.AsList();
    }
}