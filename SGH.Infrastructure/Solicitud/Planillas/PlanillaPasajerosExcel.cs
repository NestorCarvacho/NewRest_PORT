using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.Interfaces;

namespace SGH.Infrastructure.Solicitud.Planillas;

public sealed class PlanillaPasajerosExcel : IPlanillaPasajeros
{
    private static readonly string[] Encabezados = ["Identificador", "TipoIdentificador", "Nombres", "Apellidos", "Genero"];

    public IReadOnlyList<FilaCargaPasajeroDto> Leer(Stream archivo)
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
            var hoja = libro.Worksheets.FirstOrDefault(x => string.Equals(x.Name, "pasajeros", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidDataException("La planilla debe contener una hoja llamada pasajeros.");
            var ultimaColumna = hoja.LastColumnUsed()?.ColumnNumber() ?? 0;
            var columnas = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var columna = 1; columna <= ultimaColumna; columna++)
            {
                var nombre = hoja.Cell(1, columna).GetString().Trim();
                if (!string.IsNullOrEmpty(nombre) && !columnas.TryAdd(nombre, columna))
                    throw new InvalidDataException($"La columna {nombre} está repetida.");
            }
            var faltantes = Encabezados.Where(nombre => !columnas.ContainsKey(nombre)).ToArray();
            if (faltantes.Length > 0)
                throw new InvalidDataException($"Faltan columnas obligatorias: {string.Join(", ", faltantes)}.");
            var filas = new List<FilaCargaPasajeroDto>();
            var ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 1;
            for (var numero = 2; numero <= ultimaFila; numero++)
            {
                string Texto(string nombre) => hoja.Cell(numero, columnas[nombre]).GetFormattedString().Trim();
                var valores = Encabezados.Select(Texto).ToArray();
                if (valores.All(string.IsNullOrWhiteSpace))
                    continue;
                filas.Add(new FilaCargaPasajeroDto(numero, valores[0], valores[1], valores[2], valores[3], valores[4]));
            }
            return filas;
        }
    }

    public byte[] CrearPlantilla()
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("pasajeros");
        for (var columna = 0; columna < Encabezados.Length; columna++)
            hoja.Cell(1, columna + 1).Value = Encabezados[columna];
        hoja.Row(1).Style.Font.Bold = true;
        hoja.Column(1).Style.NumberFormat.Format = "@";
        hoja.Columns().AdjustToContents();
        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }
}
