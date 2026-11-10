using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.Interfaces;
using SGH.Web.Cuenta;

namespace SGH.Web.Pages.Solicitud;

public sealed class DatosModel(
    IGestionSolicitudes gestion,
    INotificadorSolicitud notificador,
    IPlanillaPasajeros planilla,
    IWebHostEnvironment ambiente,
    ILogger<DatosModel> logger) : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Index");

    public IActionResult OnGetPlantillaPasajeros() => File(planilla.CrearPlantilla(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PlantillaPasajeros.xlsx");

    public async Task<IActionResult> OnPostCargarPasajerosAsync(int id, IFormFile? archivo, CancellationToken cancellationToken)
    {
        if (id <= 0 || archivo is null || archivo.Length == 0 ||
            !string.Equals(Path.GetExtension(archivo.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return RespuestaJson.Mensaje("Seleccione una solicitud y una planilla .xlsx válida.");
        var sesion = LectorSesion.Leer(User);
        try
        {
            await using var contenido = archivo.OpenReadStream();
            var filas = planilla.Leer(contenido);
            var resultados = await gestion.CargarPasajerosAsync(id, sesion.EmpresaMandanteId,
                sesion.CampamentoId, sesion.UsuarioId, filas, cancellationToken);
            return new JsonResult(new { filas = resultados });
        }
        catch (InvalidDataException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number is 51030 or 51043)
        {
            logger.LogWarning(ex, "No se pudo importar pasajeros en la solicitud {SolicitudId}.", id);
            return RespuestaJson.Mensaje(ex.Message, ex.Number == 51030 ? 404 : 400);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la carga masiva de pasajeros de la solicitud {SolicitudId}.", id);
            return RespuestaJson.Mensaje("La carga se interrumpió. Algunas filas pueden haberse procesado; revise los pasajeros antes de volver a intentarlo.", 500);
        }
    }

    public async Task<IActionResult> OnGetCatalogoAsync(int grupoId, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RespuestaJson.Mensaje("Seleccione un grupo válido.");
        var sesion = LectorSesion.Leer(User);
        try
        {
            return new JsonResult(await gestion.CatalogoAsync(
                sesion.EmpresaMandanteId, sesion.CampamentoId, grupoId, cancellationToken));
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la consulta del catálogo de solicitudes.");
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnGetBuscarAsync(
        DateTime? desde, DateTime? hasta, int grupoId, int solicitanteId,
        string? estado, int numero, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !desde.HasValue || !hasta.HasValue)
            return RespuestaJson.Mensaje("Ingrese fechas y filtros válidos.");

        var sesion = LectorSesion.Leer(User);
        try
        {
            var filas = await gestion.BuscarAsync(new FiltroSolicitudesDto(
                sesion.EmpresaMandanteId, sesion.CampamentoId, desde.Value, hasta.Value,
                grupoId, solicitanteId, string.IsNullOrWhiteSpace(estado) ? "0" : estado, numero), cancellationToken);
            return new JsonResult(new { filas });
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la búsqueda de solicitudes.");
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnGetOrganizacionAsync(int unidadId, int divisionId, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RespuestaJson.Mensaje("Seleccione una unidad de negocio y división válidas.");
        var sesion = LectorSesion.Leer(User);
        try
        {
            return new JsonResult(await gestion.OrganizacionAsync(
                sesion.EmpresaMandanteId, sesion.CampamentoId, unidadId, divisionId, cancellationToken));
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number is 51037 or 51038 or 51039)
        {
            logger.LogWarning(ex, "No se pudo consultar la organización de la sesión.");
            return RespuestaJson.Mensaje(ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la consulta de organización de solicitudes.");
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnGetDetalleAsync(int id, CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        try
        {
            var detalle = await gestion.ObtenerAsync(id, sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
            return detalle is null ? RespuestaJson.Mensaje("Solicitud no encontrada.", 404) : new JsonResult(detalle);
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la consulta de la solicitud {SolicitudId}.", id);
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnGetPasajerosAsync(int id, CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        try
        {
            var pasajeros = await gestion.ListarPasajerosAsync(
                id, sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
            return new JsonResult(new { pasajeros });
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number == 51030)
        {
            logger.LogWarning(ex, "Solicitud {SolicitudId} no encontrada en el alcance de la sesión.", id);
            return RespuestaJson.Mensaje("Solicitud no encontrada.", 404);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la consulta de pasajeros de la solicitud {SolicitudId}.", id);
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnGetHabitacionesDisponiblesAsync(int id, CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        try
        {
            var habitaciones = await gestion.HabitacionesDisponiblesAsync(
                id, sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
            return new JsonResult(new { habitaciones });
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number == 51030)
        {
            logger.LogWarning(ex, "Solicitud {SolicitudId} no encontrada en el alcance de la sesión.", id);
            return RespuestaJson.Mensaje("Solicitud no encontrada.", 404);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la consulta de habitaciones de la solicitud {SolicitudId}.", id);
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnPostAgregarCuposAsync([FromBody] CuposVista? vista, CancellationToken cancellationToken)
    {
        if (vista is null || !ModelState.IsValid)
            return RespuestaJson.Mensaje("Indique una solicitud y una cantidad válidas.");
        var sesion = LectorSesion.Leer(User);
        try
        {
            await gestion.AgregarCuposAsync(vista.Id, sesion.EmpresaMandanteId, sesion.CampamentoId,
                sesion.UsuarioId, vista.Cantidad, cancellationToken);
            return new JsonResult(new { listo = true });
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number is 51030 or 51042 or 51043 or 51044)
        {
            logger.LogWarning(ex, "No se pudieron agregar cupos a la solicitud {SolicitudId}.", vista.Id);
            return RespuestaJson.Mensaje(ex.Message, ex.Number == 51030 ? 404 : 400);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló el alta de cupos de la solicitud {SolicitudId}.", vista.Id);
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnPostEliminarPasajeroAsync([FromBody] PasajeroVista? vista, CancellationToken cancellationToken)
    {
        if (vista is null || !ModelState.IsValid)
            return RespuestaJson.Mensaje("Seleccione una solicitud y un pasajero válidos.");
        var sesion = LectorSesion.Leer(User);
        try
        {
            await gestion.EliminarPasajeroAsync(vista.Id, vista.PasajeroId,
                sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
            return new JsonResult(new { listo = true });
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number is 51030 or 51043 or 51045)
        {
            logger.LogWarning(ex, "No se pudo eliminar el pasajero de la solicitud {SolicitudId}.", vista.Id);
            return RespuestaJson.Mensaje(ex.Message, ex.Number is 51030 or 51045 ? 404 : 400);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la eliminación de un pasajero de la solicitud {SolicitudId}.", vista.Id);
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public sealed record CuposVista(int Id, int Cantidad);
    public sealed record PasajeroVista(int Id, int PasajeroId);

    public async Task<IActionResult> OnPostGuardarPasajeroAsync([FromBody] GuardarPasajeroVista? vista, CancellationToken cancellationToken)
    {
        if (vista is null || !ModelState.IsValid)
            return RespuestaJson.Mensaje("Los datos del pasajero no son válidos.");
        var sesion = LectorSesion.Leer(User);
        try
        {
            var id = await gestion.GuardarPasajeroAsync(new GuardarPasajeroSolicitudDto(
                vista.SolicitudId, vista.PasajeroId, vista.UsuarioPasajeroId, sesion.EmpresaMandanteId,
                sesion.CampamentoId, sesion.UsuarioId, vista.FechaCheckIn, vista.FechaCheckOut), cancellationToken);
            return new JsonResult(new { id });
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number is 51030 or 51043 or 51045 or 51046 or 51047 or 51048)
        {
            logger.LogWarning(ex, "No se pudo guardar un pasajero en la solicitud {SolicitudId}.", vista.SolicitudId);
            return RespuestaJson.Mensaje(ex.Message, ex.Number is 51030 or 51045 ? 404 : 400);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló el guardado de un pasajero de la solicitud {SolicitudId}.", vista.SolicitudId);
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public sealed record GuardarPasajeroVista(
        int SolicitudId, int PasajeroId, int UsuarioPasajeroId, DateTime FechaCheckIn, DateTime? FechaCheckOut);

    public async Task<IActionResult> OnPostAsignarHabitacionAsync([FromBody] AsignacionVista? vista, CancellationToken cancellationToken)
    {
        if (vista is null || !ModelState.IsValid)
            return RespuestaJson.Mensaje("Los datos de asignación no son válidos.");
        var sesion = LectorSesion.Leer(User);
        try
        {
            await gestion.AsignarHabitacionAsync(vista.SolicitudId, vista.PasajeroId, vista.HabitacionId,
                sesion.EmpresaMandanteId, sesion.CampamentoId, sesion.UsuarioId, cancellationToken);
            return new JsonResult(new { listo = true });
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number is 51030 or 51043 or 51045 or 51055 or 51056 or 51057)
        {
            logger.LogWarning(ex, "No se pudo asignar habitación en la solicitud {SolicitudId}.", vista.SolicitudId);
            return RespuestaJson.Mensaje(ex.Message, ex.Number is 51030 or 51045 ? 404 : 400);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la asignación de habitación de la solicitud {SolicitudId}.", vista.SolicitudId);
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public sealed record AsignacionVista(int SolicitudId, int PasajeroId, int HabitacionId);

    public async Task<IActionResult> OnPostGenerarReservasAsync([FromBody] AnulacionVista? vista, CancellationToken cancellationToken)
    {
        if (vista is null || !ModelState.IsValid || vista.Id <= 0)
            return RespuestaJson.Mensaje("Seleccione una solicitud válida.");
        var sesion = LectorSesion.Leer(User);
        try
        {
            var id = await gestion.GenerarReservasAsync(vista.Id, sesion.EmpresaMandanteId,
                sesion.CampamentoId, sesion.UsuarioId, cancellationToken);
            var advertenciaCorreo = await NotificarAsync(vista.Id, sesion.EmpresaMandanteId,
                sesion.CampamentoId, cancellationToken);
            return new JsonResult(new { primeraReservaId = id, advertenciaCorreo });
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number is 51030 or 51049 or 51050 or 51051 or 51052 or 51053 or 51054 or 51055 or 51056)
        {
            logger.LogWarning(ex, "No se pudieron generar reservas para la solicitud {SolicitudId}.", vista.Id);
            return RespuestaJson.Mensaje(ex.Message, ex.Number == 51030 ? 404 : 400);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la generación de reservas para la solicitud {SolicitudId}.", vista.Id);
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnPostNotificarReservasAsync([FromBody] AnulacionVista? vista, CancellationToken cancellationToken)
    {
        if (vista is null || !ModelState.IsValid || vista.Id <= 0)
            return RespuestaJson.Mensaje("Seleccione una solicitud válida.");
        var sesion = LectorSesion.Leer(User);
        var error = await NotificarAsync(vista.Id, sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
        return error is null ? new JsonResult(new { listo = true }) : RespuestaJson.Mensaje(error);
    }

    private async Task<string?> NotificarAsync(int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        try
        {
            var solicitud = await gestion.ObtenerAsync(id, empresaMandanteId, campamentoId, cancellationToken);
            if (solicitud is null || solicitud.Estado != "R")
                return "La solicitud no existe en la sesión o no tiene reservas generadas.";
            var pasajeros = await gestion.ListarPasajerosAsync(id, empresaMandanteId, campamentoId, cancellationToken);
            await notificador.EnviarReservasGeneradasAsync(solicitud, pasajeros, cancellationToken);
            return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la notificación de reservas de la solicitud {SolicitudId}.", id);
            return "Las reservas están generadas, pero no se pudo enviar el correo. Revise la configuración y vuelva a intentarlo.";
        }
    }

    public Task<IActionResult> OnPostCrearAsync([FromBody] SolicitudVista? solicitud, CancellationToken cancellationToken) =>
        GuardarAsync(0, solicitud, cancellationToken);

    public Task<IActionResult> OnPostActualizarAsync([FromBody] SolicitudVista? solicitud, CancellationToken cancellationToken) =>
        GuardarAsync(solicitud?.Id ?? 0, solicitud, cancellationToken, actualizacion: true);

    private async Task<IActionResult> GuardarAsync(
        int id, SolicitudVista? vista, CancellationToken cancellationToken, bool actualizacion = false)
    {
        if (vista is null || !ModelState.IsValid || (actualizacion && id <= 0))
            return RespuestaJson.Mensaje("La solicitud no llegó completa o contiene datos inválidos.");

        var sesion = LectorSesion.Leer(User);
        try
        {
            var resultado = await gestion.GuardarAsync(new DatosSolicitudDto(
                id, sesion.EmpresaMandanteId, sesion.CampamentoId, sesion.UsuarioId,
                vista.FechaSolicitud, vista.FechaCheckIn, vista.FechaCheckOut, vista.GrupoId,
                vista.SolicitanteId, vista.ConSistemaTurno, vista.TurnoId, vista.TurnoRotacionId,
                vista.Observacion, vista.Adc, vista.EmailContactoCampamento ?? "",
                vista.NumeroContactoCampamento, vista.UnidadNegocioId, vista.DivisionId,
                vista.DepartamentoId, vista.NumeroContrato, vista.ReservaVisita), cancellationToken);
            return new JsonResult(new { id = resultado });
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number == 51030)
        {
            logger.LogWarning(ex, "Solicitud {SolicitudId} no encontrada en el alcance de la sesión.", id);
            return RespuestaJson.Mensaje("Solicitud no encontrada.", 404);
        }
        catch (SqlException ex) when (ex.Number is 51031 or 51033 or 51034 or 51035 or 51036 or 51038 or 51039 or 51040 or 51041)
        {
            logger.LogWarning(ex, "La solicitud no cumple la configuración del grupo.");
            return RespuestaJson.Mensaje(ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló el guardado de la solicitud {SolicitudId}.", id);
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public async Task<IActionResult> OnPostAnularAsync([FromBody] AnulacionVista? vista, CancellationToken cancellationToken)
    {
        if (vista is null || !ModelState.IsValid)
            return RespuestaJson.Mensaje("Seleccione una solicitud válida.");
        var sesion = LectorSesion.Leer(User);
        try
        {
            await gestion.AnularAsync(vista.Id, sesion.EmpresaMandanteId, sesion.CampamentoId, sesion.UsuarioId, cancellationToken);
            return new JsonResult(new { listo = true });
        }
        catch (ArgumentException ex) { return RespuestaJson.Mensaje(ex.Message); }
        catch (SqlException ex) when (ex.Number == 51030)
        {
            logger.LogWarning(ex, "Solicitud {SolicitudId} no encontrada en el alcance de la sesión.", vista.Id);
            return RespuestaJson.Mensaje("Solicitud no encontrada.", 404);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló la anulación de la solicitud {SolicitudId}.", vista.Id);
            return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment());
        }
    }

    public sealed class SolicitudVista
    {
        public int Id { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public DateTime? FechaCheckIn { get; set; }
        public DateTime? FechaCheckOut { get; set; }
        public int GrupoId { get; set; }
        public int? SolicitanteId { get; set; }
        public bool ConSistemaTurno { get; set; }
        public int? TurnoId { get; set; }
        public int? TurnoRotacionId { get; set; }
        public string? Observacion { get; set; }
        public string? Adc { get; set; }
        public string? EmailContactoCampamento { get; set; }
        public string? NumeroContactoCampamento { get; set; }
        public int? UnidadNegocioId { get; set; }
        public int? DivisionId { get; set; }
        public int? DepartamentoId { get; set; }
        public string? NumeroContrato { get; set; }
        public bool ReservaVisita { get; set; }
    }

    public sealed class AnulacionVista { public int Id { get; set; } }
}
