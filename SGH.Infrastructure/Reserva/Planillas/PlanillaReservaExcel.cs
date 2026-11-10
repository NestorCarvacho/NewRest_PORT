using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using SGH.Application.Reserva.DTOs;
using SGH.Application.Reserva.Interfaces;

namespace SGH.Infrastructure.Reserva.Planillas;

public sealed class PlanillaReservaExcel : IPlanillaReservaExcel
{
    private static readonly string[] EncabezadosObligatorios =
    [
        "Grupo Empresa",
        "Turno",
        "Fecha Reserva",
        "Fecha Llegada"
    ];

    public IReadOnlyList<FilaPlanillaReserva> Leer(Stream archivo)
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
            var hoja = libro.Worksheets.FirstOrDefault()
                ?? throw new InvalidDataException("La planilla no contiene hojas.");
            var ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 0;
            var ultimaColumna = hoja.LastColumnUsed()?.ColumnNumber() ?? 0;
            if (ultimaFila < 2 || ultimaColumna == 0)
                return [];

            var columnas = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var columna = 1; columna <= ultimaColumna; columna++)
            {
                var encabezado = hoja.Cell(1, columna).GetString().Trim();
                if (!string.IsNullOrEmpty(encabezado))
                    columnas[Normalizar(encabezado)] = columna;
            }

            var faltantes = EncabezadosObligatorios
                .Where(encabezado => !columnas.ContainsKey(Normalizar(encabezado)))
                .ToList();
            if (faltantes.Count > 0)
                throw new InvalidDataException(
                    $"Faltan columnas obligatorias: {string.Join(", ", faltantes)}.");

            var filas = new List<FilaPlanillaReserva>();
            for (var numeroFila = 2; numeroFila <= ultimaFila; numeroFila++)
            {
                var celdas = Enumerable.Range(1, ultimaColumna)
                    .Select(columna => hoja.Cell(numeroFila, columna));
                if (celdas.All(celda => celda.IsEmpty()))
                    continue;

                var errores = new List<string>();
                var fechaReserva = LeerFecha(hoja, numeroFila, columnas, "Fecha Reserva", errores);
                var fechaLlegada = LeerFecha(hoja, numeroFila, columnas, "Fecha Llegada", errores);
                var fechaSalida = LeerFecha(hoja, numeroFila, columnas, "Fecha Salida", errores);
                var observacion = LeerTexto(hoja, numeroFila, columnas, "Observación");

                filas.Add(new FilaPlanillaReserva(
                    numeroFila,
                    LeerTexto(hoja, numeroFila, columnas, "Grupo Empresa") ?? string.Empty,
                    LeerTexto(hoja, numeroFila, columnas, "Turno") ?? string.Empty,
                    fechaReserva,
                    fechaLlegada,
                    fechaSalida,
                    observacion,
                    errores));
            }

            return filas;
        }
    }

    public byte[] CrearPlantilla()
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Reservas");
        var encabezados = new[]
        {
            "Grupo Empresa",
            "Turno",
            "Fecha Reserva",
            "Fecha Llegada",
            "Fecha Salida",
            "Observación"
        };

        for (var columna = 0; columna < encabezados.Length; columna++)
            hoja.Cell(1, columna + 1).Value = encabezados[columna];

        var cabecera = hoja.Range(1, 1, 1, encabezados.Length);
        cabecera.Style.Font.Bold = true;
        cabecera.Style.Fill.BackgroundColor = XLColor.FromHtml("#AF6116");
        cabecera.Style.Font.FontColor = XLColor.White;
        hoja.SheetView.FreezeRows(1);
        hoja.Columns(1, encabezados.Length).AdjustToContents();
        hoja.Column(3).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
        hoja.Column(4).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
        hoja.Column(5).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    private static string? LeerTexto(
        IXLWorksheet hoja,
        int fila,
        IReadOnlyDictionary<string, int> columnas,
        string encabezado)
    {
        if (!columnas.TryGetValue(Normalizar(encabezado), out var columna))
            return null;

        var texto = hoja.Cell(fila, columna).GetString().Trim();
        return texto.Length == 0 ? null : texto;
    }

    private static DateTime? LeerFecha(
        IXLWorksheet hoja,
        int fila,
        IReadOnlyDictionary<string, int> columnas,
        string encabezado,
        ICollection<string> errores)
    {
        if (!columnas.TryGetValue(Normalizar(encabezado), out var columna))
            return null;

        var celda = hoja.Cell(fila, columna);
        if (celda.IsEmpty())
            return null;
        if (celda.TryGetValue<DateTime>(out var fecha))
            return fecha;

        var texto = celda.GetString().Trim();
        var formatos = new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm", "dd/MM/yyyy", "dd/MM/yyyy HH:mm" };
        if (DateTime.TryParseExact(texto, formatos, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out fecha))
            return fecha;

        errores.Add($"{encabezado} no tiene un formato de fecha válido.");
        return null;
    }

    private static string Normalizar(string texto) =>
        string.Concat(texto.Trim().Normalize(NormalizationForm.FormD)
            .Where(caracter => CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark))
            .ToUpperInvariant();
}
