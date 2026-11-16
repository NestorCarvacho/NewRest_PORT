using SGH.Application.Configuracion;

namespace SGH.Web.Configuracion;

public static class ExtensionesAlmacenSecretos
{
    /// <summary>
    /// Carga secretos por ambiente sin versionarlos: User Secrets (Development),
    /// <c>secrets.json</c> local, directorio montado y variables de entorno (estándar ASP.NET Core).
    /// </summary>
    public static WebApplicationBuilder AgregarAlmacenSecretosPorAmbiente(this WebApplicationBuilder builder)
    {
        var entorno = builder.Environment;

        var rutaSecretos = builder.Configuration["SGH:ArchivoSecretos"] ?? CatalogoSecretos.ArchivoSecretosLocal;
        builder.Configuration.AddJsonFile(
            rutaSecretos,
            optional: true,
            reloadOnChange: false);

        var directorio = builder.Configuration[CatalogoSecretos.VariableDirectorioSecretos]
            ?? Environment.GetEnvironmentVariable(CatalogoSecretos.VariableDirectorioSecretos);
        if (!string.IsNullOrWhiteSpace(directorio) && Directory.Exists(directorio))
            builder.Configuration.AddKeyPerFile(directorio, optional: true);

        if (entorno.IsDevelopment())
            builder.Configuration.AddUserSecrets<Program>(optional: true);

        builder.Configuration.AddEnvironmentVariables(prefix: "SGH_");

        return builder;
    }

    public static void ValidarSecretosObligatorios(this WebApplication app)
    {
        var entorno = app.Environment;
        if (entorno.IsEnvironment("Testing"))
            return;

        var configuracion = app.Configuration;
        var cadena = configuracion.GetConnectionString("Hoteldb");
        if (string.IsNullOrWhiteSpace(cadena))
        {
            if (entorno.IsProduction() || entorno.IsStaging())
            {
                throw new InvalidOperationException(
                    $"Falta la cadena de conexión HOTELDB. Configure {CatalogoSecretos.VariableEntornoCadenaHoteldb} " +
                    $"o User Secrets / {CatalogoSecretos.ArchivoSecretosLocal} ({CatalogoSecretos.CadenaConexionHoteldb}).");
            }

            app.Logger.LogWarning(
                "ConnectionStrings:Hoteldb no configurada. Defina secretos antes de operar con la base (ver Docs/prompt-proyecto/configuracion-secretos.md).");
        }
        else
        {
            app.Logger.LogInformation(
                "Cadena HOTELDB cargada ({Origen})",
                EnmascaramientoSecretos.EnmascararCadenaConexion(cadena));
        }
    }
}