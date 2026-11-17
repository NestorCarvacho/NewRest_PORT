using System.Security.Cryptography;
using System.Text;
using SGH.Application.Cuenta.Opciones;
using SGH.Domain.Cuenta.Interfaces;

namespace SGH.Infrastructure.Cuenta.Security;

public sealed class VerificadorContrasena : IVerificadorContrasena
{
    private readonly int _factorTrabajo;

    public VerificadorContrasena(OpcionesContrasena opciones)
    {
        var factor = opciones.FactorTrabajoBcrypt;
        _factorTrabajo = factor < 4 ? 4 : factor > 31 ? 31 : factor;
    }

    public bool Coincide(string contrasenaPlana, string hashAlmacenado, bool md5Retirado, string? hashAlmacenadoV2 = null)
    {
        if (string.IsNullOrEmpty(contrasenaPlana))
            return false;

        if (!string.IsNullOrWhiteSpace(hashAlmacenadoV2) && EsHashBcrypt(hashAlmacenadoV2))
            return BCrypt.Net.BCrypt.Verify(contrasenaPlana, hashAlmacenadoV2.Trim());

        if (string.IsNullOrWhiteSpace(hashAlmacenado))
            return false;

        if (md5Retirado || EsHashBcrypt(hashAlmacenado))
            return EsHashBcrypt(hashAlmacenado) && BCrypt.Net.BCrypt.Verify(contrasenaPlana, hashAlmacenado.Trim());

        if (EsHashMd5Legado(hashAlmacenado))
        {
            return string.Equals(
                CrearHuellaMd5Legado(contrasenaPlana),
                hashAlmacenado.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public bool EsHashMd5Legado(string hashAlmacenado)
    {
        if (string.IsNullOrWhiteSpace(hashAlmacenado) || hashAlmacenado.Trim().Length != 32)
            return false;

        foreach (var caracter in hashAlmacenado.Trim())
        {
            var esHex = caracter is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
            if (!esHex)
                return false;
        }

        return true;
    }

    public bool EsHashBcrypt(string hashAlmacenado) =>
        !string.IsNullOrWhiteSpace(hashAlmacenado)
        && hashAlmacenado.Trim().StartsWith("$2", StringComparison.Ordinal);

    public string CrearHashBcrypt(string contrasenaPlana) =>
        BCrypt.Net.BCrypt.HashPassword(contrasenaPlana, _factorTrabajo);

    public string CrearHuellaMd5Legado(string contrasenaPlana)
    {
        var bytes = MD5.HashData(Encoding.ASCII.GetBytes(contrasenaPlana ?? string.Empty));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
