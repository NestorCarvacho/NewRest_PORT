using SGH.Application.Censo.DTOs;
using SGH.Application.Censo.Interfaces;
using SGH.Application.Sesion;
using SGH.Domain.Censo;
using SGH.Domain.Censo.Entities;
using SGH.Domain.Censo.Interfaces;

namespace SGH.Application.Censo.UseCases;

public sealed class BuscarCensos : IBuscarCensos
{
    private readonly IConsultaCenso _consulta;

    public BuscarCensos(IConsultaCenso consulta)
    {
        _consulta = consulta;
    }

    public async Task<ResultadoBusquedaCenso> EjecutarAsync(SolicitudBuscarCensos solicitud, CancellationToken cancellationToken)
    {
        var mensaje = ReglasCenso.ValidarBusqueda(solicitud.Fecha, solicitud.Turno);
        if (mensaje is not null)
            return new ResultadoBusquedaCenso.Rechazada(mensaje);

        var alcance = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (alcance is not null)
            return new ResultadoBusquedaCenso.Rechazada(alcance);

        if (solicitud.ModuloId > 0 || solicitud.PisoId > 0 || solicitud.HabitacionId > 0)
        {
            var propio = await _consulta.FiltroEstaEnCampamentoAsync(
                solicitud.CampamentoId,
                solicitud.ModuloId,
                solicitud.PisoId,
                solicitud.HabitacionId,
                cancellationToken);
            if (!propio)
                return new ResultadoBusquedaCenso.Rechazada(ReglasCenso.FiltroAjeno);
        }

        var filas = await _consulta.BuscarAsync(new FiltroCenso(
            solicitud.EmpresaMandanteId,
            solicitud.CampamentoId,
            solicitud.ModuloId,
            solicitud.PisoId,
            solicitud.HabitacionId,
            solicitud.Fecha!.Value,
            solicitud.Turno!), cancellationToken);

        return new ResultadoBusquedaCenso.Lista(filas.Select(fila => new CensoFilaDto(
            fila.Id,
            fila.Fecha,
            fila.Modulo,
            fila.Piso,
            fila.Habitacion,
            fila.HabitacionId,
            fila.EstadoCenso,
            fila.EstadoHabitacion,
            fila.NumeroCamas,
            fila.CamasOcupadas,
            fila.CamasCheckIn,
            fila.Turno)).ToList());
    }
}

public sealed class ObtenerCatalogoCenso : IObtenerCatalogoCenso
{
    private readonly IConsultaCenso _consulta;

    public ObtenerCatalogoCenso(IConsultaCenso consulta)
    {
        _consulta = consulta;
    }

    public async Task<ResultadoCatalogoCenso> EjecutarAsync(int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        var alcance = AlcanceSesion.ValidarAlcance(empresaMandanteId, campamentoId);
        if (alcance is not null)
            return new ResultadoCatalogoCenso.Rechazado(alcance);

        var catalogo = await _consulta.ObtenerCatalogoAsync(campamentoId, cancellationToken);
        return new ResultadoCatalogoCenso.Listo(new CatalogoCensoDto(
            ADto(catalogo.Modulos),
            ADto(catalogo.Pisos),
            ADto(catalogo.Habitaciones),
            ADto(catalogo.TiposInstalacion),
            catalogo.HabitacionesQr.Select(h => new OpcionHabitacionCensoDto(
                h.Id, h.Etiqueta, h.PadreId, h.TipoInstalacionId, h.Capacidad)).ToList()));
    }

    private static List<OpcionCensoDto> ADto(IReadOnlyList<OpcionCenso> opciones) =>
        opciones.Select(opcion => new OpcionCensoDto(opcion.Id, opcion.Etiqueta, opcion.PadreId)).ToList();
}

public sealed class ObtenerHabitacionCensoQr : IObtenerHabitacionCensoQr
{
    private readonly IConsultaCenso _consulta;

    public ObtenerHabitacionCensoQr(IConsultaCenso consulta)
    {
        _consulta = consulta;
    }

