using System.Data;
using Dapper;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Habitacion.Commands;

public sealed class ComandoHabitacion(IFabricaConexionSql fabrica) : IComandoHabitacion
{
    public async Task<ResultadoGuardarHabitacion> GuardarAsync(
        DatosGuardarHabitacion datos, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var parametros = new DynamicParameters();
        parametros.Add("@Id", datos.Id);
        parametros.Add("@ModuloId", datos.ModuloId);
        parametros.Add("@Nombre", datos.Nombre);
        parametros.Add("@IsActive", datos.IsActive ? 1 : 0);
        parametros.Add("@CantidadCamas", datos.CantidadCamas);
        parametros.Add("@Sexo", datos.Sexo);
        parametros.Add("@UsuarioId", datos.UsuarioId);
        parametros.Add("@TipoInstalacionId", datos.TipoInstalacionId);
        parametros.Add("@StandardHabitacionId", datos.StandardHabitacionId);
        parametros.Add("@Estado", datos.Estado ?? "D");
        parametros.Add("@ObservacionBloqueo", datos.ObservacionBloqueo);
        parametros.Add("@M2", datos.M2);
        parametros.Add("@IsReservable", datos.IsReservable);
        parametros.Add("@PersonasCantidadMin", datos.UsoExclusivo ? datos.CantidadCamas : datos.PersonasCantidadMin);
        parametros.Add("@PersonasCantidadMax", datos.UsoExclusivo ? datos.CantidadCamas : datos.PersonasCantidadMax);
        parametros.Add("@MinutosMin", datos.UsoPorTurnos ? datos.MinutosMin : 0);
        parametros.Add("@MinutosMax", datos.UsoPorTurnos ? datos.MinutosMax : 0);
        parametros.Add("@TieneServiciosAlimentacion", datos.TieneServiciosAlimentacion ? 1 : 0);
        parametros.Add("@CampamentoId", campamentoId);
        parametros.Add("@IdResultado", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parametros.Add("@Mensaje", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

        await conexion.ExecuteAsync(new CommandDefinition(
            "dbo.sp_Habitacion_Guardar",
            parametros,
            commandType: CommandType.StoredProcedure,
            commandTimeout: 120,
            cancellationToken: cancellationToken));

        var mensaje = parametros.Get<string?>("@Mensaje");
        var id = parametros.Get<int?>("@IdResultado") ?? 0;
        if (!string.IsNullOrWhiteSpace(mensaje))
            return new ResultadoGuardarHabitacion(false, mensaje, id);
        if (id <= 0)
            return new ResultadoGuardarHabitacion(false, "No se pudo guardar la habitación.", 0);
        return new ResultadoGuardarHabitacion(true, "", id);
    }
}