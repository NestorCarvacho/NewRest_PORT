using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Solicitud.Commands;

public sealed class ComandoSolicitudAlojamiento(HotelDbContext db) : IComandoSolicitudAlojamiento
{
    public async Task AsignarHabitacionAsync(int solicitudId, int pasajeroId, int habitacionId, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            EXEC dbo.sp_Solicitud_AsignarHabitacion @SolicitudId={solicitudId},
                @PasajeroId={pasajeroId}, @HabitacionId={habitacionId},
                @EmpresaMandanteId={empresaMandanteId}, @CampamentoId={campamentoId}, @UsuarioId={usuarioId}
            """, cancellationToken);
    }

    public async Task<int> GenerarReservasAsync(int solicitudId, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
    {
        var ret = new SqlParameter("@PrimeraReservaId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            EXEC dbo.sp_Solicitud_GenerarReservas @SolicitudId={solicitudId},
                @EmpresaMandanteId={empresaMandanteId}, @CampamentoId={campamentoId},
                @UsuarioId={usuarioId}, @PrimeraReservaId={ret} OUTPUT
            """, cancellationToken);
        return ret.Value is int id && id > 0
            ? id : throw new InvalidOperationException("El procedimiento no devolvió una reserva válida.");
    }

    public async Task<int> GuardarPasajeroAsync(GuardarPasajeroSolicitudDto pasajero, CancellationToken cancellationToken)
    {
        var ret = new SqlParameter("@Ret", SqlDbType.Int) { Direction = ParameterDirection.Output };
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            EXEC dbo.sp_Solicitud_GuardarPasajero @SolicitudId={pasajero.SolicitudId},
                @PasajeroId={pasajero.PasajeroId}, @UsuarioPasajeroId={pasajero.UsuarioPasajeroId},
                @EmpresaMandanteId={pasajero.EmpresaMandanteId}, @CampamentoId={pasajero.CampamentoId},
                @UsuarioId={pasajero.UsuarioId}, @FechaCheckIn={pasajero.FechaCheckIn},
                @FechaCheckOut={pasajero.FechaCheckOut}, @Ret={ret} OUTPUT
            """, cancellationToken);
        return ret.Value is int id && id > 0
            ? id : throw new InvalidOperationException("El procedimiento no devolvió un identificador de pasajero válido.");
    }

    public async Task AgregarCuposAsync(int solicitudId, int empresaMandanteId, int campamentoId, int usuarioId, int cantidad, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            EXEC dbo.sp_Solicitud_AgregarCupos @SolicitudId={solicitudId}, @EmpresaMandanteId={empresaMandanteId},
                @CampamentoId={campamentoId}, @UsuarioId={usuarioId}, @Cantidad={cantidad}
            """, cancellationToken);
    }

    public async Task EliminarPasajeroAsync(int solicitudId, int pasajeroId, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            EXEC dbo.sp_Solicitud_EliminarPasajero @SolicitudId={solicitudId}, @PasajeroId={pasajeroId},
                @EmpresaMandanteId={empresaMandanteId}, @CampamentoId={campamentoId}
            """, cancellationToken);
    }

    public async Task<int> GuardarAsync(DatosSolicitudDto solicitud, CancellationToken cancellationToken)
    {
        var ret = new SqlParameter("@Ret", SqlDbType.Int) { Direction = ParameterDirection.Output };
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            EXEC dbo.sp_Solicitud_Guardar
                @Id={solicitud.Id}, @EmpresaMandanteId={solicitud.EmpresaMandanteId},
                @CampamentoId={solicitud.CampamentoId}, @UsuarioId={solicitud.UsuarioId},
                @FechaSolicitud={solicitud.FechaSolicitud?.Date}, @FechaCheckIn={solicitud.FechaCheckIn?.Date},
                @FechaCheckOut={solicitud.FechaCheckOut?.Date},
                @GrupoId={solicitud.GrupoId}, @SolicitanteId={solicitud.SolicitanteId},
                @ConSistemaTurno={solicitud.ConSistemaTurno}, @TurnoId={solicitud.TurnoId},
                @TurnoRotacionId={solicitud.TurnoRotacionId}, @Observacion={solicitud.Observacion},
                @Adc={solicitud.Adc}, @Email={solicitud.EmailContactoCampamento},
                @Telefono={solicitud.NumeroContactoCampamento}, @UnidadNegocioId={solicitud.UnidadNegocioId},
                @DivisionId={solicitud.DivisionId}, @DepartamentoId={solicitud.DepartamentoId},
                @NumeroContrato={solicitud.NumeroContrato}, @ReservaVisita={solicitud.ReservaVisita},
                @Ret={ret} OUTPUT
            """, cancellationToken);
        return ret.Value is int id && id > 0
            ? id : throw new InvalidOperationException("El procedimiento no devolvió un identificador de solicitud válido.");
    }

    public async Task AnularAsync(int id, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            EXEC dbo.sp_Solicitud_Anular @Id={id}, @EmpresaMandanteId={empresaMandanteId},
                @CampamentoId={campamentoId}, @UsuarioId={usuarioId}
            """, cancellationToken);
    }
}
