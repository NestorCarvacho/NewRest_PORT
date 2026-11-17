namespace SGH.Domain.Cuenta.Interfaces;

public interface IVerificadorContrasena
{
    bool Coincide(string contrasenaPlana, string hashAlmacenado, bool md5Retirado, string? hashAlmacenadoV2 = null);

    bool EsHashMd5Legado(string hashAlmacenado);

    bool EsHashBcrypt(string hashAlmacenado);

    string CrearHashBcrypt(string contrasenaPlana);

    string CrearHuellaMd5Legado(string contrasenaPlana);
}
