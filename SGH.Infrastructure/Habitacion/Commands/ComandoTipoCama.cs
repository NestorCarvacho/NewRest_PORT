using System.Data;
using Dapper;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Habitacion.Commands;

public sealed class ComandoTipoCama(IFabricaConexionSql fabrica) : IComandoTipoCama
{
    public async Task<ResultadoGuardarTipoCama> GuardarAsync(
        DatosGuardarTipoCama datos, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var parametros = new DynamicParameters();
        parametros.Add("@Id", datos.Id);
        parametros.Add("@CampamentoId", campamentoId);
        parametros.Add("@Nombre", datos.Nombre);
        parametros.Add("@AnchoCm", datos.AnchoCm);
        parametros.Add("@LargoCm", datos.LargoCm);
        parametros.Add("@IsActive", datos.IsActive);
        parametros.Add("@UsuarioId", datos.UsuarioId);
        parametros.Add("@IdResultado", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parametros.Add("@Mensaje", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

        await conexion.ExecuteAsync(new CommandDefinition(
            "dbo.sp_TipoCama_Guardar",
            parametros,
            commandType: CommandType.StoredProcedure,
            commandTimeout: 120,
            cancellationToken: cancellationToken));

        var mensaje = parametros.Get<string?>("@Mensaje");
        var id = parametros.Get<int?>("@IdResultado") ?? 0;
        if (!string.IsNullOrWhiteSpace(mensaje))
            return new ResultadoGuardarTipoCama(false, mensaje, id);
        if (id <= 0)
            return new ResultadoGuardarTipoCama(false, "No se pudo guardar el tipo de cama.", 0);
        return new ResultadoGuardarTipoCama(true, "", id);
    }
}