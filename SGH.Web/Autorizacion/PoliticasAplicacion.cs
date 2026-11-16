using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SGH.Domain.Cuenta;

namespace SGH.Web.Autorizacion;

public static class PoliticasAplicacion
{
    public const string AdministrarUsuarios = "AdministrarUsuarios";
    public const string GestionarReservas = "GestionarReservas";
    public const string EjecutarCheckInOut = "EjecutarCheckInOut";
    public const string GestionarCensos = "GestionarCensos";
    public const string GestionarHabitaciones = "GestionarHabitaciones";
}

public sealed class PerfilAutorizacionRequirement : IAuthorizationRequirement
{
    public PerfilAutorizacionRequirement(bool permitePasajero)
    {
        PermitePasajero = permitePasajero;
    }

    public bool PermitePasajero { get; }
}

public sealed class PerfilAutorizacionHandler : AuthorizationHandler<PerfilAutorizacionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PerfilAutorizacionRequirement requirement)
    {
        var claim = context.User.FindFirst("perfilId")?.Value;
        if (!int.TryParse(claim, out var perfilId))
            return Task.CompletedTask;

        if (requirement.PermitePasajero || !MatrizPerfiles.EsPasajero(perfilId))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
