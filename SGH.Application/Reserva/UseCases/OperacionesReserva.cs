using SGH.Application.Reserva.DTOs;
using SGH.Application.Reserva.Interfaces;
using SGH.Application.Sesion;
using SGH.Domain.Reserva;
using SGH.Domain.Reserva.Entities;
using SGH.Domain.Reserva.Interfaces;

namespace SGH.Application.Reserva.UseCases;

public sealed class BuscarReservas : IBuscarReservas
{
    private readonly IConsultaReserva _consulta;

    public BuscarReservas(IConsultaReserva consulta)
    {
        _consulta = consulta;
    }

    public async Task<ResultadoBusquedaReservas> EjecutarAsync(SolicitudBuscarReservas solicitud, CancellationToken cancellationToken)
    {
        var alcanceBusqueda = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (alcanceBusqueda is not null)
            return new ResultadoBusquedaReservas.Rechazada(alcanceBusqueda);

        var filas = await _consulta.BuscarAsync(new FiltroReserva(
            solicitud.CampamentoId,
            solicitud.Id,
            solicitud.AreaId,
            string.IsNullOrWhiteSpace(solicitud.Origen) ? "0" : solicitud.Origen,
            solicitud.SolicitudId,
            string.IsNullOrWhiteSpace(solicitud.Estado) ? "0" : solicitud.Estado,
            solicitud.FechaReserva,
            solicitud.FechaReservaDesde,
            solicitud.FechaReservaHasta,
            solicitud.FechaLlegada,
            solicitud.FechaSalida,
            solicitud.CheckIn,
            solicitud.CheckOut,
            solicitud.PasajeroId,
            solicitud.HabitacionId,
            string.IsNullOrWhiteSpace(solicitud.TieneAgua) ? "0" : solicitud.TieneAgua), cancellationToken);

        return new ResultadoBusquedaReservas.Lista(filas.Select(fila => new ReservaFilaDto(
            fila.Id,
            fila.AreaId,
            fila.PasajeroId,
            fila.HabitacionId,
            fila.TurnoId,
            fila.GrupoEmpresa,
            fila.Pasajero,
            fila.Habitacion,
            fila.Estado,
            fila.EstadoNumero,
            fila.FechaReserva,
            fila.Llegada,
            fila.Salida,
            fila.CheckIn,
            fila.CheckOut)).ToList());
    }
}

public sealed class ObtenerCatalogoReserva : IObtenerCatalogoReserva
{
    private readonly IConsultaReserva _consulta;

    public ObtenerCatalogoReserva(IConsultaReserva consulta)
    {
        _consulta = consulta;
    }

    public async Task<ResultadoCatalogoReserva> EjecutarAsync(int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        var alcanceCatalogo = AlcanceSesion.ValidarAlcance(empresaMandanteId, campamentoId);
        if (alcanceCatalogo is not null)
            return new ResultadoCatalogoReserva.Rechazado(alcanceCatalogo);

        var catalogo = await _consulta.ObtenerCatalogoAsync(empresaMandanteId, campamentoId, cancellationToken);
        return new ResultadoCatalogoReserva.Listo(new CatalogoReservaDto(
            catalogo.Grupos.Select(opcion => new OpcionDto(opcion.Id, opcion.Etiqueta)).ToList(),
            catalogo.Origenes.Select(opcion => new OpcionTextoDto(opcion.Id, opcion.Etiqueta)).ToList(),
            catalogo.Pasajeros.Select(opcion => new OpcionDto(opcion.Id, opcion.Etiqueta)).ToList(),
            catalogo.Habitaciones.Select(opcion => new OpcionDto(opcion.Id, opcion.Etiqueta)).ToList(),
            catalogo.Turnos.Select(turno => new TurnoDto(turno.Id, turno.Nombre, turno.TipoTurno)).ToList()));
    }
}

public sealed class CrearReserva : ICrearReserva
{
    private readonly IComandoReserva _comando;

    public CrearReserva(IComandoReserva comando)
    {
        _comando = comando;
    }

