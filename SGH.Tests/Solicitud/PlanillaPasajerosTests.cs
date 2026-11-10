using ClosedXML.Excel;
using SGH.Infrastructure.Solicitud.Planillas;

namespace SGH.Tests.Solicitud;

public class PlanillaPasajerosTests
{
    [Fact]
    public void PlantillaConservaColumnasYLeeFilas()
    {
        var planilla = new PlanillaPasajerosExcel();
        using var plantilla = new MemoryStream(planilla.CrearPlantilla());
        using var libro = new XLWorkbook(plantilla);
        var hoja = libro.Worksheet("pasajeros");
        hoja.Cell(2, 1).Value = "12.345.678-5";
        hoja.Cell(2, 2).Value = "rut";
        hoja.Cell(2, 3).Value = "Ana";
        hoja.Cell(2, 4).Value = "Prueba";
        hoja.Cell(2, 5).Value = "F";
        using var archivo = new MemoryStream();
        libro.SaveAs(archivo);
        archivo.Position = 0;
        var filas = planilla.Leer(archivo);
        var fila = Assert.Single(filas);
        Assert.Equal(2, fila.NumeroFila);
        Assert.Equal("12.345.678-5", fila.Identificador);
        Assert.Equal("F", fila.Genero);
    }

    [Fact]
    public void RechazaPlanillaSinColumnas()
    {
        using var libro = new XLWorkbook();
        libro.Worksheets.Add("pasajeros").Cell(1, 1).Value = "Identificador";
        using var archivo = new MemoryStream();
        libro.SaveAs(archivo);
        archivo.Position = 0;
        Assert.Throws<InvalidDataException>(() => new PlanillaPasajerosExcel().Leer(archivo));
    }
}
