using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.UseCases;
using SGH.Application.Solicitud.Interfaces;

namespace SGH.Tests.Solicitud;

public class GestionSolicitudesTests
{
    private readonly Falso _falso = new();
    private GestionSolicitudes Caso => new(_falso, _falso);

    [Fact]
    public async Task BusquedaConservaFiltrosYAcotaSesion()
    {
        var fecha = new DateTime(2026, 11, 10, 15, 30, 0);
        var filtros = new FiltroSolicitudesDto(2, 3, fecha, fecha.AddDays(1), 4, 5, "P", 6);
        await Caso.BuscarAsync(filtros, CancellationToken.None);
        Assert.Equal(new FiltroSolicitudesDto(2, 3, fecha.Date, fecha.AddDays(1).Date, 4, 5, "P", 6), _falso.Filtro);
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.BuscarAsync(filtros with { CampamentoId = 0 }, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.BuscarAsync(filtros with { Hasta = fecha.AddDays(-1) }, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.BuscarAsync(filtros with { Estado = "invalid" }, CancellationToken.None));
        Assert.Equal(1, _falso.Busquedas);
    }

    [Fact]
    public async Task CreacionValidaFechasCorreoYTurnoAntesDeGuardar()
    {
        var datos = Crear();
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.GuardarAsync(datos with { GrupoId = 0 }, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.GuardarAsync(datos with { TurnoId = null }, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.GuardarAsync(datos with { FechaCheckIn = null }, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.GuardarAsync(datos with { FechaCheckOut = datos.FechaCheckIn!.Value.AddDays(-1) }, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.GuardarAsync(datos with { ConSistemaTurno = true, FechaCheckOut = null }, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.GuardarAsync(datos with { EmailContactoCampamento = "incorrecto" }, CancellationToken.None));
        Assert.Equal(0, _falso.Guardados);
        Assert.Equal(42, await Caso.GuardarAsync(datos, CancellationToken.None));
        Assert.Equal(1, _falso.Guardados);
        Assert.Equal(datos.GrupoId, _falso.Datos!.GrupoId);
        Assert.Equal(datos.FechaCheckIn, _falso.Datos.FechaCheckIn);
    }

    [Fact]
    public async Task ActualizacionNoExigeCamposInmutablesYAnulacionExigeSesion()
    {
        var datos = Crear() with { Id = 42, GrupoId = 0, FechaSolicitud = null, FechaCheckIn = null };
        Assert.Equal(42, await Caso.GuardarAsync(datos, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.AnularAsync(42, 2, 3, 0, CancellationToken.None));
        await Caso.AnularAsync(42, 2, 3, 7, CancellationToken.None);
        Assert.Equal((42, 2, 3, 7), _falso.Anulacion);
    }

    [Fact]
    public async Task IdentificadoresOpcionalesCeroSeGuardanComoNull()
    {
        await Caso.GuardarAsync(Crear() with
        {
            SolicitanteId = 0, TurnoRotacionId = 0, UnidadNegocioId = 0, DivisionId = 0, DepartamentoId = 0
        }, CancellationToken.None);
        Assert.Null(_falso.Datos!.SolicitanteId);
        Assert.Null(_falso.Datos.TurnoRotacionId);
        Assert.Null(_falso.Datos.UnidadNegocioId);
        Assert.Null(_falso.Datos.DivisionId);
        Assert.Null(_falso.Datos.DepartamentoId);
    }

    [Fact]
    public async Task ListadoPasajerosValidaYTransmiteAlcance()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.ListarPasajerosAsync(0, 2, 3, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.ListarPasajerosAsync(42, 2, 0, CancellationToken.None));
        await Caso.ListarPasajerosAsync(42, 2, 3, CancellationToken.None);
        Assert.Equal((42, 2, 3), _falso.ListadoPasajeros);
    }

    [Fact]
    public async Task CuposYEliminacionValidanEntradaYTransfierenAlcance()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.AgregarCuposAsync(42, 2, 3, 7, 0, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.AgregarCuposAsync(42, 2, 3, 7, 1000, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.EliminarPasajeroAsync(42, 0, 2, 3, CancellationToken.None));
        await Caso.AgregarCuposAsync(42, 2, 3, 7, 3, CancellationToken.None);
        await Caso.EliminarPasajeroAsync(42, 9, 2, 3, CancellationToken.None);
        Assert.Equal((42, 2, 3, 7, 3), _falso.Cupos);
        Assert.Equal((42, 9, 2, 3), _falso.PasajeroEliminado);
    }

    [Fact]
    public async Task GuardarPasajeroValidaYNormalizaFechas()
    {
        var datos = new GuardarPasajeroSolicitudDto(42, 0, 9, 2, 3, 7,
            new DateTime(2026, 11, 10, 12, 0, 0), new DateTime(2026, 11, 11, 13, 0, 0));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.GuardarPasajeroAsync(datos with { UsuarioPasajeroId = 0 }, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.GuardarPasajeroAsync(datos with { FechaCheckOut = datos.FechaCheckIn.AddDays(-1) }, CancellationToken.None));
        Assert.Equal(9, await Caso.GuardarPasajeroAsync(datos, CancellationToken.None));
        Assert.Equal(datos.FechaCheckIn.Date, _falso.PasajeroGuardado!.FechaCheckIn);
        Assert.Equal(datos.FechaCheckOut!.Value.Date, _falso.PasajeroGuardado.FechaCheckOut);
    }

    [Fact]
    public async Task GeneracionValidaSesionYTransmiteAlcance()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.GenerarReservasAsync(42, 2, 0, 7, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.GenerarReservasAsync(0, 2, 3, 7, CancellationToken.None));
        Assert.Equal(99, await Caso.GenerarReservasAsync(42, 2, 3, 7, CancellationToken.None));
        Assert.Equal((42, 2, 3, 7), _falso.Generacion);
    }

    [Fact]
    public async Task AsignacionValidaIdentificadoresYTransmiteAlcance()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.AsignarHabitacionAsync(42, 9, 0, 2, 3, 7, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => Caso.AsignarHabitacionAsync(42, 9, 10, 2, 0, 7, CancellationToken.None));
        await Caso.AsignarHabitacionAsync(42, 9, 10, 2, 3, 7, CancellationToken.None);
        Assert.Equal((42, 9, 10, 2, 3, 7), _falso.Asignacion);
    }

    private static DatosSolicitudDto Crear() => new(
        0, 2, 3, 7, new DateTime(2026, 11, 10), new DateTime(2026, 11, 11),
        null, 4, null, false, 8, null, null, null, "contacto@example.com",
        null, null, null, null, null, false);

    private sealed class Falso : IConsultaSolicitudAlojamiento, IComandoSolicitudAlojamiento
    {
        public FiltroSolicitudesDto? Filtro { get; private set; }
        public DatosSolicitudDto? Datos { get; private set; }
        public int Busquedas { get; private set; }
        public int Guardados { get; private set; }
        public (int, int, int, int) Anulacion { get; private set; }
        public (int, int, int) ListadoPasajeros { get; private set; }
        public (int, int, int, int, int) Cupos { get; private set; }
        public (int, int, int, int) PasajeroEliminado { get; private set; }
        public GuardarPasajeroSolicitudDto? PasajeroGuardado { get; private set; }
        public (int, int, int, int) Generacion { get; private set; }
        public (int, int, int, int, int, int) Asignacion { get; private set; }

        public Task<IReadOnlyList<SolicitudListadaDto>> BuscarAsync(FiltroSolicitudesDto filtro, CancellationToken cancellationToken)
        {
            Busquedas++;
            Filtro = filtro;
            return Task.FromResult<IReadOnlyList<SolicitudListadaDto>>([]);
        }

        public Task<SolicitudDetalleDto?> ObtenerAsync(int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken) =>
            Task.FromResult<SolicitudDetalleDto?>(null);

        public Task<IReadOnlyList<PasajeroSolicitudDto>> ListarPasajerosAsync(
            int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
        {
            ListadoPasajeros = (id, empresaMandanteId, campamentoId);
            return Task.FromResult<IReadOnlyList<PasajeroSolicitudDto>>([]);
        }

        public Task<IReadOnlyList<HabitacionDisponibleSolicitudDto>> HabitacionesDisponiblesAsync(
            int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<HabitacionDisponibleSolicitudDto>>([]);

        public Task<CatalogoSolicitudDto> CatalogoAsync(int empresaMandanteId, int grupoId, CancellationToken cancellationToken) =>
            Task.FromResult(new CatalogoSolicitudDto([], [], [], []));

        public Task<CatalogoOrganizacionSolicitudDto> OrganizacionAsync(int empresaMandanteId, int unidadId, int divisionId, CancellationToken cancellationToken) =>
            Task.FromResult(new CatalogoOrganizacionSolicitudDto([], [], []));

        public Task<int> GuardarAsync(DatosSolicitudDto solicitud, CancellationToken cancellationToken)
        {
            Guardados++;
            Datos = solicitud;
            return Task.FromResult(42);
        }

        public Task AgregarCuposAsync(int id, int empresa, int campamento, int usuario, int cantidad, CancellationToken cancellationToken)
        {
            Cupos = (id, empresa, campamento, usuario, cantidad);
            return Task.CompletedTask;
        }

        public Task<int> GuardarPasajeroAsync(GuardarPasajeroSolicitudDto pasajero, CancellationToken cancellationToken)
        {
            PasajeroGuardado = pasajero;
            return Task.FromResult(9);
        }

        public Task<int> GenerarReservasAsync(int id, int empresa, int campamento, int usuario, CancellationToken cancellationToken)
        {
            Generacion = (id, empresa, campamento, usuario);
            return Task.FromResult(99);
        }

        public Task AsignarHabitacionAsync(int id, int pasajero, int habitacion, int empresa, int campamento, int usuario, CancellationToken cancellationToken)
        {
            Asignacion = (id, pasajero, habitacion, empresa, campamento, usuario);
            return Task.CompletedTask;
        }

        public Task EliminarPasajeroAsync(int id, int pasajero, int empresa, int campamento, CancellationToken cancellationToken)
        {
            PasajeroEliminado = (id, pasajero, empresa, campamento);
            return Task.CompletedTask;
        }

        public Task AnularAsync(int id, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
        {
            Anulacion = (id, empresaMandanteId, campamentoId, usuarioId);
            return Task.CompletedTask;
        }
    }
}
