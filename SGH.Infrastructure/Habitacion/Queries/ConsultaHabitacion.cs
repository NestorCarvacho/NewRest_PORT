using System.Data;
using Dapper;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Habitacion.Queries;

public sealed class ConsultaHabitacion(IFabricaConexionSql fabrica) : IConsultaHabitacion
{
    public async Task<IReadOnlyList<HabitacionListada>> ListarAsync(
        FiltroListadoHabitacion filtro, int usuarioId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<HabitacionListada>(new CommandDefinition(
            "dbo.sp_Habitacion_Listar",
            new
            {
                filtro.EmpresaMandanteId,
                filtro.CampamentoId,
                filtro.PabellonId,
                filtro.ModuloId,
                filtro.TipoInstalacionId,
                filtro.Estado,
                filtro.SoloActivas,
                filtro.SoloReservables
            },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 120,
            cancellationToken: cancellationToken));
        return filas.AsList();
    }

    public async Task<HabitacionDetalle?> ObtenerAsync(int id, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        return await conexion.QuerySingleOrDefaultAsync<HabitacionDetalle>(new CommandDefinition(
            "dbo.sp_Habitacion_Obtener",
            new { Id = id, CampamentoId = campamentoId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task<CatalogoHabitacion> CatalogoAsync(int campamentoId, int empresaMandanteId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        using var multi = await conexion.QueryMultipleAsync(new CommandDefinition(
            "dbo.sp_Habitacion_Catalogo",
            new { CampamentoId = campamentoId, MineraId = empresaMandanteId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return new CatalogoHabitacion(
            (await multi.ReadAsync<OpcionCatalogo>()).AsList(),
            (await multi.ReadAsync<OpcionCatalogo>()).AsList(),
            (await multi.ReadAsync<OpcionListaFila>()).Select(f => new OpcionLista(f.Id, f.Etiqueta)).ToList(),
            (await multi.ReadAsync<OpcionListaFila>()).Select(f => new OpcionLista(f.Id, f.Etiqueta)).ToList(),
            (await multi.ReadAsync<OpcionCatalogo>()).AsList(),
            (await multi.ReadAsync<OpcionCatalogo>()).AsList());
    }

    private sealed class OpcionListaFila
    {
        public string Id { get; set; } = "";
        public string Etiqueta { get; set; } = "";
    }
}