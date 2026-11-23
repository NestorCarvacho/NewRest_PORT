using System.Text.Json;
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
    public async Task BusquedaConservaMovimientosRegistradosYCheckoutPendienteEnJson()
    {
        var consulta = new ConsultaReservaFalsa
        {
            Filas =
            [
                new ReservaListada(1, 2, 3, 4, 5, "Grupo", "Pasajero", "Habitación", "Asignada", "A",
                    "10/11/2026 10:00", "11/11/2026 00:00", "<Sin Asignar>", "11/11/2026 09:18", "11/11/2026 13:18"),
                new ReservaListada(2, 2, 3, 4, 5, "Grupo", "Pasajero", "Habitación", "Asignada", "A",
                    "10/11/2026 10:00", "11/11/2026 00:00", "<Sin Asignar>", "11/11/2026 09:18", null)
            ]
        };
        var caso = new BuscarReservas(consulta);

        var resultado = await caso.EjecutarAsync(new SolicitudBuscarReservas(
            1, 20, 0, 0, "0", 0, "0", null, null, null, null, null, null, null, 0, 0, "0"), CancellationToken.None);

        var filas = Assert.IsType<ResultadoBusquedaReservas.Lista>(resultado).Filas;
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(filas, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        Assert.Equal("11/11/2026 09:18", json.RootElement[0].GetProperty("checkIn").GetString());
        Assert.Equal("11/11/2026 13:18", json.RootElement[0].GetProperty("checkOut").GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement[1].GetProperty("checkOut").ValueKind);
    }

    [Fact]
    public async Task SinCampamentoNoBuscaNiListaElCatalogo()
    {
        var consulta = new ConsultaReservaFalsa();
        var buscar = new BuscarReservas(consulta);
        var catalogo = new ObtenerCatalogoReserva(consulta);

        var busqueda = await buscar.EjecutarAsync(new SolicitudBuscarReservas(
            1, 0, 0, 0, "0", 0, "0", null, null, null, null, null, null, null, 0, 0, "0"), CancellationToken.None);
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
            new SolicitudMovimientoReserva(4, fecha, 3, ReglasReserva.CheckIn, 1, 20),
            CancellationToken.None);
        var propia = await caso.EjecutarAsync(
            new SolicitudMovimientoReserva(8, fecha, 3, ReglasReserva.CheckOut, 1, 20),
            CancellationToken.None);
        var sinCampamento = await caso.EjecutarAsync(
            new SolicitudMovimientoReserva(8, fecha, 3, ReglasReserva.CheckIn, 1, 0),
            CancellationToken.None);

        Assert.Equal(ReglasReserva.ReservaAjena, Assert.IsType<ResultadoMovimientoReserva.Rechazado>(ajena).Mensaje);
        Assert.IsType<ResultadoMovimientoReserva.Registrado>(propia);
        Assert.Equal(AlcanceSesion.SinCampamento, Assert.IsType<ResultadoMovimientoReserva.Rechazado>(sinCampamento).Mensaje);
        Assert.Equal(1, comando.Llamadas);
        Assert.Equal(8, comando.Reservas[0]);
    }

    [Fact]
    public async Task AnularValidaAlcanceYEstado()
    {
        var comando = new ComandoReservaFalso();
        var consulta = new ConsultaReservaFalsa
        {
            ReservasPropias = [8],
            Filas = [new ReservaListada(8, 2, 3, 4, 5, "G", "P", "H", "Asignada", "A",
                "01/01/2026", "02/01/2026", null, "", "")]
        };
        var caso = new AnularReserva(comando, consulta);

        var ajena = await caso.EjecutarAsync(new SolicitudAnularReserva(4, 1, 20, 3), CancellationToken.None);
        var propia = await caso.EjecutarAsync(new SolicitudAnularReserva(8, 1, 20, 3), CancellationToken.None);

        Assert.IsType<ResultadoAnularReserva.Rechazada>(ajena);
        Assert.IsType<ResultadoAnularReserva.Anulada>(propia);
        Assert.Equal(1, comando.Anulaciones);
    }

    [Fact]
    public async Task ReservaDeOtroCampamentoNoSeActualiza()
    {
        var comando = new ComandoReservaFalso();
        var consulta = new ConsultaReservaFalsa { ReservasPropias = [12] };
        var caso = new RegistrarMovimientoReserva.ActualizarReserva(comando, consulta);
        var fecha = new DateTime(2026, 11, 7, 10, 0, 0);

        var ajena = await caso.EjecutarAsync(new SolicitudActualizarReserva(
            99, 1, 20, 2, fecha, fecha, null, null, 1, null, null, "P", false, 3), CancellationToken.None);
        var propia = await caso.EjecutarAsync(new SolicitudActualizarReserva(
            12, 1, 20, 2, fecha, fecha, null, null, 1, null, null, "P", false, 3), CancellationToken.None);

        Assert.Equal(ReglasReserva.ReservaAjena, Assert.IsType<ResultadoActualizacionReserva.Rechazada>(ajena).Mensaje);
        Assert.IsType<ResultadoActualizacionReserva.Actualizada>(propia);
        Assert.Equal(1, comando.Actualizaciones);
    }

    private sealed class ConsultaReservaFalsa : IConsultaReserva
    {
        public HashSet<int>? ReservasPropias { get; init; }

        public IReadOnlyList<ReservaListada> Filas { get; init; } = [];

        public int Busquedas { get; private set; }

        public int Catalogos { get; private set; }

        public Task<IReadOnlyList<ReservaListada>> BuscarAsync(FiltroReserva filtro, CancellationToken cancellationToken)
        {
            Busquedas++;
            return Task.FromResult(Filas);
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

        public int Actualizaciones { get; private set; }

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

        public Task ActualizarAsync(int reservaId, int empresaMandanteId, int campamentoId, int areaId,
            DateTime fechaReserva, DateTime fechaLlegada, DateTime? fechaSalida, string? observacion,
            int turnoId, int? pasajeroId, int? habitacionId, string estado, bool tieneAgua,
            int usuarioId, CancellationToken cancellationToken)
        {
            Actualizaciones++;
            return Task.CompletedTask;
        }

        public int Anulaciones { get; private set; }

        public int Finalizaciones { get; private set; }

        public Task AnularAsync(int reservaId, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
        {
            Anulaciones++;
            return Task.CompletedTask;
        }

        public Task FinalizarCheckoutAsync(int reservaId, int empresaMandanteId, int usuarioId, CancellationToken cancellationToken)
        {
            Finalizaciones++;
            return Task.CompletedTask;
        }

        public Task ReversarMovimientoAsync(int reservaId, int campamentoId, int usuarioId, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
