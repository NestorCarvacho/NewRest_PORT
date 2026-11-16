using SGH.Application.Navegacion;

namespace SGH.Tests.Navegacion;

public sealed class MapeoEnlacesMenuTests
{
    [Theory]
    [InlineData("/api/solicitudAlojamiento/list", MenuClaves.Solicitud)]
    [InlineData("https://app/reservas", MenuClaves.Reserva)]
    [InlineData("/censo/buscar", MenuClaves.Censo)]
    [InlineData("/usuarios", MenuClaves.Usuarios)]
    public void TryMapear_reconoce_fragmentos_del_link(string link, string claveEsperada)
    {
        Assert.True(MapeoEnlacesMenu.TryMapear(link, out var mapeo));
        Assert.Equal(claveEsperada, mapeo.ClaveModulo);
    }

    [Fact]
    public void ClavesDesdeRutaPagina_devuelve_modulos_de_la_pagina()
    {
        var claves = MapeoEnlacesMenu.ClavesDesdeRutaPagina("/Reserva/Datos");
        Assert.Contains(MenuClaves.Reserva, claves);
    }
}