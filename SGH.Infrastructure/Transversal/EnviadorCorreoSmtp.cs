using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SGH.Application.Configuracion;
using SGH.Application.Transversal;

namespace SGH.Infrastructure.Transversal;

public sealed class EnviadorCorreoSmtp(
    IConfiguration configuracion,
    IOptions<OpcionesCorreoSolicitudes> correoSolicitudes) : IEnviadorCorreo
{
    public async Task EnviarAsync(CorreoMensaje mensaje, string seccionConfiguracion, CancellationToken cancellationToken)
    {
        var opciones = seccionConfiguracion == OpcionesCorreoSolicitudes.Seccion
            ? correoSolicitudes.Value
            : OpcionesCorreoSolicitudes.DesdeSeccion(configuracion.GetSection(seccionConfiguracion));

        if (!opciones.Habilitado)
            throw new InvalidOperationException("El envío de correo no está habilitado.");
        if (string.IsNullOrWhiteSpace(opciones.Host) || string.IsNullOrWhiteSpace(opciones.Remitente)
            || opciones.Puerto is < 1 or > 65535)
            throw new InvalidOperationException("La configuración SMTP está incompleta.");
        if (string.IsNullOrWhiteSpace(mensaje.Destinatario))
            throw new InvalidOperationException("El mensaje no tiene destinatario.");
        if (string.IsNullOrWhiteSpace(opciones.Usuario) != string.IsNullOrWhiteSpace(opciones.Contrasena))
            throw new InvalidOperationException("La configuración SMTP requiere usuario y contraseña.");

        using var correo = new MailMessage
        {
            From = new MailAddress(opciones.Remitente),
            Subject = mensaje.Asunto,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true,
            Body = mensaje.CuerpoHtml
        };
        correo.To.Add(mensaje.Destinatario);
        if (!string.IsNullOrWhiteSpace(mensaje.CuerpoTexto))
            correo.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mensaje.CuerpoTexto, Encoding.UTF8, "text/plain"));

        using var cliente = new SmtpClient(opciones.Host, opciones.Puerto)
        {
            EnableSsl = opciones.Ssl,
            Timeout = 30000
        };
        if (!string.IsNullOrWhiteSpace(opciones.Usuario))
            cliente.Credentials = new NetworkCredential(opciones.Usuario, opciones.Contrasena);
        await cliente.SendMailAsync(correo, cancellationToken);
    }
}