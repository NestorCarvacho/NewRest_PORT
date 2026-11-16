namespace SGH.Application.Navegacion;

public sealed record EnlaceMenuMapeado(
    string ClaveModulo,
    string? RutaPagina,
    string? Politica,
    bool PermitirPasajero);

public static class MapeoEnlacesMenu
{
    /// <summary>
    /// Rutas legado SPA (<c>/app/...</c>) evaluadas de más específica a más general.
  /// Una coincidencia por sufijo de ruta evita que <c>informe/censo</c> abra el módulo Censo.
    /// </summary>
    private static readonly (string SufijoRuta, EnlaceMenuMapeado Mapeo)[] RutasApp =
    [
        ("/app/gestion/miscensos", new(MenuClaves.CensoQr, null, "GestionarCensos", false)),
        ("/app/gestion/censo", new(MenuClaves.Censo, "/Censo/Datos", "GestionarCensos", false)),
        ("/app/gestion/solicitud", new(MenuClaves.Solicitud, "/Solicitud/Datos", "GestionarReservas", true)),
        ("/app/gestion/solicitudalojamiento", new(MenuClaves.Solicitud, "/Solicitud/Datos", "GestionarReservas", true)),
        ("/app/gestion/reserva", new(MenuClaves.Reserva, "/Reserva/Datos", "GestionarReservas", true)),
        ("/app/config/usuarios", new(MenuClaves.Usuarios, "/Cuenta/Usuarios", "AdministrarUsuarios", false)),
        ("/app/gestion/usuarios", new(MenuClaves.Usuarios, "/Cuenta/Usuarios", "AdministrarUsuarios", false)),
    ];

    private static readonly (string Fragmento, EnlaceMenuMapeado Mapeo)[] FragmentosLegado =
    [
        ("solicitudalojamiento", new(MenuClaves.Solicitud, "/Solicitud/Datos", "GestionarReservas", true)),
        ("solicitud", new(MenuClaves.Solicitud, "/Solicitud/Datos", "GestionarReservas", true)),
        ("reservas", new(MenuClaves.Reserva, "/Reserva/Datos", "GestionarReservas", true)),
        ("reserva", new(MenuClaves.Reserva, "/Reserva/Datos", "GestionarReservas", true)),
        ("miscensos", new(MenuClaves.CensoQr, null, "GestionarCensos", false)),
        ("usuarios", new(MenuClaves.Usuarios, "/Cuenta/Usuarios", "AdministrarUsuarios", false)),
    ];

    public static bool TryMapear(string linkLegado, out EnlaceMenuMapeado mapeo)
    {
        mapeo = default!;
        if (string.IsNullOrWhiteSpace(linkLegado))
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
                && !EsInformeCenso(normalizado, fragmento))
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

    private static bool EsInformeCenso(string normalizado, string fragmento) =>
        fragmento.Equals("censo", StringComparison.OrdinalIgnoreCase)
        && normalizado.Contains("/informe/", StringComparison.OrdinalIgnoreCase);

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