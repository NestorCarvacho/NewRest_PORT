using Microsoft.Extensions.Caching.Memory;
using SGH.Application.Navegacion.Interfaces;
using SGH.Domain.Navegacion.Interfaces;

namespace SGH.Application.Navegacion;

public sealed class ServicioMenuPerfil : IServicioMenuPerfil
{
    private static readonly TimeSpan VigenciaCache = TimeSpan.FromMinutes(10);
    private readonly IConsultaMenuPerfil _consulta;
    private readonly IMemoryCache _cache;

    public ServicioMenuPerfil(IConsultaMenuPerfil consulta, IMemoryCache cache)
    {
        _consulta = consulta;
        _cache = cache;
    }

    public async Task<MenuEscritorioVm> ObtenerMenuAsync(int perfilId, CancellationToken cancellationToken)
    {
        var clave = ClaveCache(perfilId);
        if (_cache.TryGetValue(clave, out MenuEscritorioVm? cacheado) && cacheado is not null)
            return cacheado;

        IReadOnlyList<Domain.Navegacion.MenuPerfilItem> filas;
        try
        {
            filas = await _consulta.ObtenerPorPerfilAsync(perfilId, cancellationToken);
        }
        catch
        {
            var respaldo = MenuRespaldoEstatico.ParaPerfil(perfilId);
            _cache.Set(clave, respaldo, VigenciaCache);
            return respaldo;
        }

        var menu = filas.Count == 0
            ? MenuRespaldoEstatico.ParaPerfil(perfilId)
            : ConstruirMenuEscritorio.DesdeFilas(filas, desdeBaseDeDatos: true);

        _cache.Set(clave, menu, VigenciaCache);
        return menu;
    }

    public async Task<bool> TieneAccesoModuloAsync(int perfilId, string claveModulo, CancellationToken cancellationToken)
    {
        var menu = await ObtenerMenuAsync(perfilId, cancellationToken);
        return menu.Grupos
            .SelectMany(g => g.Columnas)
            .SelectMany(c => c.Items)
            .Any(i => i.Disponible && string.Equals(i.ClaveModulo, claveModulo, StringComparison.OrdinalIgnoreCase));
    }

    private static string ClaveCache(int perfilId) => $"menu-perfil:{perfilId}";
}