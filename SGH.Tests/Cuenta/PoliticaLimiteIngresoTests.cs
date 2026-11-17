using Microsoft.AspNetCore.Http;
using SGH.Web.Cuenta;

namespace SGH.Tests.Cuenta;

public class PoliticaLimiteIngresoTests
{
    [Fact]
    public void LoginNoUsaRateLimiterPorIp()
    {
        var contexto = ContextoPost("/Cuenta/Login", [new KeyValuePair<string, string>("Paso", "credenciales")]);
        Assert.False(PoliticaLimiteIngreso.CuentaHaciaLimitePorIp(contexto));
    }

    [Fact]
    public void CambiarClaveSiUsaRateLimiterPorIp()
    {
        var contexto = ContextoPost("/Cuenta/CambiarClave", []);
        Assert.True(PoliticaLimiteIngreso.CuentaHaciaLimitePorIp(contexto));
    }

    private static DefaultHttpContext ContextoPost(string ruta, IEnumerable<KeyValuePair<string, string>> campos)
    {
        var contexto = new DefaultHttpContext
        {
            Request =
            {
                Method = HttpMethods.Post,
                Path = ruta,
                ContentType = "application/x-www-form-urlencoded"
            }
        };
        contexto.Request.Form = new FormCollection(campos.ToDictionary(c => c.Key, c => new Microsoft.Extensions.Primitives.StringValues(c.Value)));
        return contexto;
    }
}