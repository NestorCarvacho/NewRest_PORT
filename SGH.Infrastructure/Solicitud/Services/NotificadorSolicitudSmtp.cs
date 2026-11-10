using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.Interfaces;

namespace SGH.Infrastructure.Solicitud.Services;

public sealed class NotificadorSolicitudSmtp(IConfiguration configuracion) : INotificadorSolicitud
{
    public async Task EnviarReservasGeneradasAsync(
        SolicitudDetalleDto solicitud, IReadOnlyList<PasajeroSolicitudDto> pasajeros, CancellationToken cancellationToken)
    {
        var seccion = configuracion.GetSection("CorreoSolicitudes");
        var host = seccion["Host"];
        var remitente = seccion["Remitente"];
        var usuario = seccion["Usuario"];
        var contrasena = seccion["Contrasena"];
        if (!bool.TryParse(seccion["Habilitado"], out var habilitado) || !habilitado)
            throw new InvalidOperationException("El correo de solicitudes no está habilitado.");
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(remitente)
            || !int.TryParse(seccion["Puerto"], out var puerto) || puerto is < 1 or > 65535)
            throw new InvalidOperationException("La configuración SMTP de solicitudes está incompleta.");
        if (string.IsNullOrWhiteSpace(solicitud.EmailContactoCampamento))
            throw new InvalidOperationException("La solicitud no tiene un correo de contacto.");
        if (string.IsNullOrWhiteSpace(usuario) != string.IsNullOrWhiteSpace(contrasena))
            throw new InvalidOperationException("La configuración SMTP requiere usuario y contraseña.");
        if (!bool.TryParse(seccion["Ssl"], out var ssl))
            throw new InvalidOperationException("Indique si el servidor SMTP requiere SSL.");

        using var mensaje = new MailMessage
        {
            From = new MailAddress(remitente),
            Subject = $"Notificación de solicitud de alojamiento N° {solicitud.Id}: reservas generadas",
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };
        mensaje.To.Add(solicitud.EmailContactoCampamento);
        var cuerpo = new StringBuilder()
            .AppendLine($"Se generaron reservas para la solicitud N° {solicitud.Id}.")
            .AppendLine($"Campamento: {solicitud.CampamentoNombre}")
            .AppendLine($"Contacto del campamento: {solicitud.CampamentoEmail}")
            .AppendLine()
            .AppendLine("Pasajeros:");
        foreach (var pasajero in pasajeros)
            cuerpo.AppendLine($"- {pasajero.NombrePasajero ?? "Cupo sin identificar"} | Habitación: {pasajero.Habitacion ?? "Sin asignar"}");
        mensaje.Body = cuerpo.ToString();
        using var cliente = new SmtpClient(host, puerto) { EnableSsl = ssl, Timeout = 30000 };
        if (!string.IsNullOrWhiteSpace(usuario))
            cliente.Credentials = new NetworkCredential(usuario, contrasena);
        await cliente.SendMailAsync(mensaje, cancellationToken);
    }
}
