using System.Collections.Frozen;

namespace SGH.Application.Navegacion;

public sealed record EnlaceMenuMapeado(
    string ClaveModulo,
    string RutaPagina,
    string? Politica,
    bool PermitirPasajero);

public static class MapeoEnlacesMenu
{
    private static readonly FrozenDictionary<string, EnlaceMenuMapeado> PorFragmento =
        new Dictionary<string, EnlaceMenuMapeado>(StringComparer.OrdinalIgnoreCase)
        {
            ["solicitud"] = new(MenuClaves.Solicitud, "/Solicitud/Datos", "GestionarReservas", true),
            ["solicitudalojamiento"] = new(MenuClaves.Solicitud, "/Solicitud/Datos", "GestionarReservas", true),
            ["reserva"] = new(MenuClaves.Reserva, "/Reserva/Datos", "GestionarReservas", true),
            ["reservas"] = new(MenuClaves.Reserva, "/Reserva/Datos", "GestionarReservas", true),
            ["censo"] = new(MenuClaves.Censo, "/Censo/Datos", "GestionarCensos", false),
            ["censos"] = new(MenuClaves.Censo, "/Censo/Datos", "GestionarCensos", false),
            ["usuario"] = new(MenuClaves.Usuarios, "/Cuenta/Usuarios", "AdministrarUsuarios", false),
            ["usuarios"] = new(MenuClaves.Usuarios, "/Cuenta/Usuarios", "AdministrarUsuarios", false),
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static bool TryMapear(string linkLegado, out EnlaceMenuMapeado mapeo)
    {
        mapeo = default!;
        if (string.IsNullOrWhiteSpace(linkLegado))
            return false;

        var normalizado = Normalizar(linkLegado);
        foreach (var par in PorFragmento)
        {
            if (normalizado.Contains(par.Key, StringComparison.OrdinalIgnoreCase))
            {
                mapeo = par.Value;
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
        return PorFragmento.Values
            .Where(m => rutaPagina.StartsWith(m.RutaPagina, comparer))
            .Select(m => m.ClaveModulo)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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