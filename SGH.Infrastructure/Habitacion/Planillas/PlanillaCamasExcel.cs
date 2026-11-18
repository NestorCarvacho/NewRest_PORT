using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using SGH.Application.Habitacion.DTOs;
using SGH.Application.Habitacion.Interfaces;
using SGH.Domain.Habitacion.Interfaces;

namespace SGH.Infrastructure.Habitacion.Planillas;

public sealed class PlanillaCamasExcel : IPlanillaCamas
{
    private static readonly string[] EncabezadosCamas =
    [
        "CodigoHabitacion", "CodigoCama", "TipoCamaId", "Habilitada", "Orden",
        "FechaColchon", "ColchonObservacion", "FechaCambioRopa", "RopaCamaObservacion"
    ];

    public IReadOnlyList<FilaCargaCamaDto> Leer(Stream archivo)
    {
        XLWorkbook libro;
        try
        {
            libro = new XLWorkbook(archivo);
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or OpenXmlPackageException)
        {
            throw new InvalidDataException("El archivo no es una planilla Excel .xlsx válida.", ex);
        }

        using (libro)
        {
            var hoja = libro.Worksheets.FirstOrDefault(x => string.Equals(x.Name, "camas", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidDataException("La planilla debe contener una hoja llamada camas.");
            var columnas = LeerEncabezados(hoja);
            var faltantes = EncabezadosCamas.Where(n => n is not ("FechaColchon" or "ColchonObservacion" or "FechaCambioRopa" or "RopaCamaObservacion"))
                .Where(n => !columnas.ContainsKey(n)).ToArray();
            if (faltantes.Length > 0)
                throw new InvalidDataException($"Faltan columnas obligatorias: {string.Join(", ", faltantes)}.");

            var filas = new List<FilaCargaCamaDto>();
            var ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 1;
            for (var numero = 2; numero <= ultimaFila; numero++)
            {
                string Texto(string nombre) => columnas.TryGetValue(nombre, out var col)
                    ? hoja.Cell(numero, col).GetFormattedString().Trim()
                    : "";
                var habitacion = Texto("CodigoHabitacion");
                var cama = Texto("CodigoCama");
                if (string.IsNullOrWhiteSpace(habitacion) && string.IsNullOrWhiteSpace(cama))
                    continue;
                filas.Add(new FilaCargaCamaDto(
                    numero,
                    habitacion,
                    cama,
                    EnteroOpcional(Texto("TipoCamaId")),
                    InterpretarHabilitada(Texto("Habilitada")),
                    EnteroOpcional(Texto("Orden")) ?? 0,
                    FechaOpcional(Texto("FechaColchon")),
                    VacioANull(Texto("ColchonObservacion")),
                    FechaOpcional(Texto("FechaCambioRopa")),
                    VacioANull(Texto("RopaCamaObservacion"))));
            }

            return filas;
        }
    }

    public byte[] CrearPlantilla(int campamentoId, IReadOnlyList<TipoCamaListado> tiposCama)
    {
        using var libro = new XLWorkbook();
        var instrucciones = libro.Worksheets.Add("Instrucciones");
        instrucciones.Cell(1, 1).Value = "Carga masiva de camas";
        instrucciones.Cell(1, 1).Style.Font.Bold = true;
        instrucciones.Cell(3, 1).Value = "CampamentoId (sesión actual)";
        instrucciones.Cell(3, 2).Value = campamentoId;
        instrucciones.Cell(5, 1).Value = "Complete la hoja «camas». Use «TiposCama» para los Id de tipo.";
        instrucciones.Cell(7, 1).Value = "CodigoHabitacion: nombre/código de la habitación en el campamento.";
        instrucciones.Cell(8, 1).Value = "Habilitada: 1/Sí/S para habilitada; 0/No/N para deshabilitada.";
        instrucciones.Columns().AdjustToContents();

        var camas = libro.Worksheets.Add("camas");
        for (var i = 0; i < EncabezadosCamas.Length; i++)
        {
            camas.Cell(1, i + 1).Value = EncabezadosCamas[i];
            if (EncabezadosCamas[i] is "CodigoHabitacion" or "CodigoCama")
                camas.Column(i + 1).Style.NumberFormat.Format = "@";
        }
        camas.Row(1).Style.Font.Bold = true;
        camas.Columns().AdjustToContents();

        var tipos = libro.Worksheets.Add("TiposCama");
        var encabezadosTipos = new[] { "Id", "Nombre", "AnchoCm", "LargoCm", "Activo" };
        for (var i = 0; i < encabezadosTipos.Length; i++)
            tipos.Cell(1, i + 1).Value = encabezadosTipos[i];
        tipos.Row(1).Style.Font.Bold = true;
        var fila = 2;
        foreach (var tipo in tiposCama.OrderBy(t => t.Nombre, StringComparer.OrdinalIgnoreCase))
        {
            tipos.Cell(fila, 1).Value = tipo.Id;
            tipos.Cell(fila, 2).Value = tipo.Nombre;
            tipos.Cell(fila, 3).Value = tipo.AnchoCm;
            tipos.Cell(fila, 4).Value = tipo.LargoCm;
            tipos.Cell(fila, 5).Value = tipo.IsActive ? "Sí" : "No";
            fila++;
        }
        tipos.Columns().AdjustToContents();

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    private static Dictionary<string, int> LeerEncabezados(IXLWorksheet hoja)
    {
        var columnas = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var ultimaColumna = hoja.LastColumnUsed()?.ColumnNumber() ?? 0;
        for (var columna = 1; columna <= ultimaColumna; columna++)
        {
            var nombre = hoja.Cell(1, columna).GetString().Trim();
            if (!string.IsNullOrEmpty(nombre) && !columnas.TryAdd(nombre, columna))
                throw new InvalidDataException($"La columna {nombre} está repetida.");
        }
        return columnas;
    }

    private static int? EnteroOpcional(string texto) =>
        int.TryParse(texto, out var valor) ? valor : null;

    private static DateTime? FechaOpcional(string texto) =>
        DateTime.TryParse(texto, out var fecha) ? fecha.Date : null;

    private static string? VacioANull(string texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static bool InterpretarHabilitada(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return true;
        var u = texto.Trim().ToUpperInvariant();
        if (u is "0" or "N" or "NO" or "FALSE" or "F")
            return false;
        return true;
    }
}