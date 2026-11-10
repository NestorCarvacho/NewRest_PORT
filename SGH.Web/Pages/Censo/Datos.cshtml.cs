using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Application.Censo.DTOs;
using SGH.Application.Censo.Interfaces;
using SGH.Web.Cuenta;

namespace SGH.Web.Pages.Censo;

public class DatosModel : PageModel
{
    private readonly IBuscarCensos _buscar;
    private readonly IBuscarMisCensos _buscarMisCensos;
    private readonly IObtenerCatalogoCenso _catalogo;
    private readonly IGuardarCensos _guardar;
    private readonly IWebHostEnvironment _ambiente;
    private readonly ILogger<DatosModel> _logger;
    private readonly IConsultarGestionDiaria _gestionDiaria;
    private readonly IGenerarInformeGeneral _informeGeneral;
    private readonly IGenerarInformeOcupabilidadCenso _ocupabilidad;

    public DatosModel(
        IBuscarCensos buscar,
        IBuscarMisCensos buscarMisCensos,
        IObtenerCatalogoCenso catalogo,
        IGuardarCensos guardar,
        IWebHostEnvironment ambiente,
        ILogger<DatosModel> logger,
        IConsultarGestionDiaria gestionDiaria,
        IGenerarInformeGeneral informeGeneral,
        IGenerarInformeOcupabilidadCenso ocupabilidad)
    {
        _buscar = buscar;
        _buscarMisCensos = buscarMisCensos;
        _catalogo = catalogo;
        _guardar = guardar;
        _ambiente = ambiente;
        _logger = logger;
        _gestionDiaria = gestionDiaria;
        _informeGeneral = informeGeneral;
        _ocupabilidad = ocupabilidad;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnGetGestionDiariaAsync(string? fecha, string? turno, CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        if (!DateTime.TryParse(fecha, out var dia) || turno is not ("D" or "N"))
            return RespuestaJson.Mensaje("Debe ingresar una fecha y un turno válido.");
        try
        {
            var resultado = await _gestionDiaria.EjecutarAsync(
                new SGH.Domain.Censo.Interfaces.FiltroGestionDiaria(sesion.EmpresaMandanteId, sesion.CampamentoId, turno, dia.Date), cancellationToken);
            return new JsonResult(new { filas = resultado.Filas });
        }
        catch (Exception ex) { _logger.LogError(ex, "Fallo el informe de gestión diaria."); return RespuestaJson.DeFallo(ex, _ambiente.IsDevelopment()); }
    }

    public async Task<IActionResult> OnGetInformeGeneralAsync(
        string? tipo, int moduloId, int pisoId, int habitacionId, string? desde, string? hasta,
        string? tipoTurno, CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        if (!DateTime.TryParse(desde, out var inicio) || !DateTime.TryParse(hasta, out var fin))
            return RespuestaJson.Mensaje("Debe ingresar un período válido.");
        try
        {
            var resultado = await _informeGeneral.EjecutarAsync(
                new SGH.Domain.Censo.Interfaces.FiltroInformeGeneral(tipo ?? "C", sesion.CampamentoId, moduloId, pisoId, habitacionId,
                    inicio.Date, fin.Date, tipoTurno ?? "T", sesion.EmpresaMandanteId), cancellationToken);
            var xml = CrearExcel(resultado.Filas);
            return File(System.Text.Encoding.UTF8.GetBytes(xml), "application/vnd.ms-excel", "informe-censo.xls");
        }

        catch (Exception ex) { _logger.LogError(ex, "Fallo el informe general de censo."); return RespuestaJson.DeFallo(ex, _ambiente.IsDevelopment()); }
    }

    public async Task<IActionResult> OnGetOcupabilidadAsync(
        string? tipo, string? subtipo, string? nivel, string? desde, string? hasta,
        int mineraId, CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        if (!DateTime.TryParse(desde, out var inicio) || !DateTime.TryParse(hasta, out var fin))
            return RespuestaJson.Mensaje("Debe ingresar un período válido.");
        try
        {
            var resultado = await _ocupabilidad.EjecutarAsync(
                new SGH.Domain.Censo.Interfaces.FiltroOcupabilidadCenso(
                    tipo ?? "C", subtipo ?? string.Empty, nivel ?? "C",
                    inicio.Date, fin.Date, sesion.EmpresaMandanteId, sesion.CampamentoId, mineraId),
                cancellationToken);
            return new JsonResult(new { filas = resultado.Filas });
        }
        catch (Exception ex) { _logger.LogError(ex, "Fallo el informe de ocupabilidad de censo."); return RespuestaJson.DeFallo(ex, _ambiente.IsDevelopment()); }
    }

    private static string CrearExcel(IReadOnlyList<IReadOnlyDictionary<string, object?>> filas)
    {
        static string E(object? value) => System.Security.SecurityElement.Escape(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty) ?? string.Empty;
        var columnas = filas.FirstOrDefault()?.Keys.ToList() ?? [];
        var encabezado = string.Join("", columnas.Select(c => $"<Cell><Data ss:Type=\"String\">{E(c)}</Data></Cell>"));
        var datos = filas.Select(f => "<Row>" + string.Join("", columnas.Select(c => $"<Cell><Data ss:Type=\"String\">{E(f.TryGetValue(c, out var v) ? v : null)}</Data></Cell>")) + "</Row>");
        return "<?xml version=\"1.0\"?><Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"><Worksheet ss:Name=\"Censo\"><Table><Row>" + encabezado + "</Row>" + string.Join("", datos) + "</Table></Worksheet></Workbook>";
    }

    public async Task<IActionResult> OnGetCatalogoAsync(CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        try
        {
            var catalogo = await _catalogo.EjecutarAsync(sesion.CampamentoId, cancellationToken);
            return catalogo switch
            {
                ResultadoCatalogoCenso.Listo listo => new JsonResult(listo.Catalogo),
                ResultadoCatalogoCenso.Rechazado rechazado => RespuestaJson.Mensaje(rechazado.Mensaje),
                _ => RespuestaJson.Mensaje("No se pudo consultar el catálogo.")
            };
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

    public async Task<IActionResult> OnGetMisCensosAsync(
        string? fecha,
        CancellationToken cancellationToken)
    {
        var sesion = LectorSesion.Leer(User);
        if (!DateTime.TryParse(fecha, out var dia))
            return RespuestaJson.Mensaje("Debe ingresar la fecha de censo.");

        try
        {
            var resultado = await _buscarMisCensos.EjecutarAsync(
                new SolicitudBuscarMisCensos(
                    sesion.EmpresaMandanteId,
                    sesion.CampamentoId,
                    dia.Date,
                    sesion.UsuarioId),
                cancellationToken);

            return resultado switch
            {
                ResultadoBusquedaMisCensos.Lista lista => new JsonResult(new { filas = lista.Filas }),
                ResultadoBusquedaMisCensos.Rechazada rechazada => RespuestaJson.Mensaje(rechazada.Mensaje),
                _ => RespuestaJson.Mensaje("No se pudieron consultar mis censos.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo la búsqueda de censos del usuario.");
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
                new SolicitudGuardarCensos(sesion.UsuarioId, sesion.CampamentoId, fecha, filas),
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

    public async Task<IActionResult> OnPostGuardarQrAsync([FromBody] GuardarQrVista guardar, CancellationToken cancellationToken)
    {
        if (guardar is null || !DateTime.TryParse(guardar.Fecha, out var fecha))
            return RespuestaJson.Mensaje("Debe ingresar la fecha de censo.");

        if (!int.TryParse(guardar.HabitacionId, out var habitacionId) || habitacionId <= 0)
            return RespuestaJson.Mensaje("El código QR no contiene una habitación válida.");

        if (!int.TryParse(guardar.Cantidad, out var cantidad) || cantidad < 0)
            return RespuestaJson.Mensaje("La cantidad de camas no es válida.");

        if (guardar.Turno is not ("D" or "N" or "A"))
            return RespuestaJson.Mensaje("Seleccione un turno válido.");

        if (guardar.Turno == "A"
            && (!int.TryParse(guardar.CantidadNoche, out var cantidadNoche) || cantidadNoche < 0))
            return RespuestaJson.Mensaje("La cantidad de camas nocturnas no es válida.");

        var filas = new List<FilaVista>
        {
            new() { Id = string.Empty, HabitacionId = habitacionId, Cantidad = cantidad.ToString(), Turno = guardar.Turno == "A" ? "D" : guardar.Turno }
        };
        if (guardar.Turno == "A")
            filas.Add(new() { Id = string.Empty, HabitacionId = habitacionId, Cantidad = guardar.CantidadNoche ?? "0", Turno = "N" });

        return await OnPostGuardarAsync(new GuardarVista { Fecha = fecha.ToString("O"), Filas = filas }, cancellationToken);
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

    public sealed class GuardarQrVista
    {
        public string? HabitacionId { get; set; }
        public string? Fecha { get; set; }
        public string? Turno { get; set; }
        public string? Cantidad { get; set; }
        public string? CantidadNoche { get; set; }
    }
}
