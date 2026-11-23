using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SGH.Application.Cuenta.DTOs;
using SGH.Domain.Cuenta;

namespace SGH.Web.Cuenta;

public static class ClaimsSesion
{
    public const string Rut = "rut";
    public const string PerfilId = "perfilId";
    public const string CampamentoId = "campamentoId";
    public const string EmpresaMandanteId = "empresaMandanteId";
    public const string EmpresaMandanteNombre = "empresaMandanteNombre";
    public const string AsignacionPerfilId = "asignacionPerfilId";
    public const string Sistema = "sistema";
    public const string Capacidad = "capacidad";
    public const string TipoPerfil = "tipoPerfil";
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
            new(ClaimsSesion.EmpresaMandanteNombre, sesion.EmpresaMandanteNombre),
            new(ClaimsSesion.AsignacionPerfilId, sesion.AsignacionPerfilId.ToString()),
            new(ClaimsSesion.Sistema, sesion.Sistema)
        };

        var esPasajero = MatrizPerfiles.EsPasajero(sesion.PerfilId);
        claims.Add(new Claim(ClaimsSesion.TipoPerfil, esPasajero ? "pasajero" : "operador"));
        if (!esPasajero)
        {
            claims.Add(new Claim(ClaimsSesion.Capacidad, "gestionar-reservas"));
            claims.Add(new Claim(ClaimsSesion.Capacidad, "ejecutar-checkin-checkout"));
            claims.Add(new Claim(ClaimsSesion.Capacidad, "gestionar-censos"));
        }

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identidad);
        http.User = principal;
        var propiedades = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(sesion.MinutosVigencia)
        };

        return http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            propiedades);
    }
}
