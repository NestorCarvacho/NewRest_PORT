using System.Data;
using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.Interfaces;
using SGH.Infrastructure.Data;

namespace SGH.Infrastructure.Solicitud.Commands;

public sealed class ComandoSolicitudAlojamiento : IComandoSolicitudAlojamiento
{
    public async Task<ResultadoProcedimientoCargaDto> CargarPasajeroAsync(int solicitudId, int empresaMandanteId, int campamentoId,
        int usuarioId, FilaCargaPasajeroDto fila, CancellationToken cancellationToken)
    {
        var oDB = new AD_Global();
        var oCRB = await AbrirAsync(oDB, "dbo.sp_Solicitud_CargarPasajero", cancellationToken);
        if (oCRB.Resultado)
        {
            oCRB = oDB.AddParameter("@SolicitudId", solicitudId);
            oCRB = oDB.AddParameter("@EmpresaMandanteId", empresaMandanteId);
            oCRB = oDB.AddParameter("@CampamentoId", campamentoId);
            oCRB = oDB.AddParameter("@UsuarioId", usuarioId);
            oCRB = oDB.AddParameter("@Identificador", fila.Identificador);
            oCRB = oDB.AddParameter("@TipoIdentificador", fila.TipoIdentificador);
            oCRB = oDB.AddParameter("@Nombres", fila.Nombres);
            oCRB = oDB.AddParameter("@Apellidos", fila.Apellidos);
            oCRB = oDB.AddParameter("@Genero", fila.Genero);
            oCRB = oDB.AddParameter("@Ret", null, ParameterDirection.Output, SqlDbType.Int);
            oCRB = oDB.AddParameter("@Mensaje", null, ParameterDirection.Output, SqlDbType.VarChar, 1000);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        var codigo = oDB.LeerSalidaInt("@Ret");
        var mensaje = oDB.LeerSalidaString("@Mensaje");
        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);
        if (codigo is not int resultado)
            throw new InvalidOperationException("El procedimiento de carga no devolvió un resultado.");

        return new ResultadoProcedimientoCargaDto(resultado > 0, mensaje);
    }

    public Task AsignarHabitacionAsync(int solicitudId, int pasajeroId, int habitacionId, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken) =>
        EjecutarAsync("dbo.sp_Solicitud_AsignarHabitacion", cancellationToken,
            ("@SolicitudId", solicitudId), ("@PasajeroId", pasajeroId), ("@HabitacionId", habitacionId),
            ("@EmpresaMandanteId", empresaMandanteId), ("@CampamentoId", campamentoId), ("@UsuarioId", usuarioId));

