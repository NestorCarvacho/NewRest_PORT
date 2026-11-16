using ClosedXML.Excel;
using SGH.Infrastructure.Transversal;

namespace SGH.Tests.Transversal;

public class ServiciosTransversalesNet10Tests
{
    [Fact]
    public void QrCensoHabitacionGeneraPngValido()
    {
        var png = new GeneradorCodigoQr().CrearPngCensoHabitacion(42);
        Assert.NotEmpty(png);
        Assert.Equal(0x89, png[0]);
        Assert.Equal(0x50, png[1]);
    }

    [Fact]
    public void PdfTabularTieneCabeceraPdf()
    {
        var pdf = new GeneradorPdfDocumento().CrearInformeTabular(
            "Informe prueba",
            ["Col1", "Col2"],
            [["A", "B"], ["1", "2"]]);
        Assert.NotEmpty(pdf);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public void ExcelXlsxAbreConClosedXml()
    {
        var filas = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["Id"] = 1, ["Nombre"] = "Prueba" }
        };
        var bytes = new ExportadorHojaCalculo().CrearXlsx(filas, "Hoja");
        using var libro = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("Prueba", libro.Worksheet(1).Cell(2, 2).GetString());
    }

    [Fact]
    public void ExcelXmlSpreadsheetConservaColumnas()
    {
        var filas = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["Estado"] = "OK" }
        };
        var bytes = new ExportadorHojaCalculo().CrearXmlSpreadsheet(filas, "Informe");
        var texto = System.Text.Encoding.UTF8.GetString(bytes);
        Assert.Contains("Estado", texto, StringComparison.Ordinal);
        Assert.Contains("OK", texto, StringComparison.Ordinal);
    }
}