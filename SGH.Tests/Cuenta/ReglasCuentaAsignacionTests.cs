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
}
