using Microsoft.Extensions.DependencyInjection;
using SGH.Application.Censo.Interfaces;
using SGH.Application.Censo.UseCases;
using SGH.Application.Cuenta.Interfaces;
using SGH.Application.Cuenta.UseCases;
using SGH.Application.Reserva.Interfaces;
using SGH.Application.Reserva.UseCases;

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
        services.AddScoped<IRegistrarMovimientoReserva, RegistrarMovimientoReserva>();
        services.AddScoped<IBuscarCensos, BuscarCensos>();
        services.AddScoped<IObtenerCatalogoCenso, ObtenerCatalogoCenso>();
        services.AddScoped<IGuardarCensos, GuardarCensos>();
        return services;
    }
}
