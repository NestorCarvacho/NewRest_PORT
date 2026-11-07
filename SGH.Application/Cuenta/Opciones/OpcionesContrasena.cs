namespace SGH.Application.Cuenta.Opciones;

public sealed class OpcionesContrasena
{
    public const string Seccion = "Contrasena";

    public int FactorTrabajoBcrypt { get; init; } = 11;
}
