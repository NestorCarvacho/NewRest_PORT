namespace SGH.Application.Mid;

public static class RegistrosDepuracion
{
    public static object Invocacion(string procedimiento, object parametros) =>
        new { procedimiento, parametros };
}