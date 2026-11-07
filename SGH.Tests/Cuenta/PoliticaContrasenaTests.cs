using SGH.Domain.Cuenta.ValueObjects;

namespace SGH.Tests.Cuenta;

public class PoliticaContrasenaTests
{
    [Fact]
    public void AceptaSeisCaracteresConTresLetrasYTresNumeros()
    {
        var resultado = PoliticaContrasena.Evaluar("abc123");

        Assert.True(resultado.EsValida);
        Assert.Equal(0, resultado.Codigo);
    }

    [Fact]
    public void NoAplicaElTopeDeDiezDeclaradoEnElLegado()
    {
        var resultado = PoliticaContrasena.Evaluar("abcd1234567");

        Assert.True(resultado.EsValida);
    }

    [Theory]
    [InlineData("abc12", "Clave debe ser mayor a 6")]
    [InlineData("ab1234", "Clave debe tener al menos 3 caracteres")]
    [InlineData("abcdef", "Clave debe tener al menos 3 números")]
    [InlineData("abc1123", "Clave no debe tener caracteres consecutivos")]
    public void RechazaConElMensajeDelServicioLegado(string clave, string mensaje)
    {
        var resultado = PoliticaContrasena.Evaluar(clave);

        Assert.False(resultado.EsValida);
        Assert.Equal(mensaje, resultado.Descripcion);
    }
}
