using SGH.Domain.Consulta;

namespace SGH.Tests.Consulta;

public class ReglasConsultaListadoTests
{
    [Fact]
    public void SinFechasUsaTreintaDias()
    {
        var rango = ReglasConsultaListado.NormalizarRango(null, null);
        Assert.Equal(30, (rango.Hasta - rango.Desde).Days);
    }

    [Fact]
    public void RangoReservaVacioCuandoNoHayCriterios()
    {
        Assert.True(ReglasConsultaListado.RangoReservaVacío(null, null, null, null, null, null, null));
    }

    [Fact]
    public void AplicarTopeFilasRecortaYMarca()
    {
        var origen = Enumerable.Range(1, ReglasConsultaListado.MaxFilas + 10).ToList();
        var (filas, truncado) = ReglasConsultaListado.AplicarTopeFilas(origen);

        Assert.True(truncado);
        Assert.Equal(ReglasConsultaListado.MaxFilas, filas.Count);
    }
}