using SGH.Application.Reserva.DTOs;
using SGH.Application.Reserva.Interfaces;
using SGH.Application.Sesion;
using SGH.Domain.Reserva;
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
