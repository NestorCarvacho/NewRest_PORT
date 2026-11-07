using SGH.Application.Reserva.DTOs;
using SGH.Application.Reserva.UseCases;
using SGH.Application.Sesion;
using SGH.Domain.Reserva;
using SGH.Domain.Reserva.Entities;
using SGH.Domain.Reserva.Interfaces;

namespace SGH.Tests.Reserva;

public class AlcanceReservaTests
{
    [Fact]
    public async Task SinCampamentoNoBuscaNiListaElCatalogo()
    {
        var consulta = new ConsultaReservaFalsa();
        var buscar = new BuscarReservas(consulta);
        var catalogo = new ObtenerCatalogoReserva(consulta);

        var busqueda = await buscar.EjecutarAsync(new SolicitudBuscarReservas(
            0, 0, 0, "0", 0, "0", null, null, null, null, null, 0, 0, "0"), CancellationToken.None);
        var listado = await catalogo.EjecutarAsync(9, 0, CancellationToken.None);

        Assert.Equal(AlcanceSesion.SinCampamento, Assert.IsType<ResultadoBusquedaReservas.Rechazada>(busqueda).Mensaje);
        Assert.Equal(AlcanceSesion.SinCampamento, Assert.IsType<ResultadoCatalogoReserva.Rechazado>(listado).Mensaje);
        Assert.Equal(0, consulta.Busquedas);
        Assert.Equal(0, consulta.Catalogos);
    }

    [Fact]
    public async Task ReservaDeOtroCampamentoNoRegistraElMovimiento()
    {
        var comando = new ComandoReservaFalso();
        var consulta = new ConsultaReservaFalsa { ReservasPropias = [8] };
        var caso = new RegistrarMovimientoReserva(comando, consulta);
        var fecha = new DateTime(2026, 11, 7, 15, 0, 0);

        var ajena = await caso.EjecutarAsync(
            new SolicitudMovimientoReserva(4, fecha, 3, ReglasReserva.CheckIn, 20),
            CancellationToken.None);
        var propia = await caso.EjecutarAsync(
            new SolicitudMovimientoReserva(8, fecha, 3, ReglasReserva.CheckOut, 20),
            CancellationToken.None);
        var sinCampamento = await caso.EjecutarAsync(
            new SolicitudMovimientoReserva(8, fecha, 3, ReglasReserva.CheckIn, 0),
            CancellationToken.None);

        Assert.Equal(ReglasReserva.ReservaAjena, Assert.IsType<ResultadoMovimientoReserva.Rechazado>(ajena).Mensaje);
        Assert.IsType<ResultadoMovimientoReserva.Registrado>(propia);
        Assert.Equal(AlcanceSesion.SinCampamento, Assert.IsType<ResultadoMovimientoReserva.Rechazado>(sinCampamento).Mensaje);
        Assert.Equal(1, comando.Llamadas);
        Assert.Equal(8, comando.Reservas[0]);
    }

    private sealed class ConsultaReservaFalsa : IConsultaReserva
    {
        public HashSet<int>? ReservasPropias { get; init; }

        public int Busquedas { get; private set; }

        public int Catalogos { get; private set; }

        public Task<IReadOnlyList<ReservaListada>> BuscarAsync(FiltroReserva filtro, CancellationToken cancellationToken)
        {
            Busquedas++;
            return Task.FromResult<IReadOnlyList<ReservaListada>>([]);
        }

        public Task<CatalogoReserva> ObtenerCatalogoAsync(int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
        {
            Catalogos++;
            return Task.FromResult(new CatalogoReserva([], [], [], [], []));
        }

        public Task<bool> ReservaEstaEnCampamentoAsync(int reservaId, int campamentoId, CancellationToken cancellationToken) =>
            Task.FromResult(ReservasPropias is not null && ReservasPropias.Contains(reservaId));
    }

    private sealed class ComandoReservaFalso : IComandoReserva
    {
        public int Llamadas { get; private set; }

        public List<int> Reservas { get; } = [];

        public Task<int> CrearAsync(
            int empresaMandanteId,
            int campamentoId,
            int areaId,
            DateTime fechaReserva,
            DateTime fechaLlegada,
            DateTime? fechaSalida,
            string? observacion,
            int turnoId,
            int usuarioId,
            CancellationToken cancellationToken) =>
            Task.FromResult(1);

        public Task RegistrarMovimientoAsync(int reservaId, DateTime fecha, int usuarioId, string operacion, int campamentoId, CancellationToken cancellationToken)
        {
            Llamadas++;
            Reservas.Add(reservaId);
            return Task.CompletedTask;
        }
    }
}
