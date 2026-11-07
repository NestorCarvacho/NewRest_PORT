using SGH.Application.Reserva.DTOs;
using SGH.Application.Reserva.Interfaces;
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

    public async Task<IReadOnlyList<ReservaFilaDto>> EjecutarAsync(SolicitudBuscarReservas solicitud, CancellationToken cancellationToken)
    {
        var filas = await _consulta.BuscarAsync(new FiltroReserva(
            solicitud.CampamentoId,
            solicitud.Id,
            solicitud.AreaId,
            string.IsNullOrWhiteSpace(solicitud.Origen) ? "0" : solicitud.Origen,
            solicitud.SolicitudId,
            string.IsNullOrWhiteSpace(solicitud.Estado) ? "0" : solicitud.Estado,
            solicitud.FechaReserva,
            solicitud.FechaLlegada,
            solicitud.FechaSalida,
            solicitud.CheckIn,
            solicitud.CheckOut,
            solicitud.PasajeroId,
            solicitud.HabitacionId,
            string.IsNullOrWhiteSpace(solicitud.TieneAgua) ? "0" : solicitud.TieneAgua), cancellationToken);

        return filas.Select(fila => new ReservaFilaDto(
            fila.Id,
            fila.GrupoEmpresa,
            fila.Pasajero,
            fila.Habitacion,
            fila.Estado,
            fila.EstadoNumero,
            fila.Llegada,
            fila.Salida,
            fila.CheckIn,
            fila.CheckOut)).ToList();
    }
}

public sealed class ObtenerCatalogoReserva : IObtenerCatalogoReserva
{
    private readonly IConsultaReserva _consulta;

    public ObtenerCatalogoReserva(IConsultaReserva consulta)
    {
        _consulta = consulta;
    }

    public async Task<CatalogoReservaDto> EjecutarAsync(int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        var catalogo = await _consulta.ObtenerCatalogoAsync(empresaMandanteId, campamentoId, cancellationToken);
        return new CatalogoReservaDto(
            catalogo.Grupos.Select(opcion => new OpcionDto(opcion.Id, opcion.Etiqueta)).ToList(),
            catalogo.Origenes.Select(opcion => new OpcionTextoDto(opcion.Id, opcion.Etiqueta)).ToList(),
            catalogo.Pasajeros.Select(opcion => new OpcionDto(opcion.Id, opcion.Etiqueta)).ToList(),
            catalogo.Habitaciones.Select(opcion => new OpcionDto(opcion.Id, opcion.Etiqueta)).ToList(),
            catalogo.Turnos.Select(turno => new TurnoDto(turno.Id, turno.Nombre, turno.TipoTurno)).ToList());
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

        if (solicitud.CampamentoId <= 0 || solicitud.EmpresaMandanteId <= 0)
            return new ResultadoAltaReserva.Rechazada("La sesión no tiene un campamento.");

        var mensaje = ReglasReserva.ValidarAlta(solicitud.AreaId, solicitud.TurnoId, solicitud.FechaLlegada, solicitud.FechaSalida);
        if (mensaje is not null)
            return new ResultadoAltaReserva.Rechazada(mensaje);

        if (solicitud.Observacion is { Length: > 200 })
            return new ResultadoAltaReserva.Rechazada("La observación admite hasta 200 caracteres.");

        var id = await _comando.CrearAsync(
            solicitud.EmpresaMandanteId,
            solicitud.CampamentoId,
            solicitud.AreaId,
            solicitud.FechaReserva.Date,
            solicitud.FechaLlegada.Date,
            solicitud.FechaSalida?.Date,
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

    public RegistrarMovimientoReserva(IComandoReserva comando)
    {
        _comando = comando;
    }

    public async Task<ResultadoMovimientoReserva> EjecutarAsync(SolicitudMovimientoReserva solicitud, CancellationToken cancellationToken)
    {
        if (solicitud.UsuarioId <= 0)
            return new ResultadoMovimientoReserva.Rechazado("La sesión no tiene un usuario.");

        var mensaje = ReglasReserva.ValidarMovimiento(solicitud.ReservaId, solicitud.Fecha, solicitud.Operacion);
        if (mensaje is not null)
            return new ResultadoMovimientoReserva.Rechazado(mensaje);

        await _comando.RegistrarMovimientoAsync(solicitud.ReservaId, solicitud.Fecha, solicitud.UsuarioId, solicitud.Operacion, cancellationToken);
        return new ResultadoMovimientoReserva.Registrado();
    }
}
