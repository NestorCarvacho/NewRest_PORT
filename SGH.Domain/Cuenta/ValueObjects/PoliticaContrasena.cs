namespace SGH.Domain.Cuenta.ValueObjects;

public readonly record struct ResultadoPoliticaContrasena(bool EsValida, int Codigo, string Descripcion)
{
    public static ResultadoPoliticaContrasena Ok { get; } = new(true, 0, "OK");
}

public static class PoliticaContrasena
{
    public const int LargoMinimo = 6;
    public const int LetrasMinimas = 3;
    public const int NumerosMinimos = 3;

    public static ResultadoPoliticaContrasena Evaluar(string? contrasena)
    {
        var clave = contrasena ?? string.Empty;

        if (clave.Length < LargoMinimo)
        {
            return new ResultadoPoliticaContrasena(false, -2, $"Clave debe ser mayor a {LargoMinimo}");
        }

        if (ContarLetras(clave) < LetrasMinimas)
        {
            return new ResultadoPoliticaContrasena(false, -3, $"Clave debe tener al menos {LetrasMinimas} caracteres");
        }

        if (ContarNumeros(clave) < NumerosMinimos)
        {
            return new ResultadoPoliticaContrasena(false, -4, $"Clave debe tener al menos {NumerosMinimos} números");
        }

        if (TieneCaracteresIgualesSeguidos(clave))
        {
            return new ResultadoPoliticaContrasena(false, -5, "Clave no debe tener caracteres consecutivos");
        }

        return ResultadoPoliticaContrasena.Ok;
    }

    private static int ContarLetras(string clave)
    {
        var total = 0;
        foreach (var caracter in clave)
        {
            if (caracter is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
                total++;
        }

        return total;
    }

    private static int ContarNumeros(string clave)
    {
        var total = 0;
        foreach (var caracter in clave)
        {
            if (caracter is >= '0' and <= '9')
                total++;
        }

        return total;
    }

    private static bool TieneCaracteresIgualesSeguidos(string clave)
    {
        for (var i = 0; i < clave.Length - 1; i++)
        {
            if (clave[i] == clave[i + 1])
                return true;
        }

        return false;
    }
}
