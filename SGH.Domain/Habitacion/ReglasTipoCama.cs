namespace SGH.Domain.Habitacion;

public static class ReglasTipoCama
{
    public static string? ValidarGuardar(string? nombre, short anchoCm, short largoCm)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return "Ingrese el nombre del tipo de cama.";
        if (nombre.Trim().Length > 100)
            return "El nombre del tipo de cama no puede superar 100 caracteres.";
        if (anchoCm <= 0 || largoCm <= 0)
            return "Ancho y largo deben ser mayores que cero.";
        return null;
    }
}