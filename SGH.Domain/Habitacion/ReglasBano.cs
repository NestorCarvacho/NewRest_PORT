namespace SGH.Domain.Habitacion;

public static class ReglasBano
{
    public const string CodigoRequerido = "Ingrese el código del baño.";
    public const string UbicacionRequerida = "Ingrese la ubicación del baño.";
    public const string TipoSexoInvalido = "Seleccione un tipo de sexo válido para el baño (HOMBRE, MUJER, MIXTO, M o F).";
    public const string PrefijoInactivarConHabitaciones =
        "RN-01: No puede inactivar el baño mientras tenga habitaciones activas vinculadas.";

    private static readonly HashSet<string> SexosBano =
        new(StringComparer.OrdinalIgnoreCase) { "M", "F", "MIXTO", "HOMBRE", "MUJER" };

    public static string? ValidarGuardar(string? codigo, string? ubicacion, string? tipoSexo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return CodigoRequerido;
        if (string.IsNullOrWhiteSpace(ubicacion))
            return UbicacionRequerida;
        if (string.IsNullOrWhiteSpace(tipoSexo) || !SexosBano.Contains(tipoSexo.Trim()))
            return TipoSexoInvalido;
        return null;
    }

    /// <summary>RN-02: compatibilidad habitación ↔ baño (misma lógica que en SP).</summary>
    public static bool CompatibleConHabitacion(string? sexoHabitacion, string? tipoSexoBano) =>
        ReglasHabitacion.SexoCompatibleBano(sexoHabitacion, tipoSexoBano);

    public static string MensajeHabitacionesVinculadas(IReadOnlyList<(int Id, string Nombre)> habitaciones)
    {
        if (habitaciones.Count == 0)
            return PrefijoInactivarConHabitaciones;
        var detalle = string.Join(", ", habitaciones.Select(h =>
            string.IsNullOrWhiteSpace(h.Nombre)
                ? $"Id {h.Id}"
                : $"{h.Nombre} (Id {h.Id})"));
        return $"{PrefijoInactivarConHabitaciones} {detalle}.";
    }
}