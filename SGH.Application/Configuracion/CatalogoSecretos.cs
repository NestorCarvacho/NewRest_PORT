namespace SGH.Application.Configuracion;

/// <summary>
/// Claves de configuración y variables de entorno equivalentes (convención ASP.NET Core: <c>__</c>).
/// Los valores nunca se versionan; usar User Secrets, <c>secrets.json</c> o variables de entorno.
/// </summary>
public static class CatalogoSecretos
{
    public const string CadenaConexionHoteldb = "ConnectionStrings:Hoteldb";
    public const string VariableEntornoCadenaHoteldb = "ConnectionStrings__Hoteldb";

    public const string SeccionCorreoSolicitudes = "CorreoSolicitudes";
    public const string CorreoSolicitudesUsuario = "CorreoSolicitudes:Usuario";
    public const string CorreoSolicitudesContrasena = "CorreoSolicitudes:Contrasena";
    public const string VariableEntornoCorreoContrasena = "CorreoSolicitudes__Contrasena";

    public const string ArchivoSecretosLocal = "secrets.json";
    public const string VariableDirectorioSecretos = "SGH_SECRETS_DIRECTORY";
}