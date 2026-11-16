using SGH.Domain.Habitacion;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Application.Habitacion.Interfaces;

namespace SGH.Application.Habitacion.UseCases;

public sealed class GestionHabitaciones(IConsultaHabitacion consulta, IComandoHabitacion comando) : IGestionHabitaciones
{
    public Task<IReadOnlyList<HabitacionListada>> ListarAsync(
        FiltroListadoHabitacion filtro, int usuarioId, CancellationToken cancellationToken) =>
        consulta.ListarAsync(filtro, usuarioId, cancellationToken);

    public Task<HabitacionDetalle?> ObtenerAsync(int id, int campamentoId, CancellationToken cancellationToken) =>
        consulta.ObtenerAsync(id, campamentoId, cancellationToken);

    public Task<CatalogoHabitacion> CatalogoAsync(int campamentoId, int empresaMandanteId, CancellationToken cancellationToken) =>
        consulta.CatalogoAsync(campamentoId, empresaMandanteId, cancellationToken);

    public async Task<ResultadoGuardarHabitacion> GuardarAsync(
        DatosGuardarHabitacion datos, int campamentoId, CancellationToken cancellationToken)
    {
        var mensaje = ReglasHabitacion.ValidarAlta(
            datos.Nombre,
            datos.ModuloId,
            datos.CantidadCamas,
            datos.Sexo,
            datos.UsoExclusivo ? datos.CantidadCamas : datos.PersonasCantidadMin,
            datos.UsoExclusivo ? datos.CantidadCamas : datos.PersonasCantidadMax,
            datos.UsoExclusivo,
            datos.UsoPorTurnos ? datos.MinutosMin : 0);
        if (mensaje is not null)
            return new ResultadoGuardarHabitacion(false, mensaje, datos.Id);

        if (datos.Id > 0)
        {
            var actual = await consulta.ObtenerAsync(datos.Id, campamentoId, cancellationToken);
            if (actual is null)
                return new ResultadoGuardarHabitacion(false, "La habitación no existe en el campamento.", datos.Id);
        }

        return await comando.GuardarAsync(datos, campamentoId, cancellationToken);
    }
}