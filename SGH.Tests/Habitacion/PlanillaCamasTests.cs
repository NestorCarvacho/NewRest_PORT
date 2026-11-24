using ClosedXML.Excel;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Infrastructure.Habitacion.Planillas;

namespace SGH.Tests.Habitacion;

public class PlanillaCamasTests
{
    [Fact]
    public void PlantillaIncluyeHojaCamasTiposEInstrucciones()
    {
        var planilla = new PlanillaCamasExcel();
        var bytes = planilla.CrearPlantilla(42, [new TipoCamaListado(3, 42, "1 plaza", 90, 190, true, 0)]);
        using var libro = new XLWorkbook(new MemoryStream(bytes));
        Assert.NotNull(libro.Worksheet("camas"));
        Assert.NotNull(libro.Worksheet("TiposCama"));
        var instrucciones = libro.Worksheet("Instrucciones");
        Assert.Equal(42, instrucciones.Cell(3, 2).GetDouble());
        Assert.Equal(3, libro.Worksheet("TiposCama").Cell(2, 1).GetDouble());
    }

    [Fact]
    public void LeeFilasDeHojaCamas()
    {
        var planilla = new PlanillaCamasExcel();
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("camas");
        var encabezados = new[] { "CodigoHabitacion", "CodigoCama", "TipoCamaId", "Habilitada", "Orden",
            "FechaColchon", "ColchonObservacion", "FechaCambioRopa", "RopaCamaObservacion" };
        for (var i = 0; i < encabezados.Length; i++)
            hoja.Cell(1, i + 1).Value = encabezados[i];
        hoja.Cell(2, 1).Value = "H-101";
        hoja.Cell(2, 2).Value = "C1";
        hoja.Cell(2, 3).Value = 5;
        hoja.Cell(2, 4).Value = "Sí";
        hoja.Cell(2, 5).Value = 1;
        using var archivo = new MemoryStream();
        libro.SaveAs(archivo);
        archivo.Position = 0;
        var filas = planilla.Leer(archivo);
        var fila = Assert.Single(filas);
        Assert.Equal("H-101", fila.CodigoHabitacion);
        Assert.Equal(5, fila.TipoCamaId);
        Assert.True(fila.Habilitada);
    }
}