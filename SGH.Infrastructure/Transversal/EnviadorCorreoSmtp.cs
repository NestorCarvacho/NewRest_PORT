using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using SGH.Application.Transversal;

namespace SGH.Infrastructure.Transversal;

public sealed class EnviadorCorreoSmtp(IConfiguration configuracion) : IEnviadorCorreo
{
    public async Task EnviarAsync(CorreoMensaje mensaje, string seccionConfiguracion, CancellationToken cancellationToken)
    {
        var seccion = configuracion.GetSection(seccionConfiguracion);
        var host = seccion["Host"];
        var remitente = seccion["Remitente"];
        var usuario = seccion["Usuario"];
        var contrasena = seccion["Contrasena"];
        if (!bool.TryParse(seccion["Habilitado"], out var habilitado) || !habilitado)
            throw new InvalidOperationException("El envío de correo no está habilitado.");
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(remitente)
            || !int.TryParse(seccion["Puerto"], out var puerto) || puerto is < 1 or > 65535)
            throw new InvalidOperationException("La configuración SMTP está incompleta.");
        if (string.IsNullOrWhiteSpace(mensaje.Destinatario))
            throw new InvalidOperationException("El mensaje no tiene destinatario.");
        if (string.IsNullOrWhiteSpace(usuario) != string.IsNullOrWhiteSpace(contrasena))
            throw new InvalidOperationException("La configuración SMTP requiere usuario y contraseña.");
        if (!bool.TryParse(seccion["Ssl"], out var ssl))
            throw new InvalidOperationException("Indique si el servidor SMTP requiere SSL.");

        using var correo = new MailMessage
        {
            From = new MailAddress(remitente),
            Subject = mensaje.Asunto,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true,
            Body = mensaje.CuerpoHtml
        };
        correo.To.Add(mensaje.Destinatario);
        if (!string.IsNullOrWhiteSpace(mensaje.CuerpoTexto))
            correo.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mensaje.CuerpoTexto, Encoding.UTF8, "text/plain"));

        using var cliente = new SmtpClient(host, puerto) { EnableSsl = ssl, Timeout = 30000 };
        if (!string.IsNullOrWhiteSpace(usuario))
            cliente.Credentials = new NetworkCredential(usuario, contrasena);
        await cliente.SendMailAsync(correo, cancellationToken);
    }
}