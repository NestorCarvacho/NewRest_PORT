namespace SGH.Web.Cuenta;

/// <summary>
/// Rate limiting por IP en <c>/Cuenta/CambiarClave</c>.
/// El login usa <see cref="RegistroVentanaIngreso"/> solo cuando fallan las credenciales.
/// </summary>
public static class PoliticaLimiteIngreso
{
    public static bool CuentaHaciaLimitePorIp(HttpContext contexto) =>
        HttpMethods.IsPost(contexto.Request.Method)
        && contexto.Request.Path.StartsWithSegments("/Cuenta/CambiarClave");
}