    public async Task<ResultadoAltaReserva> EjecutarAsync(SolicitudCrearReserva solicitud, CancellationToken cancellationToken)
    {
        if (solicitud.UsuarioId <= 0)
            return new ResultadoAltaReserva.Rechazada("La sesión no tiene un usuario.");

        var alcanceAlta = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (alcanceAlta is not null)
            return new ResultadoAltaReserva.Rechazada(alcanceAlta);

        var mensaje = ReglasReserva.ValidarAlta(solicitud.AreaId, solicitud.TurnoId, solicitud.FechaLlegada, solicitud.FechaSalida);
        if (mensaje is not null)
            return new ResultadoAltaReserva.Rechazada(mensaje);

        if (solicitud.Observacion is { Length: > 200 })
            return new ResultadoAltaReserva.Rechazada("La observación admite hasta 200 caracteres.");

        var id = await _comando.CrearAsync(
            solicitud.EmpresaMandanteId,
            solicitud.CampamentoId,
            solicitud.AreaId,
            solicitud.FechaReserva,
            solicitud.FechaLlegada,
            solicitud.FechaSalida,
            solicitud.Observacion,
            solicitud.TurnoId,
            solicitud.UsuarioId,
            cancellationToken);

        return new ResultadoAltaReserva.Creada(id);
    }
}

public sealed class RegistrarMovimientoReserva : IRegistrarMovimientoReserva
{
    private readonly IComandoReserva _comando;
    private readonly IConsultaReserva _consulta;

    public RegistrarMovimientoReserva(IComandoReserva comando, IConsultaReserva consulta)
    {
        _comando = comando;
        _consulta = consulta;
    }

    public async Task<ResultadoMovimientoReserva> EjecutarAsync(SolicitudMovimientoReserva solicitud, CancellationToken cancellationToken)
    {
        if (solicitud.UsuarioId <= 0)
            return new ResultadoMovimientoReserva.Rechazado("La sesión no tiene un usuario.");

        var alcanceMovimiento = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (alcanceMovimiento is not null)
            return new ResultadoMovimientoReserva.Rechazado(alcanceMovimiento);

        var mensaje = ReglasReserva.ValidarMovimiento(solicitud.ReservaId, solicitud.Fecha, solicitud.Operacion);
        if (mensaje is not null)
            return new ResultadoMovimientoReserva.Rechazado(mensaje);

        var propia = await _consulta.ReservaEstaEnCampamentoAsync(solicitud.ReservaId, solicitud.CampamentoId, cancellationToken);
        if (!propia)
            return new ResultadoMovimientoReserva.Rechazado(ReglasReserva.ReservaAjena);

        await _comando.RegistrarMovimientoAsync(
            solicitud.ReservaId,
            solicitud.Fecha,
            solicitud.UsuarioId,
            solicitud.Operacion,
            solicitud.CampamentoId,
            cancellationToken);
        return new ResultadoMovimientoReserva.Registrado();
    }

    public sealed class ActualizarReserva : IActualizarReserva
    {
        private readonly IComandoReserva _comando;
        private readonly IConsultaReserva _consulta;

        public ActualizarReserva(IComandoReserva comando, IConsultaReserva consulta)
        {
            _comando = comando;
            _consulta = consulta;
        }

        public async Task<ResultadoActualizacionReserva> EjecutarAsync(
            SolicitudActualizarReserva solicitud, CancellationToken cancellationToken)
        {
            if (solicitud.UsuarioId <= 0)
                return new ResultadoActualizacionReserva.Rechazada("La sesión no tiene un usuario.");

            var alcance = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
            if (alcance is not null)
                return new ResultadoActualizacionReserva.Rechazada(alcance);

            var mensaje = ReglasReserva.ValidarAlta(
                solicitud.AreaId, solicitud.TurnoId, solicitud.FechaLlegada, solicitud.FechaSalida);
            if (mensaje is not null)
                return new ResultadoActualizacionReserva.Rechazada(mensaje);
            if (solicitud.ReservaId <= 0)
                return new ResultadoActualizacionReserva.Rechazada("Seleccione una reserva.");
            if (solicitud.Estado is not ("P" or "A" or "F" or "N"))
                return new ResultadoActualizacionReserva.Rechazada("El estado de la reserva no es válido.");

            var propia = await _consulta.ReservaEstaEnCampamentoAsync(solicitud.ReservaId, solicitud.CampamentoId, cancellationToken);
            if (!propia)
                return new ResultadoActualizacionReserva.Rechazada(ReglasReserva.ReservaAjena);

            await _comando.ActualizarAsync(
                solicitud.ReservaId, solicitud.EmpresaMandanteId, solicitud.CampamentoId, solicitud.AreaId,
                solicitud.FechaReserva, solicitud.FechaLlegada, solicitud.FechaSalida,
                solicitud.Observacion, solicitud.TurnoId, solicitud.PasajeroId, solicitud.HabitacionId,
                solicitud.Estado, solicitud.TieneAgua, solicitud.UsuarioId, cancellationToken);
            return new ResultadoActualizacionReserva.Actualizada();
        }
    }
}

