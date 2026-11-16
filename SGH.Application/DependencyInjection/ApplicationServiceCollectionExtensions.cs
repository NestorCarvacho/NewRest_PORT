using Microsoft.Extensions.DependencyInjection;
using SGH.Application.Censo.Interfaces;
using SGH.Application.Mid;
using SGH.Application.Censo.UseCases;
using SGH.Application.Cuenta.Interfaces;
using SGH.Application.Cuenta.UseCases;
using SGH.Application.Reserva.Interfaces;
using SGH.Application.Reserva.UseCases;
using SGH.Application.Solicitud.Interfaces;
using SGH.Application.Solicitud.UseCases;
using SGH.Application.Navegacion;
using SGH.Application.Navegacion.Interfaces;

namespace SGH.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddAplicacion(this IServiceCollection services)
    {
        services.AddScoped<IVerificacionClaveUsuario, VerificacionClaveUsuario>();
        services.AddScoped<IPreautenticarUsuario, PreautenticarUsuario>();
        services.AddScoped<ICompletarSesion, CompletarSesion>();
        services.AddScoped<ICambiarContrasena, CambiarContrasena>();
        services.AddScoped<IBuscarReservas, BuscarReservas>();
        services.AddScoped<IObtenerCatalogoReserva, ObtenerCatalogoReserva>();
        services.AddScoped<ICrearReserva, CrearReserva>();
        services.AddScoped<IImportarReservas, ImportarReservas>();
        services.AddScoped<IRegistrarMovimientoReserva, RegistrarMovimientoReserva>();
        services.AddScoped<IActualizarReserva, RegistrarMovimientoReserva.ActualizarReserva>();
        services.AddScoped<IGestionSolicitudes, GestionSolicitudes>();
        services.AddScoped<IBuscarCensos, BuscarCensos>();
        services.AddScoped<IBuscarMisCensos, BuscarMisCensos>();
        services.AddScoped<IObtenerCatalogoCenso, ObtenerCatalogoCenso>();
        services.AddScoped<IGuardarCensos, GuardarCensos>();
        services.AddScoped<IConsultarGestionDiaria, ConsultarGestionDiaria>();
        services.AddScoped<IGenerarInformeGeneral, GenerarInformeGeneral>();
        services.AddScoped<IGenerarInformeOcupabilidadCenso, GenerarInformeOcupabilidadCenso>();
        services.AddScoped<IConsultarEstadoPago, ConsultarEstadoPago>();
        services.AddScoped<IConsultarEstadoPagoPantalla, ConsultarEstadoPagoPantalla>();
        services.AddScoped<IConsultarEstadoPagoServicio, ConsultarEstadoPagoServicio>();
        services.AddScoped<IConsultarEstadoPagoServicioPantalla, ConsultarEstadoPagoServicioPantalla>();
        services.AddScoped<MidReservas>();
        services.AddScoped<MidCenso>();
        services.AddScoped<MidSolicitudes>();
        services.AddScoped<MidInformesCenso>();
        services.AddScoped<IServicioMenuPerfil, ServicioMenuPerfil>();
        return services;
    }
}
