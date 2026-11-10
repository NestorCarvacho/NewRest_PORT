using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using SGH.Application.Solicitud.DTOs;
using SGH.Infrastructure.Solicitud.Services;

namespace SGH.Tests.Solicitud;

public class NotificadorSolicitudTests
{
    [Fact]
    public async Task CorreoDeshabilitadoSeInforma()
    {
        var configuracion = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var notificador = new NotificadorSolicitudSmtp(configuracion);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            notificador.EnviarReservasGeneradasAsync(Solicitud(), [], CancellationToken.None));
    }

    [Fact]
    public async Task EnviaAlServidorConfigurado()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var puerto = ((IPEndPoint)listener.LocalEndpoint).Port;
            var configuracion = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorreoSolicitudes:Habilitado"] = "true",
                ["CorreoSolicitudes:Host"] = "127.0.0.1",
                ["CorreoSolicitudes:Puerto"] = puerto.ToString(),
                ["CorreoSolicitudes:Remitente"] = "remitente@example.test",
                ["CorreoSolicitudes:Ssl"] = "false"
            }).Build();
            var servidor = AtenderSmtpAsync(listener);
            await new NotificadorSolicitudSmtp(configuracion).EnviarReservasGeneradasAsync(Solicitud(), [], CancellationToken.None);
            var comandos = await servidor.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Contains(comandos, linea => linea.Contains("contacto@example.test", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            listener.Stop();
        }
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
                while (await lector.ReadLineAsync() is { } contenido && contenido != ".") { }
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
