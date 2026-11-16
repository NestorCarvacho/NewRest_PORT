using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGH.Application.Solicitud.DTOs;
using SGH.Infrastructure.Transversal;
using SGH.Application.Solicitud.Interfaces;

namespace SGH.Tests.Solicitud;

public class NotificadorSolicitudTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public NotificadorSolicitudTests(WebApplicationFactory<Program> fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task CorreoDeshabilitadoSeInforma()
    {
        var fabrica = _fabrica.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>());
            });
        });
        await using var ambito = fabrica.Services.CreateAsyncScope();
        var notificador = ambito.ServiceProvider.GetRequiredService<INotificadorSolicitud>();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            notificador.EnviarReservasGeneradasAsync(Solicitud(), [], CancellationToken.None));
    }

    [Fact]
    public async Task EnviaPlantillaHtmlAlServidorConfigurado()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var puerto = ((IPEndPoint)listener.LocalEndpoint).Port;
            var fabrica = _fabrica.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["CorreoSolicitudes:Habilitado"] = "true",
                        ["CorreoSolicitudes:Host"] = "127.0.0.1",
                        ["CorreoSolicitudes:Puerto"] = puerto.ToString(),
                        ["CorreoSolicitudes:Remitente"] = "remitente@example.test",
                        ["CorreoSolicitudes:Ssl"] = "false"
                    });
                });
            });
            var servidor = AtenderSmtpAsync(listener);
            await using var ambito = fabrica.Services.CreateAsyncScope();
            var notificador = ambito.ServiceProvider.GetRequiredService<INotificadorSolicitud>();
            await notificador.EnviarReservasGeneradasAsync(Solicitud(), [], CancellationToken.None);
            var comandos = await servidor.WaitAsync(TimeSpan.FromSeconds(10));
            var cuerpo = string.Join('\n', comandos);
            Assert.Contains("contacto@example.test", cuerpo, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public async Task EnviadorCorreoSmtpRechazaSinDestinatario()
    {
        var configuracion = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["CorreoSolicitudes:Habilitado"] = "true",
            ["CorreoSolicitudes:Host"] = "127.0.0.1",
            ["CorreoSolicitudes:Puerto"] = "25",
            ["CorreoSolicitudes:Remitente"] = "a@test",
            ["CorreoSolicitudes:Ssl"] = "false"
        }).Build();
        var enviador = new EnviadorCorreoSmtp(configuracion);
        await Assert.ThrowsAsync<InvalidOperationException>(() => enviador.EnviarAsync(
            new Application.Transversal.CorreoMensaje
            {
                Destinatario = "",
                Asunto = "x",
                CuerpoHtml = "<p>x</p>"
            },
            "CorreoSolicitudes",
            CancellationToken.None));
    }

    private static async Task<List<string>> AtenderSmtpAsync(TcpListener listener)
    {
        using var cliente = await listener.AcceptTcpClientAsync();
        using var stream = cliente.GetStream();
        using var lector = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        using var escritor = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true) { NewLine = "\r\n", AutoFlush = true };
        var comandos = new List<string>();
        await escritor.WriteLineAsync("220 localhost SMTP");
        while (await lector.ReadLineAsync() is { } linea)
        {
            comandos.Add(linea);
            if (linea.StartsWith("DATA", StringComparison.OrdinalIgnoreCase))
            {
                await escritor.WriteLineAsync("354 Start mail input");
                while (await lector.ReadLineAsync() is { } contenido && contenido != ".")
                    comandos.Add(contenido);
                await escritor.WriteLineAsync("250 OK");
            }
            else if (linea.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase))
            {
                await escritor.WriteLineAsync("221 Bye");
                break;
            }
            else
                await escritor.WriteLineAsync("250 OK");
        }
        return comandos;
    }

    private static SolicitudDetalleDto Solicitud() => new(
        1, new DateTime(2026, 11, 10), null, null, 2, null, 3, null, 4, null,
        false, null, null, null, null, 5, "Campamento", "campamento@example.test", null,
        new DateTime(2026, 11, 10), null, "R", null, 7, 9, null, "contacto@example.test",
        null, null, null, null, null, null, null, null, false);
}