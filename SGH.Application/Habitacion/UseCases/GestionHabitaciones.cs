using SGH.Application.Habitacion.DTOs;
using SGH.Application.Habitacion.Interfaces;
using SGH.Domain.Habitacion;
using SGH.Domain.Habitacion.Interfaces;

namespace SGH.Application.Habitacion.UseCases;

public sealed class GestionHabitaciones(
    IConsultaHabitacion consulta,
    IComandoHabitacion comando,
    IConsultaBano consultaBano,
    IComandoBano comandoBano,
    IConsultaCama consultaCama,
    IComandoCama comandoCama,
    IConsultaTipoCama consultaTipoCama,
    IComandoTipoCama comandoTipoCama) : IGestionHabitaciones
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
            var banos = await consultaBano.ListarAsync(campamentoId, 0, 0, cancellationToken);
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

    public Task<IReadOnlyList<BanoListado>> ListarBanosAsync(
        int campamentoId, int soloActivos, int banoId = 0, CancellationToken cancellationToken = default) =>
        consultaBano.ListarAsync(campamentoId, soloActivos, banoId, cancellationToken);

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
        int habitacionId, int campamentoId, int soloActivas, CancellationToken cancellationToken) =>
        consultaCama.ListarPorHabitacionAsync(habitacionId, campamentoId, soloActivas, cancellationToken);

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

        if (datos.Id > 0)
        {
            var habitacionCama = await consultaCama.ObtenerHabitacionIdDeCamaAsync(datos.Id, campamentoId, cancellationToken);
            var mensajeAlcance = ReglasCama.ValidarPerteneceHabitacion(
                datos.Id, habitacionCama ?? 0, datos.HabitacionId);
            if (mensajeAlcance is not null)
                return new ResultadoGuardarCama(false, mensajeAlcance, datos.Id);

            if (!datos.IncluyeInventarioMandante)
            {
                var camas = await consultaCama.ListarPorHabitacionAsync(
                    datos.HabitacionId, campamentoId, 0, cancellationToken);
                var actual = camas.FirstOrDefault(c => c.Id == datos.Id);
                if (actual is not null)
                {
                    datos = datos with
                    {
                        FechaColchon = actual.FechaColchon,
                        ColchonObservacion = actual.ColchonObservacion,
                        FechaCambioRopa = actual.FechaCambioRopa,
                        RopaCamaObservacion = actual.RopaCamaObservacion
                    };
                }
            }
        }

        return await comandoCama.GuardarAsync(datos, campamentoId, cancellationToken);
    }

    public Task<IReadOnlyList<TipoCamaListado>> ListarTiposCamaAsync(
        int campamentoId, int soloActivos, int tipoCamaId = 0, CancellationToken cancellationToken = default) =>
        consultaTipoCama.ListarAsync(campamentoId, soloActivos, tipoCamaId, cancellationToken);

    public async Task<ResultadoGuardarTipoCama> GuardarTipoCamaAsync(
        DatosGuardarTipoCama datos, int campamentoId, CancellationToken cancellationToken)
    {
        var mensaje = ReglasTipoCama.ValidarGuardar(datos.Nombre, datos.AnchoCm, datos.LargoCm);
        if (mensaje is not null)
            return new ResultadoGuardarTipoCama(false, mensaje, datos.Id);

        return await comandoTipoCama.GuardarAsync(datos, campamentoId, cancellationToken);
    }

    public async Task<IReadOnlyList<ResultadoCargaCamaDto>> CargarCamasAsync(
        int campamentoId, int usuarioId, IReadOnlyList<FilaCargaCamaDto> filas, CancellationToken cancellationToken)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("Indique un usuario válido.");

        var tiposValidos = (await consultaTipoCama.ListarAsync(campamentoId, 0, 0, cancellationToken))
            .ToDictionary(t => t.Id);
        var resultados = new List<ResultadoCargaCamaDto>();

        foreach (var fila in filas)
        {
            if (string.IsNullOrWhiteSpace(fila.CodigoHabitacion))
            {
                resultados.Add(new ResultadoCargaCamaDto(fila.NumeroFila, false, "CodigoHabitacion es obligatorio."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(fila.CodigoCama))
            {
                resultados.Add(new ResultadoCargaCamaDto(fila.NumeroFila, false, "CodigoCama es obligatorio."));
                continue;
            }

            if (fila.CodigoCama.Length > 50)
            {
                resultados.Add(new ResultadoCargaCamaDto(fila.NumeroFila, false, "CodigoCama no puede superar 50 caracteres."));
                continue;
            }

            var habitacionId = await consulta.ObtenerIdPorNombreEnCampamentoAsync(
                campamentoId, fila.CodigoHabitacion, cancellationToken);
            if (habitacionId is null or <= 0)
            {
                resultados.Add(new ResultadoCargaCamaDto(
                    fila.NumeroFila, false, "No se encontró la habitación en el campamento."));
                continue;
            }

            if (fila.TipoCamaId is int tipoId)
            {
                if (!tiposValidos.TryGetValue(tipoId, out var tipo) || !tipo.IsActive)
                {
                    resultados.Add(new ResultadoCargaCamaDto(fila.NumeroFila, false, "TipoCamaId no válido para el campamento."));
                    continue;
                }
            }

            var datos = new DatosGuardarCama(
                0,
                habitacionId.Value,
                fila.CodigoCama.Trim(),
                fila.Habilitada,
                fila.Orden > 0 ? fila.Orden : 1,
                true,
                fila.TipoCamaId,
                fila.FechaColchon,
                fila.ColchonObservacion,
                fila.FechaCambioRopa,
                fila.RopaCamaObservacion,
                true,
                usuarioId);

            var guardado = await GuardarCamaAsync(datos, campamentoId, cancellationToken);
            resultados.Add(new ResultadoCargaCamaDto(
                fila.NumeroFila,
                guardado.Exito,
                guardado.Exito ? "Procesada" : (string.IsNullOrWhiteSpace(guardado.Mensaje) ? "No se pudo guardar la cama." : guardado.Mensaje)));
        }

        return resultados;
    }
}