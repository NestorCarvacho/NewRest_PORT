using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGH.Application.DependencyInjection;
using SGH.Application.Cuenta.Opciones;
using SGH.Application.Reserva.Interfaces;
using SGH.Domain.Censo.Interfaces;
using SGH.Domain.Cuenta.Interfaces;
using SGH.Domain.Reserva.Interfaces;
using SGH.Infrastructure.Data;
using SGH.Infrastructure.Persistence;
using SGH.Infrastructure.Censo.Commands;
using SGH.Infrastructure.Censo.Queries;
using SGH.Infrastructure.Cuenta.Commands;
using SGH.Infrastructure.Cuenta.Queries;
using SGH.Infrastructure.Cuenta.Security;
using SGH.Infrastructure.Reserva.Commands;
using SGH.Infrastructure.Reserva.Planillas;
using SGH.Infrastructure.Reserva.Queries;
using SGH.Application.Solicitud.Interfaces;
using SGH.Infrastructure.Solicitud.Commands;
using SGH.Infrastructure.Solicitud.Queries;
using SGH.Application.Configuracion;
using SGH.Application.Transversal;
using SGH.Infrastructure.Transversal;
using SGH.Infrastructure.Solicitud.Planillas;
using SGH.Domain.Navegacion.Interfaces;
using SGH.Infrastructure.Navegacion.Queries;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Habitacion.Commands;
using SGH.Infrastructure.Habitacion.Queries;

namespace SGH.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAplicacion();

        services.AddSingleton(new OpcionesSesion
        {
            MinutosPorDefecto = configuration.GetValue("Sesion:MinutosPorDefecto", 60),
            MinutosOperaciones = configuration.GetValue("Sesion:MinutosOperaciones", 480)
        });
        services.AddSingleton(new OpcionesContrasena
        {
            FactorTrabajoBcrypt = configuration.GetValue("Contrasena:FactorTrabajoBcrypt", 11)
        });

        var cadena = configuration.GetConnectionString("Hoteldb");
        AD_Global.UsarCadena(cadena);
        services.AddSingleton<IFabricaConexionSql>(_ => new FabricaConexionSql(cadena));
        services.AddScoped<IConsultaAutenticacion, ConsultaAutenticacion>();
        services.AddScoped<IComandoAutenticacion, ComandoAutenticacion>();
        services.AddSingleton<IVerificadorContrasena, VerificadorContrasena>();
        services.AddScoped<IConsultaReserva, ConsultaReserva>();
        services.AddScoped<IComandoReserva, ComandoReserva>();
        services.AddScoped<IConsultaSolicitudAlojamiento, ConsultaSolicitudAlojamiento>();
        services.AddScoped<IComandoSolicitudAlojamiento, ComandoSolicitudAlojamiento>();
        services.Configure<OpcionesCorreoSolicitudes>(configuration.GetSection(OpcionesCorreoSolicitudes.Seccion));
        services.AddScoped<IEnviadorCorreo, EnviadorCorreoSmtp>();
        services.AddScoped<IGeneradorCodigoQr, GeneradorCodigoQr>();
        services.AddScoped<IGeneradorPdf, GeneradorPdfDocumento>();
        services.AddScoped<IExportadorHojaCalculo, ExportadorHojaCalculo>();
        services.AddScoped<IPlanillaPasajeros, PlanillaPasajerosExcel>();
        services.AddScoped<IPlanillaReservaExcel, PlanillaReservaExcel>();
        services.AddScoped<IConsultaCenso, ConsultaCenso>();
        services.AddScoped<IComandoCenso, ComandoCenso>();
        services.AddScoped<IInformesCenso, InformesCenso>();
        services.AddScoped<IInformesEstadoPago, InformesEstadoPago>();
        services.AddScoped<IConsultaMenuPerfil, ConsultaMenuPerfil>();
        services.AddScoped<IConsultaHabitacion, ConsultaHabitacion>();
        services.AddScoped<IComandoHabitacion, ComandoHabitacion>();
        return services;
    }
}
