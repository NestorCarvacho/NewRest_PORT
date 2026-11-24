using SGH.Domain.Habitacion;

namespace SGH.Tests.Habitacion;

public sealed class ReglasHabitacionTests
{
    [Fact]
    public void ValidaAltaCorrecta()
    {
        var err = ReglasHabitacion.ValidarAlta("101", 5, 2, "M", 3, 1, 2, false, 0);
        Assert.Null(err);
    }

    [Fact]
    public void RechazaSinModulo()
    {
        var err = ReglasHabitacion.ValidarAlta("101", 0, 2, "M", 3, 1, 2, false, 0);
        Assert.Equal(ReglasHabitacion.ModuloRequerido, err);
    }

    [Fact]
    public void UsoExclusivoRequiereCapacidadCubreCamas()
    {
        var err = ReglasHabitacion.ValidarAlta("Suite", 1, 4, "M", 2, 1, 2, true, 0);
        Assert.Equal(ReglasHabitacion.UsoExclusivoCapacidad, err);
    }

    [Fact]
    public void RechazaSinTipoInstalacion()
    {
        var err = ReglasHabitacion.ValidarAlta("101", 5, 2, "M", 0, 1, 2, false, 0);
        Assert.Equal(ReglasHabitacion.TipoInstalacionRequerido, err);
    }

    [Theory]
    [InlineData("M", "F", false)]
    [InlineData("M", "MIXTO", true)]
  [InlineData("F", "M", false)]
    public void SexoCompatible(string hab, string cmp, bool esperado) =>
        Assert.Equal(esperado, ReglasHabitacion.SexoCompatible(hab, cmp));
}