namespace SGH.Domain.Habitacion;

public static class ReglasCama
{
    public const string CamaAjenaHabitacion =
        "Una cama registrada en una habitación no puede asignarse a otra habitación.";

    public const string CamaNoExiste = "La cama no existe en el campamento.";

    public static string? ValidarPerteneceHabitacion(int camaId, int habitacionIdRegistrada, int habitacionIdSolicitada)
    {
        if (camaId <= 0)
            return null;
        if (habitacionIdRegistrada <= 0)
            return CamaNoExiste;
        if (habitacionIdRegistrada != habitacionIdSolicitada)
            return CamaAjenaHabitacion;
        return null;
    }
}