using SGH.Domain.Habitacion.Interfaces;

namespace SGH.Application.Habitacion;

/// <summary>Valores requeridos por HAB_GuardaHabitacion cuando la UI no los envía.</summary>
public static class HabitacionLegadoDefaults
{
    internal static async Task<DatosGuardarHabitacion> CompletarAsync(
        DatosGuardarHabitacion datos,
        int campamentoId,
        int empresaMandanteId,
        IConsultaHabitacion consulta,
        CancellationToken cancellationToken)
    {
        if (datos.Id > 0)
        {
            var actual = await consulta.ObtenerAsync(datos.Id, campamentoId, cancellationToken);
            if (actual is not null)
                datos = FusionarEdicion(datos, actual);
        }

        var tipo = datos.TipoInstalacionId;
        var standard = datos.StandardHabitacionId;
        var estado = datos.Estado;

        if (standard <= 0 || EsEstadoNoLegado(estado))
        {
            var catalogo = await consulta.CatalogoAsync(campamentoId, empresaMandanteId, cancellationToken);
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

        var personasMin = datos.PersonasCantidadMin;
        var personasMax = datos.PersonasCantidadMax;
        if (personasMin <= 0)
            personasMin = 1;
        if (personasMax <= 0)
            personasMax = Math.Max(datos.CantidadCamas, 1);

        return datos with
        {
            TipoInstalacionId = tipo,
            StandardHabitacionId = standard,
            Estado = estado,
            PersonasCantidadMin = personasMin,
            PersonasCantidadMax = personasMax
        };
    }

    public static DatosGuardarHabitacion FusionarEdicion(
        DatosGuardarHabitacion datos, HabitacionDetalle actual)
    {
        var camas = datos.CantidadCamas > 0 ? datos.CantidadCamas : actual.CantidadCamas;
        var personasMin = actual.PersonasCantidadMin > 0 ? actual.PersonasCantidadMin : 1;
        var personasMax = actual.PersonasCantidadMax > 0 ? actual.PersonasCantidadMax : camas;
        if (personasMax < camas)
            personasMax = camas;

        var minutosMin = datos.UsoPorTurnos
            ? (datos.MinutosMin > 0 ? datos.MinutosMin : actual.MinutosMin)
            : actual.MinutosMin;
        var minutosMax = datos.UsoPorTurnos
            ? (datos.MinutosMax > 0 ? datos.MinutosMax : actual.MinutosMax)
            : actual.MinutosMax;
        var higiene = datos.TiempoHigienizacionMin > 0
            ? datos.TiempoHigienizacionMin
            : actual.TiempoHigienizacionMin;

        return new DatosGuardarHabitacion(
            actual.Id,
            datos.ModuloId > 0 ? datos.ModuloId : actual.ModuloId,
            string.IsNullOrWhiteSpace(datos.Nombre) ? actual.Nombre : datos.Nombre,
            datos.IsActive,
            camas,
            string.IsNullOrWhiteSpace(datos.Sexo) ? (actual.Sexo ?? "M") : datos.Sexo,
            datos.TipoInstalacionId > 0 ? datos.TipoInstalacionId : actual.TipoInstalacionId,
            datos.StandardHabitacionId > 0 ? datos.StandardHabitacionId : actual.StandardHabitacionId,
            string.IsNullOrWhiteSpace(datos.Estado) ? actual.Estado : datos.Estado,
            datos.ObservacionBloqueo ?? actual.ObservacionBloqueo,
            actual.M2,
            datos.IsReservable,
            personasMin,
            personasMax,
            minutosMin,
            minutosMax,
            actual.TieneServiciosAlimentacion,
            datos.UsoPorTurnos,
            datos.UsoExclusivo,
            datos.BanoId ?? actual.BanoId,
            higiene,
            datos.UsuarioId);
    }

    private static bool EsEstadoNoLegado(string? estado) =>
        string.IsNullOrWhiteSpace(estado)
        || string.Equals(estado, "ACTIVO", StringComparison.OrdinalIgnoreCase)
        || string.Equals(estado, "INACTIVO", StringComparison.OrdinalIgnoreCase);
}