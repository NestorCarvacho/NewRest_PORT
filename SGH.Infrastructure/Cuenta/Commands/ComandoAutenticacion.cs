using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGH.Domain.Cuenta;
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
}
