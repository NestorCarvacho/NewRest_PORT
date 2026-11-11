namespace SGH.Application.Mid;

public sealed class EjecucionMid
{
    public const int OkStatus = 200;

    public int Status { get; init; }
    public string? Mensaje { get; init; }
    public object? Registros { get; init; }
    public byte[]? Archivo { get; init; }
    public string? NombreArchivo { get; init; }
    public string? TipoArchivo { get; init; }

    public bool EsArchivo => Archivo is not null;

    public static EjecucionMid Ok(object? registros) => new()
    {
        Status = OkStatus,
        Registros = registros
    };

    public static EjecucionMid Fallo(int status, string mensaje) => new()
    {
        Status = status,
        Mensaje = mensaje
    };

    public static EjecucionMid Descarga(byte[] contenido, string nombre, string tipo) => new()
    {
        Status = OkStatus,
        Archivo = contenido,
        NombreArchivo = nombre,
        TipoArchivo = tipo
    };
}
