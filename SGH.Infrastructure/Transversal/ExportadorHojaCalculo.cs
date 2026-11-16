using System.Globalization;
using System.Security;
using System.Text;
using ClosedXML.Excel;
using SGH.Application.Transversal;

namespace SGH.Infrastructure.Transversal;

public sealed class ExportadorHojaCalculo : IExportadorHojaCalculo
{
    public byte[] CrearXmlSpreadsheet(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> filas,
        string nombreHoja)
    {
        static string Escapar(object? valor) =>
            SecurityElement.Escape(Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty) ?? string.Empty;

        var columnas = filas.FirstOrDefault()?.Keys.ToList() ?? [];
        var encabezado = string.Join("", columnas.Select(c => $"<Cell><Data ss:Type=\"String\">{Escapar(c)}</Data></Cell>"));
        var datos = filas.Select(f => "<Row>" + string.Join("", columnas.Select(c =>
            $"<Cell><Data ss:Type=\"String\">{Escapar(f.TryGetValue(c, out var v) ? v : null)}</Data></Cell>")) + "</Row>");

        var xml = "<?xml version=\"1.0\"?><Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">" +
                  $"<Worksheet ss:Name=\"{SecurityElement.Escape(nombreHoja)}\"><Table><Row>{encabezado}</Row>{string.Join("", datos)}</Table></Worksheet></Workbook>";
        return Encoding.UTF8.GetBytes(xml);
    }

    public byte[] CrearXlsx(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> filas,
        string nombreHoja)
    {
        if (string.IsNullOrWhiteSpace(nombreHoja))
            throw new ArgumentException("El nombre de la hoja es obligatorio.", nameof(nombreHoja));

        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add(nombreHoja.Length > 31 ? nombreHoja[..31] : nombreHoja);
        var columnas = filas.FirstOrDefault()?.Keys.ToList() ?? [];
        for (var c = 0; c < columnas.Count; c++)
            hoja.Cell(1, c + 1).SetValue(columnas[c]);

        for (var r = 0; r < filas.Count; r++)
        {
            for (var c = 0; c < columnas.Count; c++)
            {
                var clave = columnas[c];
                var valor = filas[r].TryGetValue(clave, out var v) ? v : null;
                hoja.Cell(r + 2, c + 1).SetValue(valor switch
                {
                    null => string.Empty,
                    DateTime dt => dt,
                    decimal dec => dec,
                    double dbl => dbl,
                    int entero => entero,
                    long largo => largo,
                    bool booleano => booleano,
                    _ => Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty
                });
            }
        }

        hoja.Row(1).Style.Font.Bold = true;
        using var flujo = new MemoryStream();
        libro.SaveAs(flujo);
        return flujo.ToArray();
    }
}