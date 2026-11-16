using SGH.Domain.Habitacion;

namespace SGH.Tests.Habitacion;

public sealed class ReglasBanoTests
{
    [Fact]
    public void ValidaGuardarOk()
    {
        Assert.Null(ReglasBano.ValidarGuardar("B1", "Pabellón A", "MIXTO"));
    }

    [Fact]
    public void RechazaSinCodigo()
    {
        Assert.Equal(ReglasBano.CodigoRequerido, ReglasBano.ValidarGuardar("", "U", "MIXTO"));
    }

    [Theory]
    [InlineData("M", "MUJER", false)]
    [InlineData("M", "MIXTO", true)]
    [InlineData("F", "HOMBRE", false)]
    [InlineData("F", "F", true)]
    public void Rn02CompatibleBano(string habitacion, string bano, bool esperado) =>
        Assert.Equal(esperado, ReglasHabitacion.SexoCompatibleBano(habitacion, bano));
}