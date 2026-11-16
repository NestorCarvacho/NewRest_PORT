namespace SGH.Application.Transversal;

public interface IExportadorHojaCalculo
{
    /// <summary>Excel 2003 XML Spreadsheet (.xls) compatible con informes Mid legado.</summary>
    byte[] CrearXmlSpreadsheet(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> filas,
        string nombreHoja);

    byte[] CrearXlsx(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> filas,
        string nombreHoja);
}