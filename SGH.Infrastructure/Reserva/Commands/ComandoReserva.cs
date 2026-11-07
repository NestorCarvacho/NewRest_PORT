using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGH.Domain.Reserva.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Reserva.Commands;

public sealed class ComandoReserva : IComandoReserva
{
    private readonly HotelDbContext _db;

    public ComandoReserva(HotelDbContext db)
    {
        _db = db;
    }

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
        var id = new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.Output };
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            EXEC dbo.sp_Reserva_Crear
                @EmpresaMandanteId={empresaMandanteId},
                @CampamentoId={campamentoId},
                @AreaId={areaId},
                @FechaReserva={fechaReserva},
                @FechaLlegada={fechaLlegada},
                @FechaSalida={fechaSalida},
                @Observacion={observacion},
                @TurnoId={turnoId},
                @UsuarioId={usuarioId},
                @Id={id} OUT
            """,
            cancellationToken);

        return id.Value is int creado ? creado : 0;
    }

    public async Task RegistrarMovimientoAsync(int reservaId, DateTime fecha, int usuarioId, string operacion, int campamentoId, CancellationToken cancellationToken)
    {
        var ret = new SqlParameter("@Ret", SqlDbType.Int) { Direction = ParameterDirection.Output };
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            EXEC dbo.sp_Reserva_RegistrarMovimiento
                @ReservaId={reservaId},
                @Fecha={fecha},
                @UsuarioId={usuarioId},
                @Operacion={operacion},
                @CampamentoId={campamentoId},
                @Ret={ret} OUT
            """,
            cancellationToken);
    }
}
