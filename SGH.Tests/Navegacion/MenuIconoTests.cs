using SGH.Application.Navegacion;

namespace SGH.Tests.Navegacion;

public sealed class MenuIconoTests
{
    [Theory]
    [InlineData("fas fa-bed", "fas fa-bed")]
    [InlineData(" FAS fa-COG ", "FAS fa-COG")]
    [InlineData("fab fa-github", "fab fa-github")]
    public void NormalizarClase_acepta_fa5(string entrada, string esperado)
    {
        Assert.Equal(esperado, MenuIcono.NormalizarClase(entrada));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("icon-bed")]
    [InlineData("fas fa-bed;drop table")]
    public void NormalizarClase_rechaza_clases_invalidas(string? entrada)
    {
        Assert.Null(MenuIcono.NormalizarClase(entrada));
    }
}