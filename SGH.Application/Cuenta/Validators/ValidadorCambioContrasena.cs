using SGH.Domain.Cuenta.ValueObjects;

namespace SGH.Application.Cuenta.Validators;

public static class ValidadorCambioContrasena
{
    public static string? Validar(string? actual, string? nueva, string? confirmacion)
    {
        string? mensajeActual = null;
        string? mensajeNueva = null;
        string? mensajeConfirmacion = null;

        if (string.IsNullOrEmpty(actual))
            mensajeActual = "Debe ingresar una contraseña";
        if (string.IsNullOrEmpty(nueva))
            mensajeNueva = "Debe ingresar una nueva contraseña";
        if (string.IsNullOrEmpty(confirmacion))
            mensajeConfirmacion = "Debe repetir la contraseña";
        if (!string.Equals(nueva ?? string.Empty, confirmacion ?? string.Empty, StringComparison.Ordinal))
            mensajeConfirmacion = "Password debe ser la misma";

        if (!string.IsNullOrEmpty(nueva))
        {
            var politica = PoliticaContrasena.Evaluar(nueva);
            if (!politica.EsValida)
                mensajeNueva ??= politica.Descripcion;
        }

        return mensajeActual ?? mensajeNueva ?? mensajeConfirmacion;
    }
}
