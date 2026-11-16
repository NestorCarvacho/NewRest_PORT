using SGH.Domain.Habitacion;
using SGH.Domain.Habitacion.Interfaces;
using SGH.Application.Habitacion.Interfaces;

namespace SGH.Application.Habitacion.UseCases;

public sealed class GestionHabitaciones(
    IConsultaHabitacion consulta,
    IComandoHabitacion comando,
    IConsultaBano consultaBano,
    IComandoBano comandoBano,
    IConsultaCama consultaCama,
    IComandoCama comandoCama) : IGestionHabitaciones
{
    public Task<IReadOnlyList<HabitacionListada>> ListarAsync(
        FiltroListadoHabitacion filtro, int usuarioId, CancellationToken cancellationToken) =>
        consulta.ListarAsync(filtro, usuarioId, cancellationToken);

    public Task<HabitacionDetalle?> ObtenerAsync(int id, int campamentoId, CancellationToken cancellationToken) =>
        consulta.ObtenerAsync(id, campamentoId, cancellationToken);

    public Task<CatalogoHabitacion> CatalogoAsync(int campamentoId, int empresaMandanteId, CancellationToken cancellationToken) =>
        consulta.CatalogoAsync(campamentoId, empresaMandanteId, cancellationToken);

    public async Task<ResultadoGuardarHabitacion> GuardarAsync(
        DatosGuardarHabitacion datos, int campamentoId, int empresaMandanteId, CancellationToken cancellationToken)
    {
        datos = await HabitacionLegadoDefaults.CompletarAsync(
            datos, campamentoId, empresaMandanteId, consulta, cancellationToken);

        var minutosHig = datos.UsoPorTurnos
            ? (datos.TiempoHigienizacionMin > 0 ? datos.TiempoHigienizacionMin : datos.MinutosMin)
            : 0;
        var mensaje = ReglasHabitacion.ValidarAlta(
            datos.Nombre,
            datos.ModuloId,
            datos.CantidadCamas,
            datos.Sexo,
            datos.UsoExclusivo ? datos.CantidadCamas : datos.PersonasCantidadMin,
            datos.UsoExclusivo ? datos.CantidadCamas : datos.PersonasCantidadMax,
            datos.UsoExclusivo,
            minutosHig);
        if (mensaje is not null)
            return new ResultadoGuardarHabitacion(false, mensaje, datos.Id);

        if (datos.BanoId is > 0)
        {
            var banos = await consultaBano.ListarAsync(campamentoId, 0, cancellationToken);
            var bano = banos.FirstOrDefault(b => b.Id == datos.BanoId);
            if (bano is null || (!bano.IsActive && !string.Equals(bano.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase)))
                return new ResultadoGuardarHabitacion(false, "El baño seleccionado no es válido para el campamento.", datos.Id);
            if (!ReglasHabitacion.SexoCompatibleBano(datos.Sexo, bano.TipoSexo))
                return new ResultadoGuardarHabitacion(false, "RN-02: El sexo de la habitación no es compatible con el baño.", datos.Id);
        }

        if (datos.Id > 0)
        {
            var actual = await consulta.ObtenerAsync(datos.Id, campamentoId, cancellationToken);
            if (actual is null)
                return new ResultadoGuardarHabitacion(false, "La habitación no existe en el campamento.", datos.Id);
        }

        return await comando.GuardarAsync(datos, campamentoId, cancellationToken);
    }

    public Task<IReadOnlyList<BanoListado>> ListarBanosAsync(int campamentoId, int soloActivos, CancellationToken cancellationToken) =>
        consultaBano.ListarAsync(campamentoId, soloActivos, cancellationToken);

    public async Task<ResultadoGuardarBano> GuardarBanoAsync(
        DatosGuardarBano datos, int campamentoId, CancellationToken cancellationToken)
    {
        var mensaje = ReglasBano.ValidarGuardar(datos.CodigoNombre, datos.Ubicacion, datos.TipoSexo);
        if (mensaje is not null)
            return new ResultadoGuardarBano(false, mensaje, datos.Id);

        if (datos.Id > 0 && !datos.IsActive)
        {
            var vinculadas = await consultaBano.ListarHabitacionesActivasPorBanoAsync(
                datos.Id, campamentoId, cancellationToken);
            if (vinculadas.Count > 0)
            {
                var mensajeVinculos = ReglasBano.MensajeHabitacionesVinculadas(
                    vinculadas.Select(h => (h.Id, h.Nombre)).ToList());
                return new ResultadoGuardarBano(false, mensajeVinculos, datos.Id);
            }
        }

        return await comandoBano.GuardarAsync(datos, campamentoId, cancellationToken);
    }

    public Task<IReadOnlyList<CamaListada>> ListarCamasAsync(
        int habitacionId, int campamentoId, CancellationToken cancellationToken) =>
        consultaCama.ListarPorHabitacionAsync(habitacionId, campamentoId, cancellationToken);

    public async Task<ResultadoGuardarCama> GuardarCamaAsync(
        DatosGuardarCama datos, int campamentoId, CancellationToken cancellationToken)
    {
        if (datos.HabitacionId <= 0)
            return new ResultadoGuardarCama(false, "Seleccione una habitación.", 0);
        if (string.IsNullOrWhiteSpace(datos.CodigoCama))
            return new ResultadoGuardarCama(false, "Ingrese el código de la cama.", datos.Id);

        var habitacion = await consulta.ObtenerAsync(datos.HabitacionId, campamentoId, cancellationToken);
        if (habitacion is null)
            return new ResultadoGuardarCama(false, "La habitación no existe en el campamento.", datos.Id);

        return await comandoCama.GuardarAsync(datos, campamentoId, cancellationToken);
    }
}