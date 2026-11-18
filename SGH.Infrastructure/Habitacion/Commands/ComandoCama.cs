using System.Data;
using Dapper;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Habitacion.Commands;

public sealed class ComandoCama(IFabricaConexionSql fabrica) : IComandoCama
{
    public async Task<ResultadoGuardarCama> GuardarAsync(
        DatosGuardarCama datos, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var parametros = new DynamicParameters();
        parametros.Add("@Id", datos.Id);
        parametros.Add("@HabitacionId", datos.HabitacionId);
        parametros.Add("@CodigoCama", datos.CodigoCama);
        parametros.Add("@Habilitada", datos.Habilitada);
        parametros.Add("@Orden", datos.Orden);
        parametros.Add("@IsActive", datos.IsActive);
        parametros.Add("@TipoCamaId", datos.TipoCamaId);
        parametros.Add("@FechaColchon", datos.FechaColchon?.Date);
        parametros.Add("@ColchonObservacion", datos.ColchonObservacion);
        parametros.Add("@FechaCambioRopa", datos.FechaCambioRopa?.Date);
        parametros.Add("@RopaCamaObservacion", datos.RopaCamaObservacion);
        parametros.Add("@CampamentoId", campamentoId);
        parametros.Add("@UsuarioId", datos.UsuarioId);
        parametros.Add("@IdResultado", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parametros.Add("@Mensaje", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

        await conexion.ExecuteAsync(new CommandDefinition(
            "dbo.he_sp_cama_guardar",
            parametros,
            commandType: CommandType.StoredProcedure,
            commandTimeout: 120,
            cancellationToken: cancellationToken));

        var mensaje = parametros.Get<string?>("@Mensaje");
        var id = parametros.Get<int?>("@IdResultado") ?? 0;
        if (!string.IsNullOrWhiteSpace(mensaje))
            return new ResultadoGuardarCama(false, mensaje, id);
        if (id <= 0)
            return new ResultadoGuardarCama(false, "No se pudo guardar la cama.", 0);
        return new ResultadoGuardarCama(true, "", id);
    }
}