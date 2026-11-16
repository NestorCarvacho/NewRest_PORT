namespace SGH.Application.Sesion;

/// <summary>
/// Contrato de auditoría HOTELDB: columnas Fecha*/Usuario* en CUD vía SP.
/// El identificador sale siempre de la sesión de servidor, nunca del JSON del cliente.
/// </summary>
public static class AuditoriaPersistencia
{
    public const string MensajeUsuarioInvalido = "La sesión no incluye un usuario válido para registrar la operación.";

    public static int UsuarioDeSesion(DatosSesion sesion)
    {
        if (sesion.UsuarioId <= 0)
            throw new InvalidOperationException(MensajeUsuarioInvalido);

        return sesion.UsuarioId;
    }
}