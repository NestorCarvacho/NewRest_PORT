using SGH.Application.Configuracion;

namespace SGH.Tests.Configuracion;

public class EnmascaramientoSecretosTests
{
    [Fact]
    public void EnmascaraPasswordEnCadenaSql()
    {
        var enmascarada = EnmascaramientoSecretos.EnmascararCadenaConexion(
            "Server=sql;Database=HOTELDB;User Id=app;Password=secreto;TrustServerCertificate=True");
        Assert.DoesNotContain("secreto", enmascarada, StringComparison.Ordinal);
        Assert.Contains("Password=***", enmascarada, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnmascaraClavesSensibles()
    {
        Assert.Equal("***", EnmascaramientoSecretos.EnmascararValor("CorreoSolicitudes:Contrasena", "x"));
        Assert.Equal("smtp.test", EnmascaramientoSecretos.EnmascararValor("CorreoSolicitudes:Host", "smtp.test"));
    }
}