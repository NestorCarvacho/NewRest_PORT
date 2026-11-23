using SGH.Domain.Censo;

namespace SGH.Tests.Censo;

public class ReglasCensoOcupacionTests
{
    [Theory]
    [InlineData(0, 2, false)]
    [InlineData(1, 2, false)]
    [InlineData(2, 2, false)]
    [InlineData(3, 2, true)]
    public void RadioDiaRespetaCapacidad(int valor, int capacidad, bool deshabilitado)
    {
        Assert.Equal(!deshabilitado, ReglasCenso.RadioDiaHabilitado(valor, capacidad));
    }

    [Theory]
    [InlineData(0, 2, 2, false)]
    [InlineData(1, 2, 2, true)]
    [InlineData(0, 2, 1, false)]
    [InlineData(1, 2, 1, false)]
    public void RadioNocheRespetaResto(int valor, int capacidad, int camasDia, bool deshabilitado)
    {
        Assert.Equal(!deshabilitado, ReglasCenso.RadioNocheHabilitado(valor, capacidad, camasDia));
    }

    [Fact]
    public void AmbosDosDiaDejaSoloCeroEnNoche()
    {
        Assert.False(ReglasCenso.RadioNocheHabilitado(1, 2, 2));
        Assert.True(ReglasCenso.RadioNocheHabilitado(0, 2, 2));
    }

    [Fact]
    public void AjustarNocheTrasSubirDia()
    {
        Assert.Equal(0, ReglasCenso.AjustarCamasNoche(2, 2, 2));
        Assert.Equal(1, ReglasCenso.AjustarCamasNoche(2, 2, 1));
    }

    [Fact]
    public void ValidarAmbosRechazaSumaMayorQueCapacidad()
    {
        Assert.Equal(ReglasCenso.MensajeSumaCamasAmbos, ReglasCenso.ValidarOcupacionCamas(2, "A", 2, 2));
        Assert.Null(ReglasCenso.ValidarOcupacionCamas(2, "A", 1, 1));
    }
}