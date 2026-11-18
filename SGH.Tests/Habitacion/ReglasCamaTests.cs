using SGH.Domain.Habitacion;

namespace SGH.Tests.Habitacion;

public class ReglasCamaTests
{
    [Fact]
    public void ValidarPerteneceHabitacion_rechaza_cambio_de_habitacion()
    {
        var mensaje = ReglasCama.ValidarPerteneceHabitacion(5, 10, 20);
        Assert.Equal(ReglasCama.CamaAjenaHabitacion, mensaje);
    }

    [Fact]
    public void ValidarPerteneceHabitacion_acepta_misma_habitacion()
    {
        Assert.Null(ReglasCama.ValidarPerteneceHabitacion(5, 10, 10));
    }
}