public sealed class AnularReserva : IAnularReserva
{
    private readonly IComandoReserva _comando;
    private readonly IConsultaReserva _consulta;

    public AnularReserva(IComandoReserva comando, IConsultaReserva consulta)
    {
        _comando = comando;
        _consulta = consulta;
    }

    public async Task<ResultadoAnularReserva> EjecutarAsync(SolicitudAnularReserva solicitud, CancellationToken cancellationToken)
    {
        if (solicitud.UsuarioId <= 0)
            return new ResultadoAnularReserva.Rechazada("La sesión no tiene un usuario.");

        var alcance = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (alcance is not null)
            return new ResultadoAnularReserva.Rechazada(alcance);

        var propia = await _consulta.ReservaEstaEnCampamentoAsync(solicitud.ReservaId, solicitud.CampamentoId, cancellationToken);
        if (!propia)
            return new ResultadoAnularReserva.Rechazada(ReglasReserva.ReservaAjena);

        var fila = await ObtenerReservaAsync(solicitud.ReservaId, solicitud.CampamentoId, cancellationToken);
        if (fila is null)
            return new ResultadoAnularReserva.Rechazada(ReglasReserva.ReservaAjena);

        var mensaje = ReglasReserva.ValidarAnular(solicitud.ReservaId, fila.EstadoNumero);
        if (mensaje is not null)
            return new ResultadoAnularReserva.Rechazada(mensaje);

        await _comando.AnularAsync(
            solicitud.ReservaId, solicitud.EmpresaMandanteId, solicitud.CampamentoId, solicitud.UsuarioId, cancellationToken);
        return new ResultadoAnularReserva.Anulada();
    }

    private async Task<ReservaListada?> ObtenerReservaAsync(int reservaId, int campamentoId, CancellationToken cancellationToken)
    {
        var filas = await _consulta.BuscarAsync(new FiltroReserva(
            campamentoId, reservaId, 0, "0", 0, "0", null, null, null, null, null, null, null, 0, 0, "0"), cancellationToken);
        return filas.FirstOrDefault(f => f.Id == reservaId);
    }
}

public sealed class FinalizarReservaCheckout : IFinalizarReserva
{
    private readonly IComandoReserva _comando;
    private readonly IConsultaReserva _consulta;

    public FinalizarReservaCheckout(IComandoReserva comando, IConsultaReserva consulta)
    {
        _comando = comando;
        _consulta = consulta;
    }

    public async Task<ResultadoFinalizarReserva> EjecutarAsync(SolicitudFinalizarReserva solicitud, CancellationToken cancellationToken)
    {
        if (solicitud.UsuarioId <= 0)
            return new ResultadoFinalizarReserva.Rechazada("La sesión no tiene un usuario.");

        var alcance = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (alcance is not null)
            return new ResultadoFinalizarReserva.Rechazada(alcance);

        var propia = await _consulta.ReservaEstaEnCampamentoAsync(solicitud.ReservaId, solicitud.CampamentoId, cancellationToken);
        if (!propia)
            return new ResultadoFinalizarReserva.Rechazada(ReglasReserva.ReservaAjena);

        var filas = await _consulta.BuscarAsync(new FiltroReserva(
            solicitud.CampamentoId, solicitud.ReservaId, 0, "0", 0, "0", null, null, null, null, null, null, null, 0, 0, "0"), cancellationToken);
        var fila = filas.FirstOrDefault(f => f.Id == solicitud.ReservaId);
        if (fila is null)
            return new ResultadoFinalizarReserva.Rechazada(ReglasReserva.ReservaAjena);

        var mensaje = ReglasReserva.ValidarFinalizar(solicitud.ReservaId, fila.EstadoNumero, fila.CheckOut);
        if (mensaje is not null)
            return new ResultadoFinalizarReserva.Rechazada(mensaje);

        await _comando.FinalizarCheckoutAsync(
            solicitud.ReservaId, solicitud.EmpresaMandanteId, solicitud.UsuarioId, cancellationToken);
        return new ResultadoFinalizarReserva.Finalizada();
    }
}

