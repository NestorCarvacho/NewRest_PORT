namespace SGH.Domain.Cuenta;

public static class NormalizadorRut
{
    public static string Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return string.Empty;

        var texto = valor.Trim();
        var guion = texto.IndexOf('-');
        return guion < 0 ? texto : texto.Remove(guion, 1);
    }
}
