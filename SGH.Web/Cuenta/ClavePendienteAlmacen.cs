using Microsoft.Extensions.Caching.Memory;

namespace SGH.Web.Cuenta;

public sealed class ClavePendienteAlmacen
{
    private readonly IMemoryCache _cache;

    public ClavePendienteAlmacen(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Guardar(string id, string clave, DateTime expiraUtc) =>
        _cache.Set(Llave(id), clave, expiraUtc);

    public string? Leer(string id) =>
        _cache.TryGetValue(Llave(id), out string? clave) ? clave : null;

    public void Descartar(string id)
    {
        if (!string.IsNullOrEmpty(id))
            _cache.Remove(Llave(id));
    }

    private static string Llave(string id) => "sgh.clave." + id;
}
