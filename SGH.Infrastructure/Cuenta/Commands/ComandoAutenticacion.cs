using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGH.Domain.Cuenta;
using SGH.Domain.Cuenta.Entities;
using SGH.Domain.Cuenta.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Cuenta.Commands;

public sealed class ComandoAutenticacion : IComandoAutenticacion
{
    private readonly HotelDbContext _db;

    public ComandoAutenticacion(HotelDbContext db)
    {
        _db = db;
    }

    public async Task<ResultadoIntentoClave> RegistrarIntentoFallidoAsync(string rut, CancellationToken cancellationToken)
    {
        var intentos = new SqlParameter("@Intentos", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var bloqueada = new SqlParameter("@Bloqueada", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sp_Usuario_RegistrarIntentoFallido @Rut={rut}, @Intentos={intentos} OUT, @Bloqueada={bloqueada} OUT",
            cancellationToken);
        var cantidad = intentos.Value is int numero ? numero : 0;
        return new ResultadoIntentoClave(cantidad, EsVerdadero(bloqueada.Value));
    }

    public Task ReiniciarIntentosAsync(string rut, CancellationToken cancellationToken) =>
        _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sp_Usuario_ReiniciarIntentos @Rut={rut}",
            cancellationToken);

    public async Task<int> ActualizarPasswordAsync(string rut, string hashBcrypt, string? huellaMd5, CancellationToken cancellationToken)
    {
        var ret = ParametroRetorno();
        var huella = new SqlParameter("@HuellaMd5", System.Data.SqlDbType.VarChar, 32)
        {
            Value = (object?)huellaMd5 ?? DBNull.Value
        };
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sp_Usuario_ActualizarPassword @Rut={rut}, @PasswordHash={hashBcrypt}, @HuellaMd5={huella}, @Ret={ret} OUT",
            cancellationToken);
        return LeerCodigo(ret);
    }

    public async Task<int> ReemplazarHashAsync(string rut, string hashBcrypt, CancellationToken cancellationToken)
    {
        var ret = ParametroRetorno();
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sp_Usuario_ReemplazarHash @Rut={rut}, @PasswordHash={hashBcrypt}, @Ret={ret} OUT",
            cancellationToken);
        return LeerCodigo(ret);
    }

    public async Task<int> RetirarMd5Async(string rut, CancellationToken cancellationToken)
    {
        var ret = ParametroRetorno();
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sp_Usuario_RetirarMd5 @Rut={rut}, @Ret={ret} OUT",
            cancellationToken);
        return LeerCodigo(ret);
    }

    private static SqlParameter ParametroRetorno() =>
        new("@Ret", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };

    private static int LeerCodigo(SqlParameter ret) =>
        ret.Value is int codigo ? codigo : CodigoCambioClave.NoActualizada;

    private static bool EsVerdadero(object valor) =>
        valor is bool bandera && bandera
        || valor is byte octeto && octeto != 0
        || valor is int numero && numero != 0;
}
