using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SGH.Application.Mid;

namespace SGH.Web.Pages.Modulos;

/// <summary>
/// Arma un <see cref="FiltroPeticion"/> desde un formulario Razor (mismas claves que el POST JSON al Mid).
/// </summary>
public static class ConversorFormularioMid
{
    private static readonly HashSet<string> CamposIgnorados = new(StringComparer.OrdinalIgnoreCase)
    {
        "__RequestVerificationToken",
        "__Invariant",
        "handler"
    };

    public static FiltroPeticion DesdeFormulario(IFormCollection form)
    {
        var opcion = form["opcion"].FirstOrDefault()?.Trim();
        var filtro = new FiltroPeticion { Opcion = opcion };
        var resto = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in form.Keys)
        {
            if (CamposIgnorados.Contains(key) || string.Equals(key, "opcion", StringComparison.OrdinalIgnoreCase))
                continue;

            var valores = form[key];
            if (valores.Count > 1)
                resto[key] = JsonSerializer.SerializeToElement(valores.Where(v => !string.IsNullOrEmpty(v)).ToArray());
            else
                resto[key] = JsonSerializer.SerializeToElement(valores.ToString());
        }

        filtro.Resto = resto;
        return filtro;
    }
}