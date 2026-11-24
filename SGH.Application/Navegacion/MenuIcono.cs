using System.Text.RegularExpressions;

namespace SGH.Application.Navegacion;

/// <summary>Valida clases Font Awesome 5 almacenadas en <c>Menus.Icono</c>.</summary>
public static partial class MenuIcono
{
    [GeneratedRegex(@"^(fas|far|fab|fal)\s+fa-[a-z0-9-]+(\s+fa-[a-z0-9-]+)*$", RegexOptions.IgnoreCase)]
    private static partial Regex PatronClaseFa();

    public static string? NormalizarClase(string? icono)
    {
        if (string.IsNullOrWhiteSpace(icono))
            return null;
        var normalizado = icono.Trim();
        return PatronClaseFa().IsMatch(normalizado) ? normalizado : null;
    }
}