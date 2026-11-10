namespace SGH.Application.Cuenta.DTOs;

public enum MotivoRechazo
{
    CredencialesInvalidas,
    ClaveCaducada,
    EstadoInvalido,
    FormatoContrasena,
    ClaveReutilizada,
    PerfilNoPermitido,
    CuentaBloqueada
}

public sealed record OpcionPerfilDto(
    int Id,
    int PerfilId,
    string PerfilNombre,
    int CampamentoId,
    string CampamentoNombre,
    int EmpresaMandanteId,
    string EmpresaMandanteNombre,
    string Etiqueta);

public sealed record UsuarioSesionDto(
    int UsuarioId,
    string Rut,
    string NombreParaMostrar,
    int PerfilId,
    int CampamentoId,
    int EmpresaMandanteId,
    string EmpresaMandanteNombre,
    int AsignacionPerfilId,
    string Rol,
    string Sistema,
    bool EsIntegracion,
    int MinutosVigencia);

public sealed record SolicitudPreautenticacion(string Rut, string Contrasena, int LoginInterno = 0);

public abstract record ResultadoPreautenticacion
{
    private ResultadoPreautenticacion()
    {
    }

    public sealed record Listo(string Rut, string Nombre, bool EsIntegracion, IReadOnlyList<OpcionPerfilDto> Perfiles) : ResultadoPreautenticacion;

    public sealed record Rechazado(string Mensaje, MotivoRechazo Motivo) : ResultadoPreautenticacion;
}

public sealed record SolicitudCompletarSesion(string Rut, string Contrasena, int AsignacionPerfilId, int LoginInterno = 0);

public abstract record ResultadoAutenticacion
{
    private ResultadoAutenticacion()
    {
    }

    public sealed record Aceptado(UsuarioSesionDto Sesion) : ResultadoAutenticacion;

    public sealed record Rechazado(string Mensaje, MotivoRechazo Motivo) : ResultadoAutenticacion;
}

public sealed record SolicitudCambioContrasena(string Rut, string ContrasenaActual, string ContrasenaNueva, string Confirmacion);

public abstract record ResultadoCambioContrasena
{
    private ResultadoCambioContrasena()
    {
    }

    public sealed record Actualizada : ResultadoCambioContrasena;

    public sealed record Rechazado(string Mensaje, MotivoRechazo Motivo) : ResultadoCambioContrasena;
}
