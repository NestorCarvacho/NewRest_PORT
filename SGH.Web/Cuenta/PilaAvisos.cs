using System.Text.Json;

namespace SGH.Web.Cuenta;

public static class PilaAvisos
{
    public const string Cookie = "sgh.avisos";
    public const string ClaveLista = "PilaAvisos";
    public const string ClaveSale = "AvisoSale";
    public const string ClaveNuevos = "AvisosNuevos";
    public const int Tope = 4;
    public const int VidaMs = 3000;

    public sealed record AvisoGuardado(string Texto, long En, long Sale);

    public static IReadOnlyList<AvisoGuardado> Leer(HttpRequest solicitud)
    {
        if (!solicitud.Cookies.TryGetValue(Cookie, out var crudo) || string.IsNullOrWhiteSpace(crudo))
            return [];

        try
        {
            var lista = JsonSerializer.Deserialize<List<AvisoGuardado>>(crudo) ?? [];
            return Vigentes(lista, Ahora());
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static void Guardar(HttpContext http, IReadOnlyList<AvisoGuardado> actuales, IReadOnlyList<string> nuevos)
    {
        var ahora = Ahora();
        var lista = Vigentes(actuales.ToList(), ahora);
        var agregados = 0;
        string? sale = null;
        foreach (var mensaje in nuevos)
        {
            if (string.IsNullOrWhiteSpace(mensaje) || (lista.Count > 0 && lista[^1].Texto == mensaje))
                continue;

            var saleEn = lista.Count == 0 ? ahora + VidaMs : Math.Max(ahora + VidaMs, lista[^1].Sale + VidaMs);
            lista.Add(new AvisoGuardado(mensaje, ahora, saleEn));
            agregados++;
            if (lista.Count > Tope)
            {
                sale = lista[0].Texto;
                lista.RemoveAt(0);
            }
        }

        http.Response.Cookies.Append(Cookie, JsonSerializer.Serialize(lista), new CookieOptions
        {
            Path = "/Cuenta",
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = http.Request.IsHttps,
            MaxAge = TimeSpan.FromMinutes(10),
            IsEssential = true
        });
        http.Items[ClaveLista] = lista;
        http.Items[ClaveSale] = sale;
        http.Items[ClaveNuevos] = agregados;
    }

    public static void Limpiar(HttpContext http)
    {
        http.Response.Cookies.Delete(Cookie, new CookieOptions { Path = "/Cuenta" });
        http.Items[ClaveLista] = Array.Empty<AvisoGuardado>();
        http.Items[ClaveSale] = null;
        http.Items[ClaveNuevos] = 0;
    }

    public static long EsperaMs(IReadOnlyList<AvisoGuardado> lista, int indice, long ahora) =>
        Math.Max(0, lista[indice].Sale - ahora);

    public static long Ahora() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private static List<AvisoGuardado> Vigentes(List<AvisoGuardado> lista, long ahora)
    {
        while (lista.Count > 0 && lista[0].Sale <= ahora)
            lista.RemoveAt(0);

        return lista;
    }
}
