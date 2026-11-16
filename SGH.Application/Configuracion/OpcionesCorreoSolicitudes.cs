namespace SGH.Application.Configuracion;

public sealed class OpcionesCorreoSolicitudes
{
    public const string Seccion = CatalogoSecretos.SeccionCorreoSolicitudes;

    public bool Habilitado { get; init; }
    public string? Host { get; init; }
    public int Puerto { get; init; }
    public bool Ssl { get; init; }
    public string? Remitente { get; init; }
    public string? Usuario { get; init; }
    public string? Contrasena { get; init; }

    public static OpcionesCorreoSolicitudes DesdeSeccion(Microsoft.Extensions.Configuration.IConfigurationSection seccion) =>
        new()
        {
            Habilitado = bool.TryParse(seccion["Habilitado"], out var habilitado) && habilitado,
            Host = seccion["Host"],
            Puerto = int.TryParse(seccion["Puerto"], out var puerto) ? puerto : 0,
            Ssl = bool.TryParse(seccion["Ssl"], out var ssl) && ssl,
            Remitente = seccion["Remitente"],
            Usuario = seccion["Usuario"],
            Contrasena = seccion["Contrasena"]
        };
}