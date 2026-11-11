using System.Globalization;
using System.Security;
using System.Text;

namespace SGH.Application.Mid.Informes;

public static class ExportarExcel
{
    public static byte[] Crear(IReadOnlyList<IReadOnlyDictionary<string, object?>> filas, string nombreHoja)
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
}
