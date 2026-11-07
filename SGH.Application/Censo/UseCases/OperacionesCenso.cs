using SGH.Application.Censo.DTOs;
using SGH.Application.Censo.Interfaces;
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

    public async Task<CatalogoCensoDto> EjecutarAsync(int campamentoId, CancellationToken cancellationToken)
    {
        var catalogo = await _consulta.ObtenerCatalogoAsync(campamentoId, cancellationToken);
        return new CatalogoCensoDto(
            ADto(catalogo.Modulos),
            ADto(catalogo.Pisos),
            ADto(catalogo.Habitaciones));
    }

    private static List<OpcionCensoDto> ADto(IReadOnlyList<OpcionCenso> opciones) =>
        opciones.Select(opcion => new OpcionCensoDto(opcion.Id, opcion.Etiqueta, opcion.PadreId)).ToList();
}

public sealed class GuardarCensos : IGuardarCensos
{
    private readonly IComandoCenso _comando;

    public GuardarCensos(IComandoCenso comando)
    {
        _comando = comando;
    }

    public async Task<ResultadoGuardadoCenso> EjecutarAsync(SolicitudGuardarCensos solicitud, CancellationToken cancellationToken)
    {
        if (solicitud.UsuarioId <= 0)
            return new ResultadoGuardadoCenso.Rechazado("La sesión no tiene un usuario.");

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

            var turno = string.IsNullOrWhiteSpace(fila.Turno) ? ReglasCenso.TurnoDia : fila.Turno;
            var resultado = await _comando.GuardarAsync(
                new FilaCensoGuardar(fila.Id ?? string.Empty, fila.HabitacionId, cantidad, turno),
                solicitud.Fecha,
                solicitud.UsuarioId,
                cancellationToken);

            if (ReglasCenso.EsErrorDeFila(resultado.Resultado))
                errores.Add(string.IsNullOrWhiteSpace(resultado.Mensaje) ? "No se pudo guardar el censo." : resultado.Mensaje);
        }

        if (errores.Count > 0)
            return new ResultadoGuardadoCenso.Rechazado(string.Join(" ", errores.Distinct()));

        return new ResultadoGuardadoCenso.Guardado();
    }
}
