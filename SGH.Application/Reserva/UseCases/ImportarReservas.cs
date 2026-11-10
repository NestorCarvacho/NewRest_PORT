using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using SGH.Application.Reserva.DTOs;
using SGH.Application.Reserva.Interfaces;
using SGH.Domain.Reserva.Interfaces;

namespace SGH.Application.Reserva.UseCases;

public sealed class ImportarReservas : IImportarReservas
{
    private readonly IPlanillaReservaExcel _planilla;
    private readonly IObtenerCatalogoReserva _catalogo;
    private readonly ICrearReserva _crear;
    private readonly ILogger<ImportarReservas> _logger;

    public ImportarReservas(
        IPlanillaReservaExcel planilla,
        IObtenerCatalogoReserva catalogo,
        ICrearReserva crear,
        ILogger<ImportarReservas> logger)
    {
        _planilla = planilla;
        _catalogo = catalogo;
        _crear = crear;
        _logger = logger;
    }

    public async Task<ResultadoImportacionReservas> EjecutarAsync(
        Stream archivo,
        int empresaMandanteId,
        int campamentoId,
        int usuarioId,
        CancellationToken cancellationToken)
    {
        var filas = _planilla.Leer(archivo);
        if (filas.Count == 0)
            return new ResultadoImportacionReservas(0, [], "La planilla no contiene reservas.");

        if (usuarioId <= 0)
            return new ResultadoImportacionReservas(filas.Count, [], "La sesión no tiene un usuario.");

        var resultadoCatalogo = await _catalogo.EjecutarAsync(
            empresaMandanteId, campamentoId, cancellationToken);
        if (resultadoCatalogo is ResultadoCatalogoReserva.Rechazado rechazado)
            return new ResultadoImportacionReservas(filas.Count, [], rechazado.Mensaje);
        if (resultadoCatalogo is not ResultadoCatalogoReserva.Listo listo)
            return new ResultadoImportacionReservas(
                filas.Count, [], "No fue posible consultar los grupos y turnos disponibles.");

        var resultados = new List<ResultadoFilaImportacionReserva>(filas.Count);
        foreach (var fila in filas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var errores = fila.Errores.ToList();
            var grupo = listo.Catalogo.Grupos.FirstOrDefault(
                opcion => Normalizar(opcion.Etiqueta) == Normalizar(fila.GrupoEmpresa));
            var turno = listo.Catalogo.Turnos.FirstOrDefault(
                opcion => Normalizar(opcion.Etiqueta) == Normalizar(fila.Turno));

            if (string.IsNullOrWhiteSpace(fila.GrupoEmpresa))
                errores.Add("Grupo Empresa es obligatorio.");
            else if (grupo is null)
                errores.Add("Grupo Empresa no existe en el catálogo del campamento.");

            if (string.IsNullOrWhiteSpace(fila.Turno))
                errores.Add("Turno es obligatorio.");
            else if (turno is null)
                errores.Add("Turno no existe en el catálogo de la empresa.");

            if (fila.FechaReserva is null)
                errores.Add("Fecha Reserva es obligatoria o no tiene un formato válido.");
            if (fila.FechaLlegada is null)
                errores.Add("Fecha Llegada es obligatoria o no tiene un formato válido.");

            if (errores.Count > 0 || grupo is null || turno is null
                || fila.FechaReserva is null || fila.FechaLlegada is null)
            {
                resultados.Add(new ResultadoFilaImportacionReserva(
                    fila.NumeroFila, null, string.Join(" ", errores)));
                continue;
            }

            try
            {
                var resultado = await _crear.EjecutarAsync(new SolicitudCrearReserva(
                    empresaMandanteId,
                    campamentoId,
                    usuarioId,
                    grupo.Id,
                    turno.Id,
                    fila.FechaReserva.Value,
                    fila.FechaLlegada.Value,
                    fila.FechaSalida,
                    fila.Observacion), cancellationToken);

                resultados.Add(resultado switch
                {
                    ResultadoAltaReserva.Creada creada =>
                        new ResultadoFilaImportacionReserva(fila.NumeroFila, creada.Id, null),
                    ResultadoAltaReserva.Rechazada rechazada =>
                        new ResultadoFilaImportacionReserva(fila.NumeroFila, null, rechazada.Mensaje),
                    _ => new ResultadoFilaImportacionReserva(
                        fila.NumeroFila, null, "No fue posible crear esta reserva.")
                });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Falló la importación de la fila {NumeroFila}.", fila.NumeroFila);
                resultados.Add(new ResultadoFilaImportacionReserva(
                    fila.NumeroFila, null, "No fue posible crear esta reserva."));
            }
        }

        return new ResultadoImportacionReservas(filas.Count, resultados, null);
    }

    private static string Normalizar(string valor)
    {
        var texto = valor.Trim().Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(texto.Length);
        foreach (var caracter in texto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                resultado.Append(char.ToUpperInvariant(caracter));
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }
}
