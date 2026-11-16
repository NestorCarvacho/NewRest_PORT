using SGH.Domain.Habitacion.Interfaces;

namespace SGH.Application.Habitacion;

/// <summary>Valores requeridos por HAB_GuardaHabitacion cuando la UI no los envía.</summary>
internal static class HabitacionLegadoDefaults
{
    internal static async Task<DatosGuardarHabitacion> CompletarAsync(
        DatosGuardarHabitacion datos,
        int campamentoId,
        int empresaMandanteId,
        IConsultaHabitacion consulta,
        CancellationToken cancellationToken)
    {
        var tipo = datos.TipoInstalacionId;
        var standard = datos.StandardHabitacionId;
        var estado = datos.Estado;

        if (datos.Id > 0 && (tipo <= 0 || standard <= 0))
        {
            var actual = await consulta.ObtenerAsync(datos.Id, campamentoId, cancellationToken);
            if (actual is not null)
            {
                if (tipo <= 0)
                    tipo = actual.TipoInstalacionId;
                if (standard <= 0)
                    standard = actual.StandardHabitacionId;
                if (string.IsNullOrWhiteSpace(estado))
                    estado = actual.Estado;
            }
        }

        if (tipo <= 0 || standard <= 0 || EsEstadoNoLegado(estado))
        {
            var catalogo = await consulta.CatalogoAsync(campamentoId, empresaMandanteId, cancellationToken);
            if (tipo <= 0 && catalogo.TiposInstalacion.Count > 0)
                tipo = catalogo.TiposInstalacion[0].Id;
            if (standard <= 0 && catalogo.Standards.Count > 0)
                standard = catalogo.Standards[0].Id;
            if (EsEstadoNoLegado(estado))
            {
                var candidato = catalogo.EstadosHabitacion.FirstOrDefault(e =>
                    e.Etiqueta.Contains("Dispon", StringComparison.OrdinalIgnoreCase))
                    ?? catalogo.EstadosHabitacion.FirstOrDefault();
                estado = candidato?.Codigo ?? "D";
            }
        }

        if (string.IsNullOrWhiteSpace(estado))
            estado = "D";

        return datos with
        {
            TipoInstalacionId = tipo,
            StandardHabitacionId = standard,
            Estado = estado
        };
    }

    private static bool EsEstadoNoLegado(string? estado) =>
        string.IsNullOrWhiteSpace(estado)
        || string.Equals(estado, "ACTIVO", StringComparison.OrdinalIgnoreCase)
        || string.Equals(estado, "INACTIVO", StringComparison.OrdinalIgnoreCase);
}