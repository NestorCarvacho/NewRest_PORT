using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SGH.Application.Navegacion.Interfaces;
using SGH.Web.Cuenta;

namespace SGH.Web.ViewComponents;

public sealed class MenuPerfilViewComponent : ViewComponent
{
    private readonly IServicioMenuPerfil _menu;

    public MenuPerfilViewComponent(IServicioMenuPerfil menu)
    {
        _menu = menu;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Content(string.Empty);

        var perfilTexto = UserClaimsPrincipal.FindFirstValue(ClaimsSesion.PerfilId);
        if (!int.TryParse(perfilTexto, out var perfilId))
            return Content(string.Empty);

        var modelo = await _menu.ObtenerMenuAsync(perfilId, HttpContext.RequestAborted);
        return View(modelo);
    }
}