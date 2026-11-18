using SGH.Domain.Habitacion;

namespace SGH.Tests.Habitacion;

public class ReglasTipoCamaTests
{
    [Fact]
    public void ValidarGuardar_rechaza_medidas_no_positivas()
    {
        var mensaje = ReglasTipoCama.ValidarGuardar("1 plaza", 0, 190);
        Assert.Equal("Ancho y largo deben ser mayores que cero.", mensaje);
    }

    [Fact]
    public void ValidarGuardar_acepta_datos_validos()
    {
        var mensaje = ReglasTipoCama.ValidarGuardar("1 plaza", 90, 190);
        Assert.Null(mensaje);
    }
}