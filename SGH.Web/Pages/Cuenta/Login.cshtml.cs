using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Interfaces;
using SGH.Domain.Alcance;
using SGH.Domain.Cuenta;
using SGH.Web.Cuenta;

namespace SGH.Web.Pages.Cuenta;

[AllowAnonymous]
[EnableRateLimiting("ingreso")]
public class LoginModel : PageModel
{
    private readonly IPreautenticarUsuario _preautenticar;
    private readonly ICompletarSesion _completar;
    private readonly EmisorCookieSesion _emisor;
    private readonly ClavePendienteAlmacen _claves;
    private readonly IWebHostEnvironment _ambiente;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        IPreautenticarUsuario preautenticar,
        ICompletarSesion completar,
        EmisorCookieSesion emisor,
        ClavePendienteAlmacen claves,
        IWebHostEnvironment ambiente,
        ILogger<LoginModel> logger)
    {
        _preautenticar = preautenticar;
        _completar = completar;
        _emisor = emisor;
        _claves = claves;
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

    public bool MostrarSelectorAsignacion { get; private set; }

    public IActionResult OnGet(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirigir(returnUrl);

        ReturnUrl = returnUrl;
        LimiteIngreso.PublicarEspera(this);
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
            var anterior = IngresoPendienteAlmacen.Leer(TempData);
            if (anterior is not null)
                _claves.Descartar(anterior.ClaveId);

            var expira = DateTime.UtcNow.AddMinutes(5);
            var claveId = Guid.NewGuid().ToString("N");
            _claves.Guardar(claveId, Contrasena, expira);
            var pendiente = new IngresoPendiente
            {
                Rut = listo.Rut,
                ClaveId = claveId,
                LargoClave = Contrasena.Length,
                ExpiraUtc = expira,
                EsIntegracion = listo.EsIntegracion,
                Perfiles = perfiles
            };
            IngresoPendienteAlmacen.Guardar(TempData, pendiente);

            if (!listo.EsIntegracion && listo.Perfiles.Count == 0)
            {
                _claves.Descartar(claveId);
                IngresoPendienteAlmacen.Descartar(TempData);
                ModelState.AddModelError(string.Empty, ReglasAlcance.SinCampamento);
                return Page();
            }

            var campamentoIds = listo.Perfiles.Select(perfil => perfil.CampamentoId);
            var asignacionAutomatica = ReglasCuenta.ResolverAsignacionAutomatica(
                listo.Perfiles.Count,
                campamentoIds,
                listo.Perfiles.Count > 0 ? listo.Perfiles[0].Id : 0);

            if (listo.EsIntegracion || asignacionAutomatica is not null)
            {
                try
                {
                    return await EmitirAsync(listo.Rut, Contrasena, asignacionAutomatica ?? 0, pendiente, cancellationToken);
                }
                catch (Exception ex)
                {
                    InformarFallo(ex);
                    return Page();
                }
            }

            Paso = "perfil";
            ModelState.Remove(nameof(Paso));
            Perfiles = perfiles;
            MostrarSelectorAsignacion = true;

            AsignacionPerfilId = 0;
            Enmascarar(pendiente);
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
        MostrarSelectorAsignacion = true;
        Enmascarar(pendiente);

        var clave = _claves.Leer(pendiente.ClaveId);
        if (clave is null)
        {
            _claves.Descartar(pendiente.ClaveId);
            IngresoPendienteAlmacen.Descartar(TempData);
            Paso = "credenciales";
            Contrasena = string.Empty;
            ModelState.Remove(nameof(Contrasena));
            ModelState.AddModelError(string.Empty, "El ingreso pendiente venció. Vuelva a identificarse.");
            return Page();
        }

        try
        {
            return await EmitirAsync(pendiente.Rut, clave, AsignacionPerfilId, pendiente, cancellationToken);
        }
        catch (Exception ex)
        {
            InformarFallo(ex);
            return Page();
        }
    }

    private async Task<IActionResult> EmitirAsync(
        string rut,
        string contrasena,
        int asignacionPerfilId,
        IngresoPendiente pendiente,
        CancellationToken cancellationToken)
    {
        var resultado = await _completar.EjecutarAsync(new SolicitudCompletarSesion(rut, contrasena, asignacionPerfilId), cancellationToken);
        if (resultado is ResultadoAutenticacion.Rechazado rechazado)
        {
            Enmascarar(pendiente);
            ModelState.AddModelError(string.Empty, rechazado.Mensaje);
            return Page();
        }

        _claves.Descartar(pendiente.ClaveId);
        IngresoPendienteAlmacen.Descartar(TempData);
        await _emisor.EmitirAsync(HttpContext, ((ResultadoAutenticacion.Aceptado)resultado).Sesion);
        return Redirigir(ReturnUrl);
    }

    private void Enmascarar(IngresoPendiente pendiente)
    {
        ModelState.Remove(nameof(Contrasena));
        Contrasena = new string('•', Math.Max(pendiente.LargoClave, 0));
    }

    private IActionResult Redirigir(string? returnUrl)
    {
        PilaAvisos.Limpiar(HttpContext);
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
