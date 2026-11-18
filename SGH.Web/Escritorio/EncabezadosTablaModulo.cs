using System.Text.Json;

namespace SGH.Web.Escritorio;

/// <summary>
/// Encabezados visibles de tablas de módulo (fuente: wwwroot/datos/encabezados-tabla-modulo.json).
/// La estructura HTML vive en Razor; el JS solo rellena filas vía DataTables.
/// </summary>
public static class EncabezadosTablaModulo
{
    private static readonly IReadOnlyDictionary<string, string[]> PorModo;

    static EncabezadosTablaModulo()
    {
        var nombre = Path.Combine("wwwroot", "datos", "encabezados-tabla-modulo.json");
        var candidatos = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), nombre),
            Path.Combine(AppContext.BaseDirectory, nombre),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SGH.Web", nombre))
        };
        var ruta = candidatos.Select(Path.GetFullPath).FirstOrDefault(File.Exists);
        if (ruta is null)
        {
            PorModo = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        var json = File.ReadAllText(ruta);
        PorModo = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json)
                  ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<string> Obtener(string moduloId)
    {
        if (string.IsNullOrWhiteSpace(moduloId)) return Array.Empty<string>();
        return PorModo.TryGetValue(moduloId, out var columnas)
            ? columnas
            : Array.Empty<string>();
    }
}