using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Application.Reserva.DTOs;
using SGH.Application.Reserva.Interfaces;
using SGH.Web.Cuenta;

namespace SGH.Web.Pages.Reserva;

public class DatosModel : PageModel
{
    private readonly IBuscarReservas _buscar;
    private readonly IObtenerCatalogoReserva _catalogo;
    private readonly ICrearReserva _crear;
    private readonly IRegistrarMovimientoReserva _movimiento;
    private readonly IWebHostEnvironment _ambiente;
    private readonly ILogger<DatosModel> _logger;

    public DatosModel(
        IBuscarReservas buscar,
        IObtenerCatalogoReserva catalogo,
        ICrearReserva crear,
        IRegistrarMovimientoReserva movimiento,
        IWebHostEnvironment ambiente,
        ILogger<DatosModel> logger)
    {
        _buscar = buscar;
        _catalogo = catalogo;
        _crear = crear;
        _movimiento = movimiento;
        _ambiente = ambiente;
        _logger = logger;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnGetCatalogoAsync(CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        try
        {
            var catalogo = await _catalogo.EjecutarAsync(sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
            return catalogo switch
            {
                ResultadoCatalogoReserva.Listo listo => new JsonResult(listo.Catalogo),
                ResultadoCatalogoReserva.Rechazado rechazado => RespuestaJson.Mensaje(rechazado.Mensaje),
                _ => RespuestaJson.Mensaje("No se pudo consultar el catálogo.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo el catálogo de reserva.");
            return RespuestaJson.DeFallo(ex, _ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnGetBuscarAsync(
        int id,
        int areaId,
        string? origen,
        int solicitudId,
        string? estado,
        string? fechaReserva,
        string? fechaLlegada,
        string? fechaSalida,
        string? checkIn,
        string? checkOut,
        int pasajeroId,
        int habitacionId,
        string? tieneAgua,
        CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        try
        {
            var resultado = await _buscar.EjecutarAsync(new SolicitudBuscarReservas(
                sesion.CampamentoId,
                id,
                areaId,
                origen ?? "0",
                solicitudId,
                estado ?? "0",
                Fecha(fechaReserva),
                Fecha(fechaLlegada),
                Fecha(fechaSalida),
                Fecha(checkIn),
                Fecha(checkOut),
                pasajeroId,
                habitacionId,
                string.IsNullOrWhiteSpace(tieneAgua) ? "0" : tieneAgua), cancellationToken);
            return resultado switch
            {
                ResultadoBusquedaReservas.Lista lista => new JsonResult(new { filas = lista.Filas }),
                ResultadoBusquedaReservas.Rechazada rechazada => RespuestaJson.Mensaje(rechazada.Mensaje),
                _ => RespuestaJson.Mensaje("No se pudo consultar las reservas.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo la búsqueda de reservas.");
            return RespuestaJson.DeFallo(ex, _ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnPostCrearAsync([FromBody] AltaVista alta, CancellationToken cancellationToken)
    {
        if (alta is null)
            return RespuestaJson.Mensaje("La reserva no llegó completa.");

        if (!FechaObligatoria(alta.FechaReserva, out var reserva) || !FechaObligatoria(alta.FechaLlegada, out var llegada))
            return RespuestaJson.Mensaje("Ingrese la fecha de reserva y la fecha de llegada.");

        var sesion = LectorSesion.Leer(User);
        try
        {
            var resultado = await _crear.EjecutarAsync(new SolicitudCrearReserva(
                sesion.EmpresaMandanteId,
                sesion.CampamentoId,
                sesion.UsuarioId,
                alta.AreaId,
                alta.TurnoId,
                reserva,
                llegada,
                Fecha(alta.FechaSalida),
                alta.Observacion), cancellationToken);

            return resultado switch
            {
                ResultadoAltaReserva.Creada creada => new JsonResult(new { id = creada.Id }),
                ResultadoAltaReserva.Rechazada rechazada => RespuestaJson.Mensaje(rechazada.Mensaje),
                _ => RespuestaJson.Mensaje("No se pudo crear la reserva.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo el alta de reserva.");
            return RespuestaJson.DeFallo(ex, _ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnPostMovimientoAsync([FromBody] MovimientoVista movimiento, CancellationToken cancellationToken)
    {
        if (movimiento is null)
            return RespuestaJson.Mensaje("La operación no llegó completa.");

        var fecha = FechaHora(movimiento.Fecha);
        if (fecha is null)
            return RespuestaJson.Mensaje("Seleccione la fecha.");

        var sesion = LectorSesion.Leer(User);
        try
        {
            var resultado = await _movimiento.EjecutarAsync(new SolicitudMovimientoReserva(
                movimiento.ReservaId,
                fecha.Value,
                sesion.UsuarioId,
                movimiento.Operacion ?? string.Empty,
                sesion.CampamentoId), cancellationToken);

            return resultado switch
            {
                ResultadoMovimientoReserva.Registrado => new JsonResult(new { listo = true }),
                ResultadoMovimientoReserva.Rechazado rechazado => RespuestaJson.Mensaje(rechazado.Mensaje),
                _ => RespuestaJson.Mensaje("No se pudo registrar el movimiento.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo el check-in o check-out de la reserva {ReservaId}.", movimiento.ReservaId);
            return RespuestaJson.DeFallo(ex, _ambiente.IsDevelopment());
        }
    }

    private static DateTime? Fecha(string? valor) =>
        DateTime.TryParse(valor, out var fecha) ? fecha.Date : null;

    private static bool FechaObligatoria(string? valor, out DateTime fecha)
    {
        if (DateTime.TryParse(valor, out fecha))
        {
            fecha = fecha.Date;
            return true;
        }

        return false;
    }

    private static DateTime? FechaHora(string? valor) =>
        DateTime.TryParse(valor, out var fecha) ? fecha : null;

    public sealed class AltaVista
    {
        public int AreaId { get; set; }
        public int TurnoId { get; set; }
        public string? FechaReserva { get; set; }
        public string? FechaLlegada { get; set; }
        public string? FechaSalida { get; set; }
        public string? Observacion { get; set; }
    }

    public sealed class MovimientoVista
    {
        public int ReservaId { get; set; }
        public string? Fecha { get; set; }
        public string? Operacion { get; set; }
    }
}