    public async Task<ResultadoHabitacionCensoQr> EjecutarAsync(int campamentoId, int habitacionId, CancellationToken cancellationToken)
    {
        if (campamentoId <= 0)
            return new ResultadoHabitacionCensoQr.Rechazado(AlcanceSesion.SinCampamento);
        if (habitacionId <= 0)
            return new ResultadoHabitacionCensoQr.Rechazado("El código QR no contiene una habitación válida.");

        var detalle = await _consulta.ObtenerHabitacionQrAsync(campamentoId, habitacionId, cancellationToken);
        if (detalle is null)
            return new ResultadoHabitacionCensoQr.NoEncontrada();

        return new ResultadoHabitacionCensoQr.Listo(new HabitacionCensoQrDto(
            detalle.HabitacionId,
            detalle.Habitacion,
            detalle.Modulo,
            detalle.Piso,
            detalle.Capacidad));
    }
}

public sealed class BuscarMisCensos : IBuscarMisCensos
{
    private readonly IConsultaCenso _consulta;

    public BuscarMisCensos(IConsultaCenso consulta)
    {
        _consulta = consulta;
    }

    public async Task<ResultadoBusquedaMisCensos> EjecutarAsync(
        SolicitudBuscarMisCensos solicitud,
        CancellationToken cancellationToken)
    {
        if (solicitud.UsuarioId <= 0)
            return new ResultadoBusquedaMisCensos.Rechazada("La sesión no tiene un usuario.");

        var alcance = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (alcance is not null)
            return new ResultadoBusquedaMisCensos.Rechazada(alcance);

        var filas = await _consulta.BuscarPorUsuarioAsync(
            solicitud.EmpresaMandanteId,
            solicitud.CampamentoId,
            solicitud.Fecha.Date,
            solicitud.UsuarioId,
            cancellationToken);

        return new ResultadoBusquedaMisCensos.Lista(filas.Select(fila => new CensoUsuarioFilaDto(
            fila.Id,
            fila.Fecha,
            fila.Campamento,
            fila.Modulo,
            fila.Piso,
            fila.Habitacion,
            fila.NumeroCamas,
            fila.CamasOcupadas,
            fila.Turno,
            fila.TurnoNombre)).ToList());
    }
}

public sealed class GuardarCensos : IGuardarCensos
{
    private readonly IComandoCenso _comando;
    private readonly IConsultaCenso _consulta;

    public GuardarCensos(IComandoCenso comando, IConsultaCenso consulta)
    {
        _comando = comando;
        _consulta = consulta;
    }

    public async Task<ResultadoGuardadoCenso> EjecutarAsync(SolicitudGuardarCensos solicitud, CancellationToken cancellationToken)
    {
        if (solicitud.UsuarioId <= 0)
            return new ResultadoGuardadoCenso.Rechazado("La sesión no tiene un usuario.");

        var alcance = AlcanceSesion.ValidarAlcance(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (alcance is not null)
            return new ResultadoGuardadoCenso.Rechazado(alcance);

        if (solicitud.Filas.Count == 0)
            return new ResultadoGuardadoCenso.Rechazado("No hay censos para guardar.");

        var errores = new List<string>();
        foreach (var fila in solicitud.Filas)
        {
            if (fila.HabitacionId <= 0)
            {
                errores.Add("Una fila no tiene habitación.");
                continue;
            }

            if (!int.TryParse(fila.Cantidad, out var cantidad) || cantidad < 0)
            {
                errores.Add("La cantidad de camas no es válida.");
                continue;
            }

            var propia = await _consulta.FiltroEstaEnCampamentoAsync(
                solicitud.CampamentoId,
                0,
                0,
                fila.HabitacionId,
                cancellationToken);
            if (!propia)
            {
                errores.Add(ReglasCenso.HabitacionAjena);
                continue;
            }

            var turno = string.IsNullOrWhiteSpace(fila.Turno) ? ReglasCenso.TurnoDia : fila.Turno;
            var resultado = await _comando.GuardarAsync(
                new FilaCensoGuardar(fila.Id ?? string.Empty, fila.HabitacionId, cantidad, turno),
                solicitud.Fecha,
                solicitud.UsuarioId,
                solicitud.CampamentoId,
                cancellationToken);

            if (ReglasCenso.EsErrorDeFila(resultado.Resultado))
                errores.Add(string.IsNullOrWhiteSpace(resultado.Mensaje) ? "No se pudo guardar el censo." : resultado.Mensaje);
        }

        if (errores.Count > 0)
            return new ResultadoGuardadoCenso.Rechazado(string.Join(" ", errores.Distinct()));

        return new ResultadoGuardadoCenso.Guardado();
    }
}
