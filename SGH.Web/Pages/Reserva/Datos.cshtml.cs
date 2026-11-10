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
    private readonly IImportarReservas _importar;
    private readonly IPlanillaReservaExcel _planilla;
    private readonly IRegistrarMovimientoReserva _movimiento;
    private readonly IActualizarReserva _actualizar;
    private readonly IWebHostEnvironment _ambiente;
    private readonly ILogger<DatosModel> _logger;

    public DatosModel(
        IBuscarReservas buscar,
        IObtenerCatalogoReserva catalogo,
        ICrearReserva crear,
        IImportarReservas importar,
        IPlanillaReservaExcel planilla,
        IRegistrarMovimientoReserva movimiento,
        IActualizarReserva actualizar,
        IWebHostEnvironment ambiente,
        ILogger<DatosModel> logger)
    {
        _buscar = buscar;
        _catalogo = catalogo;
        _crear = crear;
        _importar = importar;
        _planilla = planilla;
        _movimiento = movimiento;
        _actualizar = actualizar;
        _ambiente = ambiente;
        _logger = logger;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public IActionResult OnGetPlantilla()
    {
        var contenido = _planilla.CrearPlantilla();
        return File(
            contenido,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "PlantillaReservas.xlsx");
    }

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
        string? fechaReservaDesde,
        string? fechaReservaHasta,
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
                Fecha(fechaReservaDesde),
                Fecha(fechaReservaHasta),
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

        if (!FechaHoraObligatoria(alta.FechaReserva, out var reserva) || !FechaHoraObligatoria(alta.FechaLlegada, out var llegada))
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
                FechaHora(alta.FechaSalida),
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

    public async Task<IActionResult> OnPostImportarAsync(IFormFile? archivo, CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { mensaje = "Seleccione una planilla Excel para importar." });

        if (!string.Equals(Path.GetExtension(archivo.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { mensaje = "El archivo debe tener formato .xlsx." });

        var sesion = LectorSesion.Leer(User);
        try
        {
            await using var contenido = archivo.OpenReadStream();
            var resultado = await _importar.EjecutarAsync(
                contenido,
                sesion.EmpresaMandanteId,
                sesion.CampamentoId,
                sesion.UsuarioId,
                cancellationToken);
            return new JsonResult(resultado);
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Falló la importación de reservas desde Excel.");
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

        public async Task<IActionResult> OnPostActualizarAsync([FromBody] ActualizacionVista actualizacion, CancellationToken cancellationToken)
        {
            if (actualizacion is null)
                return RespuestaJson.Mensaje("La reserva no llegó completa.");

            var sesion = LectorSesion.Leer(User);
            if (!FechaHoraObligatoria(actualizacion.FechaReserva, out var reserva)
                || !FechaHoraObligatoria(actualizacion.FechaLlegada, out var llegada))
                return RespuestaJson.Mensaje("Ingrese las fechas obligatorias.");

            try
            {
                var resultado = await _actualizar.EjecutarAsync(new SolicitudActualizarReserva(
                    actualizacion.ReservaId, sesion.EmpresaMandanteId, sesion.CampamentoId,
                    actualizacion.AreaId, reserva, llegada, FechaHora(actualizacion.FechaSalida),
                    actualizacion.Observacion, actualizacion.TurnoId, actualizacion.PasajeroId,
                    actualizacion.HabitacionId, actualizacion.Estado ?? "P",
                    actualizacion.TieneAgua, sesion.UsuarioId), cancellationToken);
                return resultado switch
                {
                    ResultadoActualizacionReserva.Actualizada => new JsonResult(new { listo = true }),
                    ResultadoActualizacionReserva.Rechazada rechazado => RespuestaJson.Mensaje(rechazado.Mensaje),
                    _ => RespuestaJson.Mensaje("No se pudo actualizar la reserva.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo la actualización de la reserva {ReservaId}.", actualizacion.ReservaId);
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

    private static bool FechaHoraObligatoria(string? valor, out DateTime fecha) =>
        DateTime.TryParse(valor, out fecha);

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

    public sealed class ActualizacionVista
    {
        public int ReservaId { get; set; }
        public int AreaId { get; set; }
        public int TurnoId { get; set; }
        public string? FechaReserva { get; set; }
        public string? FechaLlegada { get; set; }
        public string? FechaSalida { get; set; }
        public string? Observacion { get; set; }
        public int? PasajeroId { get; set; }
        public int? HabitacionId { get; set; }
        public string? Estado { get; set; }
        public bool TieneAgua { get; set; }
    }
}