    public async Task<int> GenerarReservasAsync(int solicitudId, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
    {
        var id = await EjecutarSalidaAsync("dbo.sp_Solicitud_GenerarReservas", "@PrimeraReservaId", cancellationToken,
            ("@SolicitudId", solicitudId), ("@EmpresaMandanteId", empresaMandanteId),
            ("@CampamentoId", campamentoId), ("@UsuarioId", usuarioId));
        return id > 0 ? id : throw new InvalidOperationException("El procedimiento no devolvió una reserva válida.");
    }

    public async Task<int> GuardarPasajeroAsync(GuardarPasajeroSolicitudDto pasajero, CancellationToken cancellationToken)
    {
        var id = await EjecutarSalidaAsync("dbo.sp_Solicitud_GuardarPasajero", "@Ret", cancellationToken,
            ("@SolicitudId", pasajero.SolicitudId), ("@PasajeroId", pasajero.PasajeroId),
            ("@UsuarioPasajeroId", pasajero.UsuarioPasajeroId), ("@EmpresaMandanteId", pasajero.EmpresaMandanteId),
            ("@CampamentoId", pasajero.CampamentoId), ("@UsuarioId", pasajero.UsuarioId),
            ("@FechaCheckIn", pasajero.FechaCheckIn), ("@FechaCheckOut", pasajero.FechaCheckOut));
        return id > 0 ? id : throw new InvalidOperationException("El procedimiento no devolvió un identificador de pasajero válido.");
    }

    public Task AgregarCuposAsync(int solicitudId, int empresaMandanteId, int campamentoId, int usuarioId, int cantidad, CancellationToken cancellationToken) =>
        EjecutarAsync("dbo.sp_Solicitud_AgregarCupos", cancellationToken,
            ("@SolicitudId", solicitudId), ("@EmpresaMandanteId", empresaMandanteId),
            ("@CampamentoId", campamentoId), ("@UsuarioId", usuarioId), ("@Cantidad", cantidad));

    public Task EliminarPasajeroAsync(int solicitudId, int pasajeroId, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken) =>
        EjecutarAsync("dbo.sp_Solicitud_EliminarPasajero", cancellationToken,
            ("@SolicitudId", solicitudId), ("@PasajeroId", pasajeroId),
            ("@EmpresaMandanteId", empresaMandanteId), ("@CampamentoId", campamentoId));

    public async Task<int> GuardarAsync(DatosSolicitudDto solicitud, CancellationToken cancellationToken)
    {
        var id = await EjecutarSalidaAsync("dbo.sp_Solicitud_Guardar", "@Ret", cancellationToken,
            ("@Id", solicitud.Id), ("@EmpresaMandanteId", solicitud.EmpresaMandanteId),
            ("@CampamentoId", solicitud.CampamentoId), ("@UsuarioId", solicitud.UsuarioId),
            ("@FechaSolicitud", solicitud.FechaSolicitud?.Date), ("@FechaCheckIn", solicitud.FechaCheckIn?.Date),
            ("@FechaCheckOut", solicitud.FechaCheckOut?.Date), ("@GrupoId", solicitud.GrupoId),
            ("@SolicitanteId", solicitud.SolicitanteId), ("@ConSistemaTurno", solicitud.ConSistemaTurno),
            ("@TurnoId", solicitud.TurnoId), ("@TurnoRotacionId", solicitud.TurnoRotacionId),
            ("@Observacion", solicitud.Observacion), ("@Adc", solicitud.Adc),
            ("@Email", solicitud.EmailContactoCampamento), ("@Telefono", solicitud.NumeroContactoCampamento),
            ("@UnidadNegocioId", solicitud.UnidadNegocioId), ("@DivisionId", solicitud.DivisionId),
            ("@DepartamentoId", solicitud.DepartamentoId), ("@NumeroContrato", solicitud.NumeroContrato),
            ("@ReservaVisita", solicitud.ReservaVisita));
        return id > 0 ? id : throw new InvalidOperationException("El procedimiento no devolvió un identificador de solicitud válido.");
    }

    public Task AnularAsync(int id, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken) =>
        EjecutarAsync("dbo.sp_Solicitud_Anular", cancellationToken,
            ("@Id", id), ("@EmpresaMandanteId", empresaMandanteId),
            ("@CampamentoId", campamentoId), ("@UsuarioId", usuarioId));

    private static async Task EjecutarAsync(string procedimiento, CancellationToken cancellationToken, params (string Nombre, object? Valor)[] parametros)
    {
        var oDB = new AD_Global();
        var oCRB = await AbrirAsync(oDB, procedimiento, cancellationToken);
        if (oCRB.Resultado)
            oCRB = Agregar(oDB, parametros);

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);
    }

    private static async Task<int> EjecutarSalidaAsync(string procedimiento, string salida, CancellationToken cancellationToken, params (string Nombre, object? Valor)[] parametros)
    {
        var oDB = new AD_Global();
        var oCRB = await AbrirAsync(oDB, procedimiento, cancellationToken);
        if (oCRB.Resultado)
        {
            oCRB = Agregar(oDB, parametros);
            oCRB = oDB.AddParameter(salida, null, ParameterDirection.Output, SqlDbType.Int);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        var id = oDB.LeerSalidaInt(salida) ?? 0;
        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);

        return id;
    }

    private static async Task<LIB_ControlResultado> AbrirAsync(AD_Global oDB, string procedimiento, CancellationToken cancellationToken)
    {
        var oCRB = await oDB.DBOpenAsync(300, cancellationToken);
        if (oCRB.Resultado)
            oDB.NameProcedure = procedimiento;

        return oCRB;
    }

    private static LIB_ControlResultado Agregar(AD_Global oDB, (string Nombre, object? Valor)[] parametros)
    {
        var oCRB = new LIB_ControlResultado();
        foreach (var (nombre, valor) in parametros)
        {
            oCRB = valor is null
                ? oDB.AddParameter(nombre, null, ParameterDirection.Input, TipoNulo(nombre), -1)
                : oDB.AddParameter(nombre, valor);
        }

        return oCRB;
    }

    private static SqlDbType TipoNulo(string nombre) => nombre switch
    {
        _ when nombre.Contains("Fecha", StringComparison.Ordinal) => SqlDbType.DateTime,
        "@Observacion" or "@Adc" or "@Email" or "@Telefono" or "@NumeroContrato"
            or "@Identificador" or "@TipoIdentificador" or "@Nombres" or "@Apellidos" or "@Genero" => SqlDbType.NVarChar,
        _ => SqlDbType.Int
    };
}
