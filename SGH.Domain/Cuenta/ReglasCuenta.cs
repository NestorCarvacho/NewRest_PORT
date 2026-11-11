namespace SGH.Domain.Cuenta;

public static class ReglasCuenta
{
    public const string RutPerfilForzado = "767095309";
    public const int PerfilForzado = 11;
    public const string NombreIntegracion = "ideauno";
    public const int PerfilOperacionesA = 4;
    public const int PerfilOperacionesB = 5;
    public const string RolGeneral = "all";
    public const string RolIntegracion = "ideauno";
    public const string Sistema = "SGH";

    public const string CredencialesInvalidas = "Credenciales inválidas";
    public const string ClaveCaducada = "Su Clave ha Caducado";
    public const string ClaveReutilizada = "Contraseña no debe haber sido usada con anterioridad";
    public const string ErrorEstado = "Se ha producido un error al validar el estado del usuario";
    public const string PerfilNoPermitido = "El perfil seleccionado no corresponde al usuario.";
    public const string SeleccionePerfil = "Seleccione Campamento/Perfil";
    public const int IntentosAntesDeBloquear = 5;
    public const int SegundosBloqueo = 600;

    public static string MensajeCuentaBloqueada =>
        $"La cuenta quedó bloqueada por {SegundosBloqueo} segundos.";

    public static string MensajeIntentosRestantes(int restantes) =>
        restantes == 1
            ? "Clave incorrecta. Queda 1 intento."
            : $"Clave incorrecta. Quedan {restantes} intentos.";

    public static bool FuerzaPerfil(string rutNormalizado) =>
        string.Equals(rutNormalizado, RutPerfilForzado, StringComparison.Ordinal);

    public static bool EsIntegracion(string? nombre) =>
        string.Equals(nombre, NombreIntegracion, StringComparison.Ordinal);

    public static bool UsaSesionOperaciones(int perfilId) =>
        perfilId is PerfilOperacionesA or PerfilOperacionesB;

    /// <summary>
    /// Omite la pantalla de selección cuando hay una sola asignación o un único campamento.
    /// Devuelve el id de asignación perfil-campamento a usar.
    /// </summary>
    public static int? ResolverAsignacionAutomatica(
        int cantidadAsignaciones,
        int cantidadCampamentosDistintos,
        int primeraAsignacionId)
    {
        if (cantidadAsignaciones == 0)
            return null;
        if (cantidadAsignaciones == 1)
            return primeraAsignacionId;
        if (cantidadCampamentosDistintos == 1)
            return primeraAsignacionId;
        return null;
    }
}

public static class CodigoEstadoUsuario
{
    public const int Vigente = 0;
    public const int ClaveCaducada = -2;
}

public static class CodigoCambioClave
{
    public const int Exito = 0;
    public const int Reutilizada = -3;
    public const int NoActualizada = -1;
}
