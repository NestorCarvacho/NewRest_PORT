using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SGH.Tests.Cuenta;

public class SalirPaginaTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public SalirPaginaTests(WebApplicationFactory<Program> fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task PostSinAntiforgeryRedirigeAlLogin()
    {
        var cliente = _fabrica.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var respuesta = await cliente.PostAsync("/Cuenta/Salir", null);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/Cuenta/Login", respuesta.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task GetRedirigeAlLogin()
    {
        var cliente = _fabrica.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var respuesta = await cliente.GetAsync("/Cuenta/Salir");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/Cuenta/Login", respuesta.Headers.Location?.OriginalString);
    }
}