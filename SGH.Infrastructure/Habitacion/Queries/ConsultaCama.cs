using System.Data;
using Dapper;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Habitacion.Queries;

public sealed class ConsultaCama(IFabricaConexionSql fabrica) : IConsultaCama
{
    public async Task<IReadOnlyList<CamaListada>> ListarPorHabitacionAsync(
        int habitacionId, int campamentoId, int soloActivas, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<CamaListada>(new CommandDefinition(
            "dbo.sp_Cama_ListarPorHabitacion",
            new { HabitacionId = habitacionId, CampamentoId = campamentoId, SoloActivas = soloActivas },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 120,
            cancellationToken: cancellationToken));
        return filas.AsList();
    }

    public async Task<int?> ObtenerHabitacionIdDeCamaAsync(
        int camaId, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        return await conexion.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(
            """
            SELECT c.HabitacionId
            FROM dbo.Camas AS c WITH (NOLOCK)
            INNER JOIN dbo.Habitaciones AS h WITH (NOLOCK) ON h.Id = c.HabitacionId
            INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
            INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
            WHERE c.Id = @CamaId AND p.CampamentoId = @CampamentoId
            """,
            new { CamaId = camaId, CampamentoId = campamentoId },
            cancellationToken: cancellationToken));
    }
}