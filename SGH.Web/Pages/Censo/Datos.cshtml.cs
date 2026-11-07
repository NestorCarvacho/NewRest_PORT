using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Application.Censo.DTOs;
using SGH.Application.Censo.Interfaces;
using SGH.Web.Cuenta;

namespace SGH.Web.Pages.Censo;

public class DatosModel : PageModel
{
    private readonly IBuscarCensos _buscar;
    private readonly IObtenerCatalogoCenso _catalogo;
    private readonly IGuardarCensos _guardar;
    private readonly IWebHostEnvironment _ambiente;
    private readonly ILogger<DatosModel> _logger;

    public DatosModel(
        IBuscarCensos buscar,
        IObtenerCatalogoCenso catalogo,
        IGuardarCensos guardar,
        IWebHostEnvironment ambiente,
        ILogger<DatosModel> logger)
    {
        _buscar = buscar;
        _catalogo = catalogo;
        _guardar = guardar;
        _ambiente = ambiente;
        _logger = logger;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnGetCatalogoAsync(CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        try
        {
            var catalogo = await _catalogo.EjecutarAsync(sesion.CampamentoId, cancellationToken);
            return new JsonResult(catalogo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo el catálogo de censo.");
            return RespuestaJson.DeFallo(ex, _ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnGetBuscarAsync(
        int moduloId,
        int pisoId,
        int habitacionId,
        string? fecha,
        string? turno,
        CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        try
        {
            var resultado = await _buscar.EjecutarAsync(new SolicitudBuscarCensos(
                sesion.EmpresaMandanteId,
                sesion.CampamentoId,
                moduloId,
                pisoId,
                habitacionId,
                DateTime.TryParse(fecha, out var dia) ? dia.Date : null,
                turno), cancellationToken);

            return resultado switch
            {
                ResultadoBusquedaCenso.Lista lista => new JsonResult(new { filas = lista.Filas }),
                ResultadoBusquedaCenso.Rechazada rechazada => RespuestaJson.Mensaje(rechazada.Mensaje),
                _ => RespuestaJson.Mensaje("No se pudo consultar el censo.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo la búsqueda de censos.");
            return RespuestaJson.DeFallo(ex, _ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnPostGuardarAsync([FromBody] GuardarVista guardar, CancellationToken cancellationToken)
    {
        if (guardar is null || !DateTime.TryParse(guardar.Fecha, out var fecha))
            return RespuestaJson.Mensaje("Debe ingresar la fecha de censo.");

        var sesion = LectorSesion.Leer(User);
        try
        {
            var filas = (guardar.Filas ?? []).Select(fila => new FilaGuardarCensoDto(
                fila.Id,
                fila.HabitacionId,
                fila.Cantidad,
                fila.Turno)).ToList();

            var resultado = await _guardar.EjecutarAsync(
                new SolicitudGuardarCensos(sesion.UsuarioId, fecha, filas),
                cancellationToken);

            return resultado switch
            {
                ResultadoGuardadoCenso.Guardado => new JsonResult(new { listo = true }),
                ResultadoGuardadoCenso.Rechazado rechazado => RespuestaJson.Mensaje(rechazado.Mensaje),
                _ => RespuestaJson.Mensaje("No se pudo guardar el censo.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo el guardado del censo.");
            return RespuestaJson.DeFallo(ex, _ambiente.IsDevelopment());
        }
    }

    public sealed class GuardarVista
    {
        public string? Fecha { get; set; }
        public List<FilaVista>? Filas { get; set; }
    }

    public sealed class FilaVista
    {
        public string? Id { get; set; }
        public int HabitacionId { get; set; }
        public string? Cantidad { get; set; }
        public string? Turno { get; set; }
    }
}
