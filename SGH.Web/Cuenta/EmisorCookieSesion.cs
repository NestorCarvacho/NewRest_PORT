using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SGH.Application.Cuenta.DTOs;

namespace SGH.Web.Cuenta;

public static class ClaimsSesion
{
    public const string Rut = "rut";
    public const string PerfilId = "perfilId";
    public const string CampamentoId = "campamentoId";
    public const string EmpresaMandanteId = "empresaMandanteId";
    public const string AsignacionPerfilId = "asignacionPerfilId";
    public const string Sistema = "sistema";
}

public sealed class EmisorCookieSesion
{
    public Task EmitirAsync(HttpContext http, UsuarioSesionDto sesion)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sesion.UsuarioId.ToString()),
            new(ClaimTypes.Name, sesion.NombreParaMostrar),
            new(ClaimTypes.Role, sesion.Rol),
            new(ClaimsSesion.Rut, sesion.Rut),
            new(ClaimsSesion.PerfilId, sesion.PerfilId.ToString()),
            new(ClaimsSesion.CampamentoId, sesion.CampamentoId.ToString()),
            new(ClaimsSesion.EmpresaMandanteId, sesion.EmpresaMandanteId.ToString()),
            new(ClaimsSesion.AsignacionPerfilId, sesion.AsignacionPerfilId.ToString()),
            new(ClaimsSesion.Sistema, sesion.Sistema)
        };

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identidad);
        http.User = principal;
        var propiedades = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(sesion.MinutosVigencia)
        };

        return http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            propiedades);
    }
}
