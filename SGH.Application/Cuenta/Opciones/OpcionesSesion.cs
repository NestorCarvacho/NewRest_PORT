namespace SGH.Application.Cuenta.Opciones;

public sealed class OpcionesSesion
{
    public const string Seccion = "Sesion";

    public int MinutosPorDefecto { get; init; } = 60;

    public int MinutosOperaciones { get; init; } = 480;

    public int MinutosPara(int perfilId)
    {
        var minutos = SGH.Domain.Cuenta.ReglasCuenta.UsaSesionOperaciones(perfilId)
            ? MinutosOperaciones
            : MinutosPorDefecto;
        return minutos > 0 ? minutos : 60;
    }
}