public sealed class ReversarMovimientoReserva : IReversarMovimientoReserva
{
    private readonly IComandoReserva _comando;
    private readonly IConsultaReserva _consulta;

    public ReversarMovimientoReserva(IComandoReserva comando, IConsultaReserva consulta)
    {
        _comando = comando;
        _consulta = consulta;
    }

    public async Task<ResultadoReversarMovimientoReserva> EjecutarAsync(
        SolicitudReversarMovimientoReserva solicitud, CancellationToken cancellationToken)
    {
        if (solicitud.UsuarioId <= 0)
            return new ResultadoReversarMovimientoReserva.Rechazado("La sesión no tiene un usuario.");

        var alcance = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (alcance is not null)
            return new ResultadoReversarMovimientoReserva.Rechazado(alcance);

        var propia = await _consulta.ReservaEstaEnCampamentoAsync(solicitud.ReservaId, solicitud.CampamentoId, cancellationToken);
        if (!propia)
            return new ResultadoReversarMovimientoReserva.Rechazado(ReglasReserva.ReservaAjena);

        var filas = await _consulta.BuscarAsync(new FiltroReserva(
            solicitud.CampamentoId, solicitud.ReservaId, 0, "0", 0, "0", null, null, null, null, null, null, null, 0, 0, "0"), cancellationToken);
        var fila = filas.FirstOrDefault(f => f.Id == solicitud.ReservaId);
        if (fila is null)
            return new ResultadoReversarMovimientoReserva.Rechazado(ReglasReserva.ReservaAjena);

        var mensaje = ReglasReserva.ValidarReversarMovimiento(
            solicitud.ReservaId, fila.EstadoNumero, fila.CheckIn, fila.CheckOut);
        if (mensaje is not null)
            return new ResultadoReversarMovimientoReserva.Rechazado(mensaje);

        await _comando.ReversarMovimientoAsync(
            solicitud.ReservaId, solicitud.CampamentoId, solicitud.UsuarioId, cancellationToken);
        return new ResultadoReversarMovimientoReserva.Reversado();
    }
}

//operacion finalizar reserva
public sealed class FinalizarMasivoReserva : IFinalizarMasivoReserva
{
    private readonly IComandoReserva _comando;
    private readonly IConsultaReserva _consulta;

    public FinalizarMasivoReserva(IComandoReserva comando, IConsultaReserva consulta)
    {
        _comando = comando;
        _consulta = consulta;
    }

    public async Task<ResultadoFinalizarMasivoReserva> EjecutarAsync(SolicitudFinalizarMasivoReserva solicitud, CancellationToken cancellationToken)
    {
        if (solicitud.UsuarioId <= 0)
            return new ResultadoFinalizarMasivoReserva.Rechazado("La sesión no tiene un usuario.");

        var alcance = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (alcance is not null)
            return new ResultadoFinalizarMasivoReserva.Rechazado(alcance);

        var propia = await _consulta.ReservaEstaEnCampamentoAsync(solicitud.ReservaId, solicitud.CampamentoId, cancellationToken);
        if (!propia)
            return new ResultadoFinalizarMasivoReserva.Rechazado(ReglasReserva.ReservaAjena);

        var filas = await _consulta.BuscarAsync(new FiltroReserva(
            solicitud.CampamentoId, solicitud.ReservaId, 0, "0", 0, "0", null, null, null, null, null, null, null, 0, 0, "0"), cancellationToken);
        var fila = filas.FirstOrDefault(f => f.Id == solicitud.ReservaId);
        if (fila is null)
            return new ResultadoFinalizarMasivoReserva.Rechazado(ReglasReserva.ReservaAjena);

        var mensaje = ReglasReserva.ValidarFinalizarMasivo(
            solicitud.ReservaId, fila.EstadoNumero, fila.CheckIn, fila.CheckOut);
        if (mensaje is not null)
            return new ResultadoFinalizarMasivoReserva.Rechazado(mensaje);

        await _comando.FinalizarMasivoAsync(
            solicitud.ReservaId, solicitud.CampamentoId, solicitud.UsuarioId, cancellationToken);
        return new ResultadoFinalizarMasivoReserva.Finalizada();
    }
}
