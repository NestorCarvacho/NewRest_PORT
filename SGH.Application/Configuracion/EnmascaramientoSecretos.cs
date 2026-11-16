using System.Text.RegularExpressions;

namespace SGH.Application.Configuracion;

public static partial class EnmascaramientoSecretos
{
    public static string EnmascararCadenaConexion(string? cadena)
    {
        if (string.IsNullOrWhiteSpace(cadena))
            return "(no configurada)";

        return PatronPassword().Replace(cadena, "$1=***");
    }

    public static string EnmascararValor(string? clave, string? valor)
    {
        if (string.IsNullOrEmpty(valor))
            return string.Empty;
        if (clave is null)
            return "***";
        if (clave.Contains("Password", StringComparison.OrdinalIgnoreCase)
            || clave.Contains("Contrasena", StringComparison.OrdinalIgnoreCase)
            || clave.Contains("Secret", StringComparison.OrdinalIgnoreCase)
            || clave.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase))
            return "***";
        return valor;
    }

    [GeneratedRegex(@"(Password|Pwd)(\s*=[^;]*)", RegexOptions.IgnoreCase)]
    private static partial Regex PatronPassword();
}