using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Interfaces;
using SGH.Web.Cuenta;

namespace SGH.Web.Pages.Cuenta;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IPreautenticarUsuario _preautenticar;
    private readonly ICompletarSesion _completar;
    private readonly EmisorCookieSesion _emisor;
    private readonly IWebHostEnvironment _ambiente;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        IPreautenticarUsuario preautenticar,
        ICompletarSesion completar,
        EmisorCookieSesion emisor,
        IWebHostEnvironment ambiente,
        ILogger<LoginModel> logger)
    {
        _preautenticar = preautenticar;
        _completar = completar;
        _emisor = emisor;
        _ambiente = ambiente;
        _logger = logger;
    }

    [BindProperty]
    public string Rut { get; set; } = string.Empty;

    [BindProperty]
    public string Contrasena { get; set; } = string.Empty;

    [BindProperty]
    public int AsignacionPerfilId { get; set; }

    [BindProperty]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public string Paso { get; set; } = "credenciales";

    public IReadOnlyList<OpcionPerfilVista> Perfiles { get; private set; } = [];

    public IActionResult OnGet(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirigir(returnUrl);

        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (Paso == "perfil")
            return await CompletarAsync(cancellationToken);

        return await PreautenticarAsync(cancellationToken);
    }

    private async Task<IActionResult> PreautenticarAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Contrasena))
        {
            ModelState.AddModelError(nameof(Contrasena), "Debe ingresar una contraseña");
            return Page();
        }

        try
        {
            var resultado = await _preautenticar.EjecutarAsync(
                new SolicitudPreautenticacion(Rut, Contrasena),
                cancellationToken);

            if (resultado is ResultadoPreautenticacion.Rechazado rechazado)
            {
                ModelState.AddModelError(string.Empty, rechazado.Mensaje);
                return Page();
            }

            var listo = (ResultadoPreautenticacion.Listo)resultado;
            var perfiles = AVista(listo.Perfiles);
            Paso = "perfil";
            Perfiles = perfiles;
            AsignacionPerfilId = 0;
            IngresoPendienteAlmacen.Guardar(TempData, new IngresoPendiente
            {
                Rut = listo.Rut,
                ExpiraUtc = DateTime.UtcNow.AddMinutes(5),
                EsIntegracion = listo.EsIntegracion,
                Perfiles = perfiles
            });
            return Page();
        }
        catch (Exception ex)
        {
            InformarFallo(ex);
            return Page();
        }
    }

    private async Task<IActionResult> CompletarAsync(CancellationToken cancellationToken)
    {
        var pendiente = IngresoPendienteAlmacen.Leer(TempData);
        if (pendiente is null)
        {
            Paso = "credenciales";
            ModelState.AddModelError(string.Empty, "El ingreso pendiente venció. Vuelva a identificarse.");
            return Page();
        }

        Perfiles = pendiente.Perfiles;
        Paso = "perfil";

        try
        {
            return await EmitirAsync(pendiente.Rut, Contrasena, AsignacionPerfilId, cancellationToken);
        }
        catch (Exception ex)
        {
            InformarFallo(ex);
            return Page();
        }
    }

    private async Task<IActionResult> EmitirAsync(string rut, string contrasena, int asignacionPerfilId, CancellationToken cancellationToken)
    {
        var resultado = await _completar.EjecutarAsync(new SolicitudCompletarSesion(rut, contrasena, asignacionPerfilId), cancellationToken);
        if (resultado is ResultadoAutenticacion.Rechazado rechazado)
        {
            ModelState.AddModelError(string.Empty, rechazado.Mensaje);
            return Page();
        }

        IngresoPendienteAlmacen.Descartar(TempData);
        await _emisor.EmitirAsync(HttpContext, ((ResultadoAutenticacion.Aceptado)resultado).Sesion);
        return Redirigir(ReturnUrl);
    }

    private IActionResult Redirigir(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToPage("/Index");
    }

    private static List<OpcionPerfilVista> AVista(IEnumerable<OpcionPerfilDto> perfiles) =>
        perfiles.Select(perfil => new OpcionPerfilVista { Id = perfil.Id, Etiqueta = perfil.Etiqueta }).ToList();

    private void InformarFallo(Exception ex)
    {
        _logger.LogError(ex, "Fallo el inicio de sesión.");
        ModelState.AddModelError(string.Empty, _ambiente.IsDevelopment()
            ? ex.Message
            : "No se pudo completar el inicio de sesión. Intente nuevamente.");
    }
}
