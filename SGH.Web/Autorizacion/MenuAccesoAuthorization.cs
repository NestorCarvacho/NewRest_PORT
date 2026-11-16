using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SGH.Application.Navegacion.Interfaces;
using SGH.Web.Cuenta;

namespace SGH.Web.Autorizacion;

public sealed class MenuAccesoRequirement : IAuthorizationRequirement
{
    public MenuAccesoRequirement(params string[] clavesModulo)
    {
        ClavesModulo = clavesModulo;
    }

    public IReadOnlyList<string> ClavesModulo { get; }
}

public sealed class MenuAccesoAuthorizationHandler : AuthorizationHandler<MenuAccesoRequirement>
{
    private readonly IServicioMenuPerfil _menu;

    public MenuAccesoAuthorizationHandler(IServicioMenuPerfil menu)
    {
        _menu = menu;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MenuAccesoRequirement requirement)
    {
        var perfilTexto = context.User.FindFirstValue(ClaimsSesion.PerfilId);
        if (!int.TryParse(perfilTexto, out var perfilId) || perfilId <= 0)
            return;

        if (requirement.ClavesModulo.Count == 0)
        {
            context.Succeed(requirement);
            return;
        }

        foreach (var clave in requirement.ClavesModulo)
        {
            if (await _menu.TieneAccesoModuloAsync(perfilId, clave, CancellationToken.None))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}