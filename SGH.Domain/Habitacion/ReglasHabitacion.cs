namespace SGH.Domain.Habitacion;

/// <summary>
/// Validaciones de infraestructura alineadas a CU-HAB-01 en la medida del modelo HOTELDB
/// (habitación + CantidadCamas; sin entidad Baño).
/// </summary>
public static class ReglasHabitacion
{
    public const string NombreRequerido = "Ingrese el código o nombre de la habitación.";
    public const string ModuloRequerido = "Seleccione el módulo (ubicación física) de la habitación.";
    public const string CantidadCamasInvalida = "La habitación debe tener al menos 1 cama (capacidad física).";
    public const string SexoInvalido = "Seleccione un sexo válido (M, F, O o MIXTO).";
    public const string CapacidadPersonasInvalida = "La capacidad máxima de personas no puede ser menor que la mínima.";
    public const string UsoExclusivoCapacidad = "Para uso exclusivo, la capacidad máxima de personas debe cubrir todas las camas habilitadas.";
    public const string MinutosHigienizacion = "Los minutos de higienización entre turnos no pueden ser negativos.";

    private static readonly HashSet<string> SexosPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "M", "F", "O", "MIXTO", "HOMBRE", "MUJER" };

    public static string? ValidarAlta(
        string? nombre,
        int moduloId,
        int cantidadCamas,
        string? sexo,
        int personasMin,
        int personasMax,
        bool usoExclusivo,
        int minutosHigienizacion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return NombreRequerido;
        if (moduloId <= 0)
            return ModuloRequerido;
        if (cantidadCamas < 1)
            return CantidadCamasInvalida;
        if (string.IsNullOrWhiteSpace(sexo) || !SexosPermitidos.Contains(sexo.Trim()))
            return SexoInvalido;
        if (personasMax > 0 && personasMin > personasMax)
            return CapacidadPersonasInvalida;
        if (usoExclusivo && personasMax > 0 && personasMax < cantidadCamas)
            return UsoExclusivoCapacidad;
        if (minutosHigienizacion < 0)
            return MinutosHigienizacion;
        return null;
    }

    /// <summary>RN-02: compatibilidad sexo habitación con tipo de sexo del baño.</summary>
    public static bool SexoCompatibleBano(string? sexoHabitacion, string? tipoSexoBano)
    {
        if (string.IsNullOrWhiteSpace(tipoSexoBano))
            return true;
        var b = tipoSexoBano.Trim().ToUpperInvariant();
        var h = NormalizarSexo(sexoHabitacion);
        if (b is "MIXTO" or "M" or "F")
            return true;
        if (b is "HOMBRE" or "M")
            return h is "M" or "MIXTO";
        if (b is "MUJER" or "F")
            return h is "F" or "MIXTO";
        return SexoCompatible(sexoHabitacion, tipoSexoBano);
    }

    /// <summary>RN-02 simplificado entre dos códigos de sexo.</summary>
    public static bool SexoCompatible(string? sexoHabitacion, string? sexoComparar)
    {
        if (string.IsNullOrWhiteSpace(sexoComparar))
            return true;
        var h = NormalizarSexo(sexoHabitacion);
        var c = NormalizarSexo(sexoComparar);
        if (h is "MIXTO" or "M" or "F" && c is "MIXTO")
            return true;
        return string.Equals(h, c, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizarSexo(string? sexo)
    {
        if (string.IsNullOrWhiteSpace(sexo)) return "MIXTO";
        var s = sexo.Trim().ToUpperInvariant();
        return s switch
        {
            "HOMBRE" => "M",
            "MUJER" => "F",
            "MASCULINO" => "M",
            "FEMENINO" => "F",
            _ => s
        };
    }
}