using System.Text.Json;
using System.Text.Json.Serialization;

namespace SGH.Application.Mid;

public sealed class FiltroPeticion
{
    public string? Opcion { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Resto { get; set; }
}

public static class LecturaPeticion
{
    public static JsonElement? Campo(FiltroPeticion filtro, string nombre)
    {
        if (filtro.Resto is null || !filtro.Resto.TryGetValue(nombre, out var valor) || valor.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        return valor;
    }

    public static int Entero(FiltroPeticion filtro, string nombre)
    {
        var campo = Campo(filtro, nombre);
        if (campo is null)
            return 0;

        var valor = campo.Value;
        if (valor.ValueKind == JsonValueKind.Number && valor.TryGetInt32(out var numero))
            return numero;

        return valor.ValueKind == JsonValueKind.String && int.TryParse(valor.GetString(), out numero) ? numero : 0;
    }

    public static int? EnteroOpcional(FiltroPeticion filtro, string nombre)
    {
        var campo = Campo(filtro, nombre);
        if (campo is null)
            return null;

        var valor = campo.Value;
        if (valor.ValueKind == JsonValueKind.Number && valor.TryGetInt32(out var numero))
            return numero;

        if (valor.ValueKind == JsonValueKind.String && int.TryParse(valor.GetString(), out numero))
            return numero;

        return null;
    }

    public static string? Texto(FiltroPeticion filtro, string nombre)
    {
        var campo = Campo(filtro, nombre);
        if (campo is null)
            return null;

        return campo.Value.ValueKind == JsonValueKind.String ? campo.Value.GetString() : campo.Value.ToString();
    }

    public static bool Bandera(FiltroPeticion filtro, string nombre)
    {
        var campo = Campo(filtro, nombre);
        if (campo is null)
            return false;

        var valor = campo.Value;
        return valor.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(valor.GetString(), out var bandera) && bandera,
            _ => false
        };
    }

    public static DateTime? Fecha(string? valor) =>
        DateTime.TryParse(valor, out var fecha) ? fecha : null;

    public static DateTime? FechaOpcional(FiltroPeticion filtro, string nombre)
    {
        var texto = Texto(filtro, nombre);
        return string.IsNullOrWhiteSpace(texto) ? null : Fecha(texto);
    }

    public static JsonElement? Lista(FiltroPeticion filtro, string nombre)
    {
        var campo = Campo(filtro, nombre);
        return campo?.ValueKind == JsonValueKind.Array ? campo : null;
    }
}
