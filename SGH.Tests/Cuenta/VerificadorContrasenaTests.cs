using SGH.Application.Cuenta.Opciones;
using SGH.Infrastructure.Cuenta.Security;

namespace SGH.Tests.Cuenta;

public class VerificadorContrasenaTests
{
    private readonly VerificadorContrasena _verificador = new(new OpcionesContrasena { FactorTrabajoBcrypt = 4 });

    [Fact]
    public void LaHuellaMd5DeAbcCoincideConElAlgoritmoLegado()
    {
        Assert.Equal("900150983cd24fb0d6963f7d28e17f72", _verificador.CrearHuellaMd5Legado("abc"));
    }

    [Fact]
    public void ReconoceMd5EnMayusculasYBcrypt()
    {
        var md5 = _verificador.CrearHuellaMd5Legado("abc123").ToUpperInvariant();
        var bcrypt = _verificador.CrearHashBcrypt("abc123");

        Assert.True(_verificador.EsHashMd5Legado(md5));
        Assert.False(_verificador.EsHashMd5Legado(bcrypt));
        Assert.True(_verificador.Coincide("abc123", md5, md5Retirado: false));
        Assert.False(_verificador.Coincide("abc123", md5, md5Retirado: true));
        Assert.True(_verificador.Coincide("abc123", bcrypt, md5Retirado: true));
        Assert.False(_verificador.Coincide("otra123", bcrypt, md5Retirado: true));
    }

    [Fact]
    public void PasswordV2TienePrioridadSinTocarMd5Legado()
    {
        var md5 = _verificador.CrearHuellaMd5Legado("claveLegacy");
        var bcryptNueva = _verificador.CrearHashBcrypt("claveNueva");

        Assert.True(_verificador.Coincide("claveNueva", md5, md5Retirado: true, bcryptNueva));
        Assert.False(_verificador.Coincide("claveLegacy", md5, md5Retirado: true, bcryptNueva));
    }
}
