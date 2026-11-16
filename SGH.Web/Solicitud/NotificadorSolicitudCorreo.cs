using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.Interfaces;
using SGH.Application.Transversal;
using SGH.Web.Correo;

namespace SGH.Web.Solicitud;

public sealed class NotificadorSolicitudCorreo(
    IEnviadorCorreo enviador,
    IRenderizadorPlantillaCorreo renderizador) : INotificadorSolicitud
{
    public async Task EnviarReservasGeneradasAsync(
        SolicitudDetalleDto solicitud,
        IReadOnlyList<PasajeroSolicitudDto> pasajeros,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(solicitud.EmailContactoCampamento))
            throw new InvalidOperationException("La solicitud no tiene un correo de contacto.");

        var modelo = new CorreoReservasGeneradasVm { Solicitud = solicitud, Pasajeros = pasajeros };
        var html = await renderizador.RenderizarHtmlAsync(PlantillasCorreo.ReservasGeneradasSolicitud, modelo, cancellationToken);
        var mensaje = new CorreoMensaje
        {
            Destinatario = solicitud.EmailContactoCampamento,
            Asunto = $"Notificación de solicitud de alojamiento N° {solicitud.Id}: reservas generadas",
            CuerpoHtml = html,
            CuerpoTexto = TextoPlano(solicitud, pasajeros)
        };
        await enviador.EnviarAsync(mensaje, PlantillasCorreo.SeccionSmtpSolicitudes, cancellationToken);
    }

    private static string TextoPlano(SolicitudDetalleDto solicitud, IReadOnlyList<PasajeroSolicitudDto> pasajeros)
    {
        var lineas = new List<string>
        {
            $"Se generaron reservas para la solicitud N° {solicitud.Id}.",
            $"Campamento: {solicitud.CampamentoNombre}",
            $"Contacto del campamento: {solicitud.CampamentoEmail}",
            string.Empty,
            "Pasajeros:"
        };
        foreach (var pasajero in pasajeros)
            lineas.Add($"- {pasajero.NombrePasajero ?? "Cupo sin identificar"} | Habitación: {pasajero.Habitacion ?? "Sin asignar"}");
        return string.Join(Environment.NewLine, lineas);
    }
}