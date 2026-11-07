using SGH.Domain.Cuenta;

namespace SGH.Tests.Cuenta;

public class NormalizadorRutTests
{
    [Theory]
    [InlineData("12.345.678-9", "12.345.6789")]
    [InlineData("12345678-9", "123456789")]
    [InlineData(" 123456789 ", "123456789")]
    [InlineData("12.345.678-9-0", "12.345.6789-0")]
    public void QuitaSoloElPrimerGuionComoElLoginLegado(string entrada, string esperado)
    {
        Assert.Equal(esperado, NormalizadorRut.Normalizar(entrada));
    }
}
