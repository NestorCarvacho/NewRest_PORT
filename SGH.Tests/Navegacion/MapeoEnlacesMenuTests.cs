using SGH.Application.Navegacion;

namespace SGH.Tests.Navegacion;

public sealed class MapeoEnlacesMenuTests
{
    [Theory]
    [InlineData("/app/gestion/censo", MenuClaves.Censo)]
    [InlineData("/app/gestion/miscensos", MenuClaves.CensoQr)]
    [InlineData("/app/gestion/solicitud", MenuClaves.Solicitud)]
    [InlineData("/api/solicitudAlojamiento/list", MenuClaves.Solicitud)]
    public void TryMapear_reconoce_rutas_app(string link, string claveEsperada)
    {
        Assert.True(MapeoEnlacesMenu.TryMapear(link, out var mapeo));
        Assert.Equal(claveEsperada, mapeo.ClaveModulo);
    }

    [Fact]
    public void TryMapear_informe_censo_no_abre_modulo_censo()
    {
        Assert.False(MapeoEnlacesMenu.TryMapear("/app/gestion/informe/censo", out _));
    }

    [Fact]
    public void ClavesDesdeRutaPagina_devuelve_modulos_de_la_pagina()
    {
        var claves = MapeoEnlacesMenu.ClavesDesdeRutaPagina("/Reserva/Datos");
        Assert.Contains(MenuClaves.Reserva, claves);
    }
}