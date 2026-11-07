using SGH.Application.Censo.DTOs;
using SGH.Application.Censo.UseCases;
using SGH.Domain.Censo;
using SGH.Domain.Censo.Entities;
using SGH.Domain.Censo.Interfaces;

namespace SGH.Tests.Censo;

public class ReglasCensoTests
{
    [Fact]
    public void BusquedaExigeFechaYTurnoDiaONoche()
    {
        var fecha = new DateTime(2026, 11, 7);

        Assert.Equal("Debe ingresar la fecha de censo.", ReglasCenso.ValidarBusqueda(null, "D"));
        Assert.Equal("Seleccione el tipo de turno.", ReglasCenso.ValidarBusqueda(fecha, "X"));
        Assert.Null(ReglasCenso.ValidarBusqueda(fecha, "D"));
        Assert.Null(ReglasCenso.ValidarBusqueda(fecha, "N"));
    }
}

public class GuardarCensosTests
{
    [Fact]
    public async Task AcumulaElErrorDeCadaFilaYNoCortaEnLaPrimera()
    {
        var comando = new ComandoCensoFalso(habitacion =>
            habitacion == 2
                ? new ResultadoFilaCenso(1, "Habitación 2 rechazada.")
                : new ResultadoFilaCenso(0, null));
        var caso = new GuardarCensos(comando);

        var resultado = await caso.EjecutarAsync(new SolicitudGuardarCensos(
            4,
            new DateTime(2026, 11, 7),
            [
                new FilaGuardarCensoDto("1", 1, "2", "D"),
                new FilaGuardarCensoDto("2", 2, "1", "D")
            ]), CancellationToken.None);

        var rechazado = Assert.IsType<ResultadoGuardadoCenso.Rechazado>(resultado);
        Assert.Equal("Habitación 2 rechazada.", rechazado.Mensaje);
        Assert.Equal(2, comando.Llamadas);
    }

    [Fact]
    public async Task GuardaCuandoTodasLasFilasDevuelvenCero()
    {
        var comando = new ComandoCensoFalso(_ => new ResultadoFilaCenso(0, null));
        var caso = new GuardarCensos(comando);

        var resultado = await caso.EjecutarAsync(new SolicitudGuardarCensos(
            4,
            new DateTime(2026, 11, 7),
            [new FilaGuardarCensoDto("1", 9, "3", "N")]), CancellationToken.None);

        Assert.IsType<ResultadoGuardadoCenso.Guardado>(resultado);
    }

    private sealed class ComandoCensoFalso : IComandoCenso
    {
        private readonly Func<int, ResultadoFilaCenso> _respuesta;

        public ComandoCensoFalso(Func<int, ResultadoFilaCenso> respuesta)
        {
            _respuesta = respuesta;
        }

        public int Llamadas { get; private set; }

        public Task<ResultadoFilaCenso> GuardarAsync(FilaCensoGuardar fila, DateTime fecha, int usuarioId, CancellationToken cancellationToken)
        {
            Llamadas++;
            return Task.FromResult(_respuesta(fila.HabitacionId));
        }
    }
}
