using System.Data;
using SGH.Domain.Reserva.Interfaces;
using SGH.Infrastructure.Data;

namespace SGH.Infrastructure.Reserva.Commands;

public sealed class ComandoReserva : IComandoReserva
{
    public async Task<int> CrearAsync(
        int empresaMandanteId,
        int campamentoId,
        int areaId,
        DateTime fechaReserva,
        DateTime fechaLlegada,
        DateTime? fechaSalida,
        string? observacion,
        int turnoId,
        int usuarioId,
        CancellationToken cancellationToken)
    {
        var oDB = new AD_Global();
        var oCRB = await oDB.DBOpenAsync(300, cancellationToken);
        if (oCRB.Resultado)
        {
            oDB.NameProcedure = "dbo.he_sp_reserva_crear";
            oCRB = oDB.AddParameter("@EmpresaMandanteId", empresaMandanteId);
            oCRB = oDB.AddParameter("@CampamentoId", campamentoId);
            oCRB = oDB.AddParameter("@AreaId", areaId);
            oCRB = oDB.AddParameter("@FechaReserva", fechaReserva);
            oCRB = oDB.AddParameter("@FechaLlegada", fechaLlegada);
            oCRB = oDB.AddParameter("@FechaSalida", fechaSalida, ParameterDirection.Input, SqlDbType.DateTime);
            oCRB = oDB.AddParameter("@Observacion", observacion, ParameterDirection.Input, SqlDbType.NVarChar, -1);
            oCRB = oDB.AddParameter("@TurnoId", turnoId);
            oCRB = oDB.AddParameter("@UsuarioId", usuarioId);
            oCRB = oDB.AddParameter("@Id", null, ParameterDirection.Output, SqlDbType.Int);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        var id = oCRB.Resultado ? oDB.LeerSalidaInt("@Id") ?? 0 : 0;
        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);

        return id;
    }

    public async Task RegistrarMovimientoAsync(int reservaId, DateTime fecha, int usuarioId, string operacion, int campamentoId, CancellationToken cancellationToken)
    {
        var oDB = new AD_Global();
        var oCRB = await oDB.DBOpenAsync(300, cancellationToken);
        if (oCRB.Resultado)
        {
            oDB.NameProcedure = "dbo.he_sp_reserva_registrar_movimiento";
            oCRB = oDB.AddParameter("@ReservaId", reservaId);
            oCRB = oDB.AddParameter("@Fecha", fecha);
            oCRB = oDB.AddParameter("@UsuarioId", usuarioId);
            oCRB = oDB.AddParameter("@Operacion", operacion);
            oCRB = oDB.AddParameter("@CampamentoId", campamentoId);
            oCRB = oDB.AddParameter("@Ret", null, ParameterDirection.Output, SqlDbType.Int);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);
    }

    public async Task ActualizarAsync(int reservaId, int empresaMandanteId, int campamentoId, int areaId,
        DateTime fechaReserva, DateTime fechaLlegada, DateTime? fechaSalida, string? observacion,
        int turnoId, int? pasajeroId, int? habitacionId, string estado, bool tieneAgua,
        int usuarioId, CancellationToken cancellationToken)
    {
        var oDB = new AD_Global();
        var oCRB = await oDB.DBOpenAsync(300, cancellationToken);
        if (oCRB.Resultado)
        {
            oDB.NameProcedure = "dbo.he_sp_reserva_actualizar";
            oCRB = oDB.AddParameter("@ReservaId", reservaId);
            oCRB = oDB.AddParameter("@EmpresaMandanteId", empresaMandanteId);
            oCRB = oDB.AddParameter("@CampamentoId", campamentoId);
            oCRB = oDB.AddParameter("@AreaId", areaId);
            oCRB = oDB.AddParameter("@FechaReserva", fechaReserva);
            oCRB = oDB.AddParameter("@FechaLlegada", fechaLlegada);
            oCRB = oDB.AddParameter("@FechaSalida", fechaSalida, ParameterDirection.Input, SqlDbType.DateTime);
            oCRB = oDB.AddParameter("@Observacion", observacion, ParameterDirection.Input, SqlDbType.NVarChar, -1);
            oCRB = oDB.AddParameter("@TurnoId", turnoId);
            oCRB = oDB.AddParameter("@PasajeroId", pasajeroId, ParameterDirection.Input, SqlDbType.Int);
            oCRB = oDB.AddParameter("@HabitacionId", habitacionId, ParameterDirection.Input, SqlDbType.Int);
            oCRB = oDB.AddParameter("@Estado", estado);
            oCRB = oDB.AddParameter("@TieneAgua", tieneAgua);
            oCRB = oDB.AddParameter("@UsuarioId", usuarioId);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);
    }

    public async Task AnularAsync(int reservaId, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
    {
        var oDB = new AD_Global();
        var oCRB = await oDB.DBOpenAsync(300, cancellationToken);
        if (oCRB.Resultado)
        {
            oDB.NameProcedure = "dbo.he_sp_reserva_anular";
            oCRB = oDB.AddParameter("@ReservaId", reservaId);
            oCRB = oDB.AddParameter("@EmpresaMandanteId", empresaMandanteId);
            oCRB = oDB.AddParameter("@CampamentoId", campamentoId);
            oCRB = oDB.AddParameter("@UsuarioId", usuarioId);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);
    }

    public async Task FinalizarCheckoutAsync(int reservaId, int empresaMandanteId, int usuarioId, CancellationToken cancellationToken)
    {
        var oDB = new AD_Global();
        var oCRB = await oDB.DBOpenAsync(300, cancellationToken);
        if (oCRB.Resultado)
        {
            oDB.NameProcedure = "dbo.he_sp_reserva_finalizar_checkout";
            oCRB = oDB.AddParameter("@ReservaId", reservaId);
            oCRB = oDB.AddParameter("@EmpresaMandanteId", empresaMandanteId);
            oCRB = oDB.AddParameter("@IdentificadorRut", null, ParameterDirection.Input, SqlDbType.VarChar, 25);
            oCRB = oDB.AddParameter("@UsuarioId", usuarioId);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);
    }

    public async Task ReversarMovimientoAsync(int reservaId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
    {
        var oDB = new AD_Global();
        var oCRB = await oDB.DBOpenAsync(300, cancellationToken);
        if (oCRB.Resultado)
        {
            oDB.NameProcedure = "dbo.he_sp_reserva_reversar_movimiento";
            oCRB = oDB.AddParameter("@ReservaId", reservaId);
            oCRB = oDB.AddParameter("@CampamentoId", campamentoId);
            oCRB = oDB.AddParameter("@UsuarioId", usuarioId);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);
    }
}
