using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Interfaces;
using SGH.Web.Cuenta;

namespace SGH.Web.Pages.Cuenta;

[AllowAnonymous]
[EnableRateLimiting("ingreso")]
public class CambiarClaveModel : PageModel
{
    private readonly ICambiarContrasena _cambiar;
    private readonly IWebHostEnvironment _ambiente;
    private readonly ILogger<CambiarClaveModel> _logger;

    public CambiarClaveModel(ICambiarContrasena cambiar, IWebHostEnvironment ambiente, ILogger<CambiarClaveModel> logger)
    {
        _cambiar = cambiar;
        _ambiente = ambiente;
        _logger = logger;
    }

    [BindProperty]
    public string Rut { get; set; } = string.Empty;

    [BindProperty]
    public string ContrasenaActual { get; set; } = string.Empty;

    [BindProperty]
    public string ContrasenaNueva { get; set; } = string.Empty;

    [BindProperty]
    public string Confirmacion { get; set; } = string.Empty;

    public string? Mensaje { get; private set; }

    public bool Actualizada { get; private set; }

    public void OnGet(string? rut)
    {
        if (!string.IsNullOrWhiteSpace(rut))
            Rut = rut;

        if (TempData["MensajeClave"] is string mensaje)
            Mensaje = mensaje;

        LimiteIngreso.PublicarEspera(this);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _cambiar.EjecutarAsync(
                new SolicitudCambioContrasena(Rut, ContrasenaActual, ContrasenaNueva, Confirmacion),
                cancellationToken);

            if (resultado is ResultadoCambioContrasena.Rechazado rechazado)
            {
                ModelState.AddModelError(string.Empty, rechazado.Mensaje);
                return Page();
            }

            Actualizada = true;
            ViewData["AvisoExito"] = "La clave fue actualizada.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo el cambio de clave.");
            ModelState.AddModelError(string.Empty, _ambiente.IsDevelopment()
                ? ex.Message
                : "No se pudo cambiar la clave. Intente nuevamente.");
            return Page();
        }
    }
}
