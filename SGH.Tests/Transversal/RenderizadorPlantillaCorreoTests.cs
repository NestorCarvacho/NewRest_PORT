using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SGH.Application.Transversal;
using SGH.Application.Solicitud.DTOs;
using SGH.Web.Correo;

namespace SGH.Tests.Transversal;

public class RenderizadorPlantillaCorreoTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public RenderizadorPlantillaCorreoTests(WebApplicationFactory<Program> fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task PlantillaReservasGeneradasIncluyeSolicitudId()
    {
        await using var ambito = _fabrica.Services.CreateAsyncScope();
        var renderizador = ambito.ServiceProvider.GetRequiredService<IRenderizadorPlantillaCorreo>();
        var solicitud = new SolicitudDetalleDto(
            99, new DateTime(2026, 11, 10), null, null, 2, null, 3, null, 4, null,
            false, null, null, null, null, 5, "Campamento Norte", "campo@test", null,
            new DateTime(2026, 11, 10), null, "R", null, 7, 9, null, "contacto@test",
            null, null, null, null, null, null, null, null, false);

        var html = await renderizador.RenderizarHtmlAsync(
            PlantillasCorreo.ReservasGeneradasSolicitud,
            new CorreoReservasGeneradasVm { Solicitud = solicitud, Pasajeros = [] },
            CancellationToken.None);

        Assert.Contains("solicitud N° 99", html, StringComparison.Ordinal);
        Assert.Contains("Campamento Norte", html, StringComparison.Ordinal);
    }
}