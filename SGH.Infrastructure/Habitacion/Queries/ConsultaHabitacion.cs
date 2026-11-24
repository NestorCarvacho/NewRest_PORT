using System.Data;
using Dapper;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Habitacion.Queries;

public sealed class ConsultaHabitacion(IFabricaConexionSql fabrica, IConsultaTipoCama consultaTipoCama) : IConsultaHabitacion
{
    public async Task<IReadOnlyList<HabitacionListada>> ListarAsync(
        FiltroListadoHabitacion filtro, int usuarioId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<HabitacionListada>(new CommandDefinition(
            "dbo.he_sp_habitacion_listar",
            new
            {
                filtro.EmpresaMandanteId,
                filtro.CampamentoId,
                filtro.PabellonId,
                filtro.ModuloId,
                filtro.TipoInstalacionId,
                filtro.Estado,
                filtro.SoloActivas,
                filtro.SoloReservables,
                HabitacionId = filtro.HabitacionId
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
            "dbo.he_sp_habitacion_obtener",
            new { Id = id, CampamentoId = campamentoId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task<int?> ObtenerIdPorNombreEnCampamentoAsync(
        int campamentoId, string nombre, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return null;
        using var conexion = fabrica.Crear();
        return await conexion.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(
            "dbo.he_sp_habitacion_id_por_nombre",
            new { CampamentoId = campamentoId, Nombre = nombre.Trim() },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task<CatalogoHabitacion> CatalogoAsync(int campamentoId, int empresaMandanteId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        using var multi = await conexion.QueryMultipleAsync(new CommandDefinition(
            "dbo.he_sp_habitacion_catalogo",
            new { CampamentoId = campamentoId, MineraId = empresaMandanteId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var pabellones = (await multi.ReadAsync<OpcionCatalogo>()).AsList();
        var modulos = (await multi.ReadAsync<OpcionCatalogo>()).AsList();
        var sexos = (await multi.ReadAsync<OpcionListaFila>()).Select(f => new OpcionLista(f.Id, f.Etiqueta)).ToList();
        var estados = (await multi.ReadAsync<OpcionListaFila>()).Select(f => new OpcionLista(f.Id, f.Etiqueta)).ToList();
        var tipos = (await multi.ReadAsync<OpcionCatalogo>()).AsList();
        var standards = (await multi.ReadAsync<OpcionCatalogo>()).AsList();
        IReadOnlyList<OpcionCatalogo> banos = [];
        IReadOnlyList<OpcionCatalogo> tiposCama = [];
        if (!multi.IsConsumed)
        {
            try
            {
                banos = (await multi.ReadAsync<OpcionCatalogo>()).AsList();
            }
            catch (InvalidOperationException)
            {
                /* catálogo sin 7.º result set (pre-002) */
            }
        }
        if (!multi.IsConsumed)
        {
            try
            {
                tiposCama = (await multi.ReadAsync<OpcionCatalogo>()).AsList();
            }
            catch (InvalidOperationException)
            {
                /* catálogo sin 8.º result set (pre CU-HAB-02) */
            }
        }

        if (tiposCama.Count == 0)
            tiposCama = await OpcionesTiposCamaAsync(campamentoId, cancellationToken);

        return new CatalogoHabitacion(pabellones, modulos, sexos, estados, tipos, standards, banos, tiposCama);
    }

    private async Task<IReadOnlyList<OpcionCatalogo>> OpcionesTiposCamaAsync(
        int campamentoId, CancellationToken cancellationToken)
    {
        var filas = await consultaTipoCama.ListarAsync(campamentoId, soloActivos: 1, tipoCamaId: 0, cancellationToken);
        return filas
            .Select(t => new OpcionCatalogo(t.Id, $"{t.Nombre} ({t.AnchoCm}×{t.LargoCm} cm)", 0))
            .ToList();
    }

    private sealed class OpcionListaFila
    {
        public string Id { get; set; } = "";
        public string Etiqueta { get; set; } = "";
    }
}