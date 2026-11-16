namespace SGH.Application.Navegacion;

public sealed record EnlaceMenuMapeado(
    string ClaveModulo,
    string? RutaPagina,
    string? Politica,
    bool PermitirPasajero);

public static class MapeoEnlacesMenu
{
    /// <summary>
    /// Rutas SPA (<c>/app/...</c>) por sufijo exacto; las más largas primero.
    /// </summary>
    private static readonly (string SufijoRuta, EnlaceMenuMapeado Mapeo)[] RutasApp =
    [
        ("/app/gestion/solicitudalojamiento", new(MenuClaves.Solicitud, "/Solicitud/Datos", "GestionarReservas", true)),
        ("/app/gestion/solicitud", new(MenuClaves.Solicitud, "/Solicitud/Datos", "GestionarReservas", true)),
        ("/app/gestion/miscensos", new(MenuClaves.CensoQr, null, "GestionarCensos", false)),
        ("/app/gestion/censo", new(MenuClaves.Censo, "/Censo/Datos", "GestionarCensos", false)),
        ("/app/gestion/reserva", new(MenuClaves.Reserva, "/Reserva/Datos", "GestionarReservas", true)),
        ("/app/config/usuario", new(MenuClaves.Usuarios, "/Cuenta/Usuarios", "AdministrarUsuarios", false)),
        ("/app/config/usuarios", new(MenuClaves.Usuarios, "/Cuenta/Usuarios", "AdministrarUsuarios", false)),
        ("/app/gestion/usuarios", new(MenuClaves.Usuarios, "/Cuenta/Usuarios", "AdministrarUsuarios", false)),
    ];

    /// <summary>Rutas API u otros orígenes sin prefijo <c>/app</c>.</summary>
    private static readonly (string Fragmento, EnlaceMenuMapeado Mapeo)[] FragmentosLegado =
    [
        ("solicitudalojamiento", new(MenuClaves.Solicitud, "/Solicitud/Datos", "GestionarReservas", true)),
        ("miscensos", new(MenuClaves.CensoQr, null, "GestionarCensos", false)),
    ];

    public static bool TryMapear(string linkLegado, out EnlaceMenuMapeado mapeo)
    {
        mapeo = default!;
        if (string.IsNullOrWhiteSpace(linkLegado) || linkLegado is "#")
            return false;

        var normalizado = Normalizar(linkLegado);

        foreach (var (sufijo, regla) in RutasApp)
        {
            if (CoincideSufijoRuta(normalizado, sufijo))
            {
                mapeo = regla;
                return true;
            }
        }

        foreach (var (fragmento, regla) in FragmentosLegado)
        {
            if (normalizado.Contains(fragmento, StringComparison.OrdinalIgnoreCase)
                && !ExcluirFragmento(normalizado, fragmento))
            {
                mapeo = regla;
                return true;
            }
        }

        return false;
    }

    public static IReadOnlyCollection<string> ClavesDesdeRutaPagina(string rutaPagina)
    {
        if (string.IsNullOrWhiteSpace(rutaPagina))
            return Array.Empty<string>();

        var comparer = StringComparison.OrdinalIgnoreCase;
        var claves = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (_, regla) in RutasApp)
        {
            if (!string.IsNullOrEmpty(regla.RutaPagina) && rutaPagina.StartsWith(regla.RutaPagina, comparer))
                claves.Add(regla.ClaveModulo);
        }

        foreach (var (_, regla) in FragmentosLegado)
        {
            if (!string.IsNullOrEmpty(regla.RutaPagina) && rutaPagina.StartsWith(regla.RutaPagina, comparer))
                claves.Add(regla.ClaveModulo);
        }

        return claves;
    }

    private static bool ExcluirFragmento(string normalizado, string fragmento)
    {
        if (normalizado.Contains("/informe/", StringComparison.OrdinalIgnoreCase))
            return true;

        if (fragmento.Contains("solicitud", StringComparison.OrdinalIgnoreCase)
            && normalizado.Contains("solicitudempresa", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static bool CoincideSufijoRuta(string normalizado, string sufijo)
    {
        if (!normalizado.EndsWith(sufijo, StringComparison.OrdinalIgnoreCase))
            return false;

        var indice = normalizado.Length - sufijo.Length;
        return indice == 0 || normalizado[indice - 1] == '/';
    }

    private static string Normalizar(string link)
    {
        var sinQuery = link.Split('?', '#')[0];
        return sinQuery
            .Replace('\\', '/')
            .Trim()
            .TrimEnd('/');
    }
}