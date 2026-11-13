using SGH.Domain.Cuenta;

namespace SGH.Tests.Cuenta;

public class ReglasCuentaAsignacionTests
{
    [Fact]
    public void SinAsignacionesNoResuelve()
    {
        Assert.Null(ReglasCuenta.ResolverAsignacionAutomatica(0, [], 0));
    }

    [Fact]
    public void UnaSolaAsignacionUsaSuId()
    {
        Assert.Equal(8, ReglasCuenta.ResolverAsignacionAutomatica(1, [20], 8));
    }

    [Fact]
    public void VariosCampamentosRealesPidenElegir()
    {
        Assert.Null(ReglasCuenta.ResolverAsignacionAutomatica(2, [20, 21], 8));
    }

    [Fact]
    public void UnCampamentoVariosPerfilesEligeLaPrimera()
    {
        Assert.Equal(8, ReglasCuenta.ResolverAsignacionAutomatica(2, [20, 20], 8));
    }

    [Fact]
    public void VariasFilasConCampamentoCeroPidenElegir()
    {
        Assert.Null(ReglasCuenta.ResolverAsignacionAutomatica(2, [0, 0], 8));
    }

    [Fact]
    public void SelectorSoloConMasDeUnCampamentoReal()
    {
        Assert.False(ReglasCuenta.RequiereSelectorCampamento(1, [20]));
        Assert.False(ReglasCuenta.RequiereSelectorCampamento(2, [20, 20]));
        Assert.False(ReglasCuenta.RequiereSelectorCampamento(2, [0, 0]));
        Assert.True(ReglasCuenta.RequiereSelectorCampamento(2, [20, 21]));
    }

    [Fact]
    public void AsignacionOperativaRequiereCampamentoYMandante()
    {
        Assert.False(ReglasCuenta.AlgunaAsignacionOperativa([], []));
        Assert.False(ReglasCuenta.AlgunaAsignacionOperativa([0], [9]));
        Assert.False(ReglasCuenta.AlgunaAsignacionOperativa([20], [0]));
        Assert.True(ReglasCuenta.AlgunaAsignacionOperativa([20], [9]));
        Assert.True(ReglasCuenta.AlgunaAsignacionOperativa([0, 20], [0, 9]));
    }
}
