using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SGH.Web.Cuenta;

public static class LimiteIngreso
{
    public const string Mensaje = "Demasiados intentos. Espere unos minutos.";
    public const string ClaveAviso = "AvisoIngreso";
    public const string CookieEspera = "sgh.espera";

    public static int SegundosRestantes(HttpRequest solicitud)
    {
        if (!solicitud.Cookies.TryGetValue(CookieEspera, out var crudo) || !long.TryParse(crudo, out var hasta))
            return 0;

        var resta = hasta - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var tope = (int)Math.Ceiling(RegistroVentanaIngreso.Duracion.TotalSeconds);
        if (resta > tope)
            return 0;

        return resta > 0 ? (int)resta : 0;
    }

    public static void PublicarEspera(PageModel pagina)
    {
        var segundos = SegundosRestantes(pagina.Request);
        pagina.ViewData["SegundosEspera"] = segundos;
        if (pagina.TempData[ClaveAviso] is string aviso)
            pagina.ModelState.AddModelError(string.Empty, aviso);
        else if (segundos > 0)
            pagina.ModelState.AddModelError(string.Empty, Mensaje);
    }

    public static void MarcarEspera(HttpResponse respuesta, TimeSpan espera)
    {
        var segundos = (int)Math.Ceiling(espera.TotalSeconds);
        if (segundos < 1)
            segundos = 1;
        if (segundos > 600)
            segundos = 600;

        var hasta = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + segundos;
        respuesta.Cookies.Append(CookieEspera, hasta.ToString(), new CookieOptions
        {
            Path = "/Cuenta",
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = respuesta.HttpContext.Request.IsHttps,
            MaxAge = TimeSpan.FromSeconds(segundos),
            IsEssential = true
        });
    }
}
