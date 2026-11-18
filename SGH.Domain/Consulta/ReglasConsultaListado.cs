namespace SGH.Domain.Consulta;

/// <summary>Límites de listados para no escanear toda HOTELDB sin rango de fechas.</summary>
public static class ReglasConsultaListado
{
    public const int DiasPorDefecto = 30;
    public const int MaxFilas = 5000;

    public static (DateTime Desde, DateTime Hasta) NormalizarRango(DateTime? desde, DateTime? hasta)
    {
        var hoy = DateTime.Today;
        var desdeEfectivo = desde ?? (hasta.HasValue ? hasta.Value.AddDays(-DiasPorDefecto) : hoy.AddDays(-DiasPorDefecto));
        var hastaEfectivo = hasta ?? hoy;
        if (desdeEfectivo > hastaEfectivo)
            (desdeEfectivo, hastaEfectivo) = (hastaEfectivo, desdeEfectivo);

        var maxDias = 366;
        if ((hastaEfectivo - desdeEfectivo).TotalDays > maxDias)
            desdeEfectivo = hastaEfectivo.AddDays(-maxDias);

        return (desdeEfectivo, hastaEfectivo);
    }

    /// <summary>Recorta el listado y marca si superó el tope permitido para la UI.</summary>
    public static (IReadOnlyList<T> Filas, bool Truncado) AplicarTopeFilas<T>(IReadOnlyList<T> filas)
    {
        if (filas.Count <= MaxFilas)
            return (filas, false);

        var recorte = filas.Take(MaxFilas).ToList();
        return (recorte, true);
    }

    public static bool RangoReservaVacío(
        DateTime? fechaReserva,
        DateTime? fechaReservaDesde,
        DateTime? fechaReservaHasta,
        DateTime? fechaLlegada,
        DateTime? fechaSalida,
        DateTime? checkIn,
        DateTime? checkOut) =>
        fechaReserva is null
        && fechaReservaDesde is null
        && fechaReservaHasta is null
        && fechaLlegada is null
        && fechaSalida is null
        && checkIn is null
        && checkOut is null;
}