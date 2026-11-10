using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Application.Censo.Interfaces;
using SGH.Domain.Censo.Interfaces;
using SGH.Web.Cuenta;

namespace SGH.Web.Pages.Censo;

public class EstadoPagoModel(
    IConsultarEstadoPago estadoPago,
    IConsultarEstadoPagoPantalla estadoPagoPantalla,
    IConsultarEstadoPagoServicio estadoPagoServicio,
    IConsultarEstadoPagoServicioPantalla estadoPagoServicioPantalla,
    IWebHostEnvironment ambiente,
    ILogger<EstadoPagoModel> logger) : PageModel
{
    public IActionResult OnGet() => Page();

    public Task<IActionResult> OnGetConsultarAsync(string? desde, string? hasta, int mineraId, CancellationToken ct) =>
        ConsultarAsync(desde, hasta, mineraId, false, ct);

    public Task<IActionResult> OnGetDescargarAsync(string? desde, string? hasta, int mineraId, CancellationToken ct) =>
        ConsultarAsync(desde, hasta, mineraId, true, ct);

    public Task<IActionResult> OnGetConsultarServicioAsync(string? desde, string? hasta, int habitacionId, int moduloId, int pisoId, string? servicio, int mineraId, CancellationToken ct) =>
        ConsultarServicioAsync(desde, hasta, habitacionId, moduloId, pisoId, servicio, mineraId, false, ct);

    public Task<IActionResult> OnGetDescargarServicioAsync(string? desde, string? hasta, int habitacionId, int moduloId, int pisoId, string? servicio, int mineraId, CancellationToken ct) =>
        ConsultarServicioAsync(desde, hasta, habitacionId, moduloId, pisoId, servicio, mineraId, true, ct);

    private async Task<IActionResult> ConsultarAsync(string? desde, string? hasta, int minera, bool descargar, CancellationToken ct)
    {
        var sesion = LectorSesion.Leer(User);
        if (!Parsear(desde, hasta, out var inicio, out var fin)) return RespuestaJson.Mensaje("Debe ingresar un período válido.");
        try
        {
            var filtro = new FiltroEstadoPago(sesion.EmpresaMandanteId, sesion.CampamentoId, inicio, fin, minera);
            var resultado = await (descargar ? estadoPago.EjecutarAsync(filtro, ct) : estadoPagoPantalla.EjecutarAsync(filtro, ct));
            return descargar ? Excel(resultado.Filas, "EstadoPago") : new JsonResult(new { filas = resultado.Filas });
        }
        catch (Exception ex) { logger.LogError(ex, "Fallo el informe de estado de pago."); return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment()); }
    }

    private async Task<IActionResult> ConsultarServicioAsync(string? desde, string? hasta, int habitacion, int modulo, int piso, string? servicio, int minera, bool descargar, CancellationToken ct)
    {
        var sesion = LectorSesion.Leer(User);
        if (!Parsear(desde, hasta, out var inicio, out var fin)) return RespuestaJson.Mensaje("Debe ingresar un período válido.");
        try
        {
            var filtro = new FiltroEstadoPagoServicio(sesion.EmpresaMandanteId, sesion.CampamentoId, inicio, fin, habitacion, modulo, piso, servicio ?? "F", minera);
            var resultado = await (descargar ? estadoPagoServicio.EjecutarAsync(filtro, ct) : estadoPagoServicioPantalla.EjecutarAsync(filtro, ct));
            return descargar ? Excel(resultado.Filas, "EstadoPagoServicio") : new JsonResult(new { filas = resultado.Filas });
        }
        catch (Exception ex) { logger.LogError(ex, "Fallo el informe de estado de pago por servicio."); return RespuestaJson.DeFallo(ex, ambiente.IsDevelopment()); }
    }

    private static bool Parsear(string? desde, string? hasta, out DateTime? inicio, out DateTime? fin)
    {
        inicio = DateTime.TryParse(desde, out var d) ? d.Date : null;
        fin = DateTime.TryParse(hasta, out var h) ? h.Date : null;
        return (desde is null or "" || inicio.HasValue)
            && (hasta is null or "" || fin.HasValue);
    }

    private static FileContentResult Excel(IReadOnlyList<IReadOnlyDictionary<string, object?>> filas, string nombre)
    {
        static string E(object? value) => System.Security.SecurityElement.Escape(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty) ?? string.Empty;
        var columnas = filas.FirstOrDefault()?.Keys.ToList() ?? [];
        var encabezado = string.Join("", columnas.Select(c => $"<Cell><Data ss:Type=\"String\">{E(c)}</Data></Cell>"));
        var filasXml = filas.Select(f => "<Row>" + string.Join("", columnas.Select(c => $"<Cell><Data ss:Type=\"String\">{E(f.TryGetValue(c, out var v) ? v : null)}</Data></Cell>")) + "</Row>");
        var xml = "<?xml version=\"1.0\"?><Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"><Worksheet ss:Name=\"" + nombre + "\"><Table><Row>" + encabezado + "</Row>" + string.Join("", filasXml) + "</Table></Worksheet></Workbook>";
        return new FileContentResult(Encoding.UTF8.GetBytes(xml), "application/vnd.ms-excel") { FileDownloadName = nombre + ".xls" };
    }
}
