using System.Data;
using SGH.Domain.Cuenta;
using SGH.Domain.Cuenta.Entities;
using SGH.Domain.Cuenta.Interfaces;
using SGH.Infrastructure.Data;

namespace SGH.Infrastructure.Cuenta.Commands;

public sealed class ComandoAutenticacion : IComandoAutenticacion
{
    public async Task<ResultadoIntentoClave> RegistrarIntentoFallidoAsync(string rut, CancellationToken cancellationToken)
    {
        var oDB = new AD_Global();
        var oCRB = await oDB.DBOpenAsync(300, cancellationToken);
        if (oCRB.Resultado)
        {
            oDB.NameProcedure = "dbo.he_sp_usuario_registrar_intento_fallido";
            oCRB = oDB.AddParameter("@Rut", rut);
            oCRB = oDB.AddParameter("@Intentos", null, ParameterDirection.Output, SqlDbType.Int);
            oCRB = oDB.AddParameter("@Bloqueada", null, ParameterDirection.Output, SqlDbType.Bit);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        var intentos = oCRB.Resultado ? oDB.LeerSalidaInt("@Intentos") ?? 0 : 0;
        var bloqueada = oCRB.Resultado && oDB.LeerSalidaBool("@Bloqueada");
        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);

        return new ResultadoIntentoClave(intentos, bloqueada);
    }

    public async Task ReiniciarIntentosAsync(string rut, CancellationToken cancellationToken)
    {
        var oDB = new AD_Global();
        var oCRB = await oDB.DBOpenAsync(300, cancellationToken);
        if (oCRB.Resultado)
        {
            oDB.NameProcedure = "dbo.he_sp_usuario_reiniciar_intentos";
            oCRB = oDB.AddParameter("@Rut", rut);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);
    }

    public Task<int> ActualizarPasswordAsync(string rut, string hashBcrypt, string? huellaMd5, CancellationToken cancellationToken) =>
        EjecutarRetornoAsync("dbo.he_sp_usuario_actualizar_password", cancellationToken, ("@Rut", rut), ("@PasswordHash", hashBcrypt), ("@HuellaMd5", huellaMd5));

    public Task<int> ReemplazarHashAsync(string rut, string hashBcrypt, CancellationToken cancellationToken) =>
        EjecutarRetornoAsync("dbo.he_sp_usuario_reemplazar_hash", cancellationToken, ("@Rut", rut), ("@PasswordHash", hashBcrypt));

    public Task<int> RetirarMd5Async(string rut, CancellationToken cancellationToken) =>
        EjecutarRetornoAsync("dbo.he_sp_usuario_retirar_md5", cancellationToken, ("@Rut", rut));

    private static async Task<int> EjecutarRetornoAsync(string procedimiento, CancellationToken cancellationToken, params (string Nombre, object? Valor)[] parametros)
    {
        var oDB = new AD_Global();
        var oCRB = await oDB.DBOpenAsync(300, cancellationToken);
        if (oCRB.Resultado)
        {
            oDB.NameProcedure = procedimiento;
            foreach (var (nombre, valor) in parametros)
            {
                if (nombre == "@HuellaMd5")
                    oCRB = oDB.AddParameter(nombre, valor, ParameterDirection.Input, SqlDbType.VarChar, 32);
                else
                    oCRB = oDB.AddParameter(nombre, valor);
            }

            oCRB = oDB.AddParameter("@Ret", null, ParameterDirection.Output, SqlDbType.Int);
        }

        if (oCRB.Resultado)
            oCRB = await oDB.ExecuteNonQueryAsync(cancellationToken);

        var codigo = oCRB.Resultado ? oDB.LeerSalidaInt("@Ret") ?? CodigoCambioClave.NoActualizada : CodigoCambioClave.NoActualizada;
        oDB.DBClose();
        if (!oCRB.Resultado)
            throw new InvalidOperationException(oCRB.Mensaje);

        return codigo;
    }
}
