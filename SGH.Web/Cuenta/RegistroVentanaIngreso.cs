namespace SGH.Web.Cuenta;

public sealed class RegistroVentanaIngreso
{
    public const int Tope = 8;
    public const string ClaveRestantes = "IntentosRestantes";
    public static readonly TimeSpan Duracion = TimeSpan.FromMinutes(10);

    private readonly Lock _sync = new();
    private readonly Dictionary<string, Ventana> _ventanas = new(StringComparer.Ordinal);

    public int Registrar(string clave)
    {
        lock (_sync)
        {
            var ahora = DateTimeOffset.UtcNow;
            if (!_ventanas.TryGetValue(clave, out var ventana) || ventana.Expira <= ahora)
                ventana = new Ventana(ahora.Add(Duracion), 0);

            ventana = ventana with { Usados = ventana.Usados + 1 };
            _ventanas[clave] = ventana;
            return Math.Max(0, Tope - ventana.Usados);
        }
    }

    public static string Texto(int restantes) => restantes switch
    {
        <= 0 => "No quedan intentos.",
        1 => "Queda 1 intento.",
        _ => $"Quedan {restantes} intentos."
    };

    private sealed record Ventana(DateTimeOffset Expira, int Usados);
}
