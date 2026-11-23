using SGH.Application.Censo.DTOs;
using SGH.Application.Censo.UseCases;
using SGH.Application.Sesion;
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
        var caso = new GuardarCensos(comando, new ConsultaCensoFalsa());

        var resultado = await caso.EjecutarAsync(new SolicitudGuardarCensos(
            4,
            1,
            20,
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
        var caso = new GuardarCensos(comando, new ConsultaCensoFalsa());

        var resultado = await caso.EjecutarAsync(new SolicitudGuardarCensos(
            4,
            1,
            20,
            new DateTime(2026, 11, 7),
            [new FilaGuardarCensoDto("1", 9, "3", "N")]), CancellationToken.None);

        Assert.IsType<ResultadoGuardadoCenso.Guardado>(resultado);
    }

    [Fact]
    public async Task HabitacionDeOtroCampamentoNoSeEnviaAlProcedimiento()
    {
        var comando = new ComandoCensoFalso(_ => new ResultadoFilaCenso(0, null));
        var consulta = new ConsultaCensoFalsa { HabitacionesPropias = [9] };
        var caso = new GuardarCensos(comando, consulta);

        var resultado = await caso.EjecutarAsync(new SolicitudGuardarCensos(
            4,
            1,
            20,
            new DateTime(2026, 11, 7),
            [
                new FilaGuardarCensoDto("1", 9, "3", "D"),
                new FilaGuardarCensoDto("2", 4, "1", "D")
            ]), CancellationToken.None);

        var rechazado = Assert.IsType<ResultadoGuardadoCenso.Rechazado>(resultado);
        Assert.Equal(ReglasCenso.HabitacionAjena, rechazado.Mensaje);
        Assert.Equal(1, comando.Llamadas);
        Assert.Equal(9, comando.Habitaciones[0]);
    }

    [Fact]
    public async Task SinCampamentoNoBuscaNiGuarda()
    {
        var comando = new ComandoCensoFalso(_ => new ResultadoFilaCenso(0, null));
        var consulta = new ConsultaCensoFalsa();
        var guardar = new GuardarCensos(comando, consulta);
        var buscar = new BuscarCensos(consulta);
        var fecha = new DateTime(2026, 11, 7);

        var guardado = await guardar.EjecutarAsync(new SolicitudGuardarCensos(
            4,
            9,
            0,
            fecha,
            [new FilaGuardarCensoDto("1", 9, "3", "D")]), CancellationToken.None);
        var busqueda = await buscar.EjecutarAsync(new SolicitudBuscarCensos(9, 0, 0, 0, 0, fecha, "D"), CancellationToken.None);

        Assert.Equal(AlcanceSesion.SinCampamento, Assert.IsType<ResultadoGuardadoCenso.Rechazado>(guardado).Mensaje);
        Assert.Equal(AlcanceSesion.SinCampamento, Assert.IsType<ResultadoBusquedaCenso.Rechazada>(busqueda).Mensaje);
        Assert.Equal(0, comando.Llamadas);
        Assert.Equal(0, consulta.Busquedas);
    }

    private sealed class ComandoCensoFalso : IComandoCenso
    {
        private readonly Func<int, ResultadoFilaCenso> _respuesta;

        public ComandoCensoFalso(Func<int, ResultadoFilaCenso> respuesta)
        {
            _respuesta = respuesta;
        }

        public int Llamadas { get; private set; }

        public List<int> Habitaciones { get; } = [];

        public Task<ResultadoFilaCenso> GuardarAsync(FilaCensoGuardar fila, DateTime fecha, int usuarioId, int campamentoId, CancellationToken cancellationToken)
        {
            Llamadas++;
            Habitaciones.Add(fila.HabitacionId);
            return Task.FromResult(_respuesta(fila.HabitacionId));
        }
    }

    private sealed class ConsultaCensoFalsa : IConsultaCenso
    {
        public HashSet<int>? HabitacionesPropias { get; init; }

        public int Busquedas { get; private set; }

        public Task<IReadOnlyList<CensoListado>> BuscarAsync(FiltroCenso filtro, CancellationToken cancellationToken)
        {
            Busquedas++;
            return Task.FromResult<IReadOnlyList<CensoListado>>([]);
        }

        public Task<IReadOnlyList<CensoUsuarioListado>> BuscarPorUsuarioAsync(
            int empresaMandanteId,
            int campamentoId,
            DateTime fecha,
            int usuarioId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CensoUsuarioListado>>([]);

        public Task<CatalogoCenso> ObtenerCatalogoAsync(int campamentoId, CancellationToken cancellationToken) =>
            Task.FromResult(new CatalogoCenso([], [], [], [], []));

        public Task<HabitacionCensoQr?> ObtenerHabitacionQrAsync(int campamentoId, int habitacionId, CancellationToken cancellationToken) =>
            Task.FromResult<HabitacionCensoQr?>(null);

        public Task<bool> FiltroEstaEnCampamentoAsync(int campamentoId, int moduloId, int pisoId, int habitacionId, CancellationToken cancellationToken) =>
            Task.FromResult(HabitacionesPropias is null || HabitacionesPropias.Contains(habitacionId));
    }
}
