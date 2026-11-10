using ClosedXML.Excel;
using Microsoft.Extensions.Logging.Abstractions;
using SGH.Application.Reserva.DTOs;
using SGH.Application.Reserva.Interfaces;
using SGH.Application.Reserva.UseCases;
using SGH.Infrastructure.Reserva.Planillas;

namespace SGH.Tests.Reserva;

public class ImportacionReservasTests
{
    [Fact]
    public async Task ImportaFilasValidasYReportaLasInvalidas()
    {
        var planilla = new PlanillaFalsa([
            new FilaPlanillaReserva(2, "Empresa - Grupo", "Día", DateTime.Today, DateTime.Today, null, null, []),
            new FilaPlanillaReserva(3, "Grupo inexistente", "Día", DateTime.Today, DateTime.Today, null, null, [])
        ]);
        var creador = new CreadorFalso();
        var caso = new ImportarReservas(
            planilla,
            new CatalogoFalso(),
            creador,
            NullLogger<ImportarReservas>.Instance);

        var resultado = await caso.EjecutarAsync(
            Stream.Null, 7, 9, 11, CancellationToken.None);

        Assert.Equal(2, resultado.FilasLeidas);
        Assert.Null(resultado.Error);
        Assert.Equal(1, creador.Llamadas);
        Assert.Equal(42, resultado.Filas[0].ReservaId);
        Assert.Null(resultado.Filas[0].Error);
        Assert.Null(resultado.Filas[1].ReservaId);
        Assert.Contains("Grupo Empresa no existe", resultado.Filas[1].Error);
    }

    [Fact]
    public void LeeUnaPlanillaXlsxYGeneraLaPlantillaDescargable()
    {
        var lector = new PlanillaReservaExcel();
        using var entrada = new MemoryStream();
        using (var libro = new XLWorkbook())
        {
            var hoja = libro.Worksheets.Add("Reservas");
            hoja.Cell(1, 1).Value = "Grupo Empresa";
            hoja.Cell(1, 2).Value = "Turno";
            hoja.Cell(1, 3).Value = "Fecha Reserva";
            hoja.Cell(1, 4).Value = "Fecha Llegada";
            hoja.Cell(1, 5).Value = "Fecha Salida";
            hoja.Cell(1, 6).Value = "Observación";
            hoja.Cell(2, 1).Value = "Empresa - Grupo";
            hoja.Cell(2, 2).Value = "Día";
            hoja.Cell(2, 3).Value = new DateTime(2026, 11, 10, 8, 30, 0);
            hoja.Cell(2, 4).Value = new DateTime(2026, 11, 11, 9, 0, 0);
            hoja.Cell(2, 5).Value = new DateTime(2026, 11, 12, 10, 0, 0);
            hoja.Cell(2, 6).Value = "Observación de prueba";
            libro.SaveAs(entrada);
        }

        entrada.Position = 0;
        var filas = lector.Leer(entrada);
        using var plantilla = new MemoryStream(lector.CrearPlantilla());
        using var libroPlantilla = new XLWorkbook(plantilla);

        Assert.Single(filas);
        Assert.Equal("Empresa - Grupo", filas[0].GrupoEmpresa);
        Assert.Equal(new DateTime(2026, 11, 10, 8, 30, 0), filas[0].FechaReserva);
        Assert.Equal("Observación de prueba", filas[0].Observacion);
        Assert.Equal("Grupo Empresa", libroPlantilla.Worksheet(1).Cell(1, 1).GetString());
    }

    private sealed class PlanillaFalsa(IReadOnlyList<FilaPlanillaReserva> filas) : IPlanillaReservaExcel
    {
        public IReadOnlyList<FilaPlanillaReserva> Leer(Stream archivo) => filas;

        public byte[] CrearPlantilla() => [];
    }

    private sealed class CatalogoFalso : IObtenerCatalogoReserva
    {
        public Task<ResultadoCatalogoReserva> EjecutarAsync(
            int empresaMandanteId, int campamentoId, CancellationToken cancellationToken) =>
            Task.FromResult<ResultadoCatalogoReserva>(new ResultadoCatalogoReserva.Listo(
                new CatalogoReservaDto(
                    [new OpcionDto(5, "Empresa - Grupo")],
                    [],
                    [],
                    [],
                    [new TurnoDto(8, "Día", "D")])));
    }

    private sealed class CreadorFalso : ICrearReserva
    {
        public int Llamadas { get; private set; }

        public Task<ResultadoAltaReserva> EjecutarAsync(
            SolicitudCrearReserva solicitud, CancellationToken cancellationToken)
        {
            Llamadas++;
            return Task.FromResult<ResultadoAltaReserva>(new ResultadoAltaReserva.Creada(42));
        }
    }
}
