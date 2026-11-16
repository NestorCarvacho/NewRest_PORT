using SGH.Application.Solicitud.Interfaces;
using SGH.Application.Transversal;
using SGH.Web.Solicitud;

namespace SGH.Web.Transversal;

public static class ServiciosTransversalesWebExtensions
{
    public static IServiceCollection AddServiciosTransversalesWeb(this IServiceCollection services)
    {
        services.AddScoped<IRenderizadorPlantillaCorreo, RenderizadorPlantillaCorreoRazor>();
        services.AddScoped<INotificadorSolicitud, NotificadorSolicitudCorreo>();
        return services;
    }
}