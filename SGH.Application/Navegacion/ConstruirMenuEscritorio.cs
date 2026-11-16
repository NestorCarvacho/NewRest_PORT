using SGH.Domain.Navegacion;

namespace SGH.Application.Navegacion;

public static class ConstruirMenuEscritorio
{
    private static readonly StringComparer Comparador = StringComparer.OrdinalIgnoreCase;

    public static MenuEscritorioVm DesdeFilas(IReadOnlyList<MenuPerfilItem> filas, bool desdeBaseDeDatos)
    {
        if (filas.Count == 0)
            return new MenuEscritorioVm(Array.Empty<MenuGrupoVm>(), desdeBaseDeDatos);

        var raices = filas
            .Where(EsRaizBarra)
            .OrderBy(f => f.Orden)
            .ThenBy(f => f.Nombre, Comparador)
            .ToList();

        if (raices.Count == 0)
            return DesdeFilasPlano(filas, desdeBaseDeDatos);

        var grupos = new List<MenuGrupoVm>();
        foreach (var raiz in raices)
        {
            var hijos = filas
                .Where(f => f.PadreId == raiz.Id)
                .OrderBy(f => f.Orden)
                .ThenBy(f => f.Nombre, Comparador)
                .Select(CrearItem)
                .ToList();

            if (hijos.Count == 0)
                continue;

            var esConfig = EsGrupoConfiguracionRaiz(raiz);
            var columna = new MenuColumnaVm(raiz.Nombre, hijos);
            grupos.Add(new MenuGrupoVm(raiz.Nombre, esConfig, new[] { columna }));
        }

        return new MenuEscritorioVm(grupos, desdeBaseDeDatos);
    }

    /// <summary>Formato legado sin filas <c>nav</c> (solo <c>side</c> u otras ubicaciones).</summary>
    private static MenuEscritorioVm DesdeFilasPlano(IReadOnlyList<MenuPerfilItem> filas, bool desdeBaseDeDatos)
    {
        var grupos = filas
            .GroupBy(f => string.IsNullOrWhiteSpace(f.Ubicacion) ? "Otros" : f.Ubicacion.Trim(), Comparador)
            .OrderBy(g => g.Key, Comparador)
            .Select(grupo =>
            {
                var items = grupo.OrderBy(f => f.Orden).Select(CrearItem).ToList();
                var titulo = grupo.Key;
                var esConfig = titulo.Contains("config", StringComparison.OrdinalIgnoreCase);
                return new MenuGrupoVm(titulo, esConfig, new[] { new MenuColumnaVm(titulo, items) });
            })
            .ToList();

        return new MenuEscritorioVm(grupos, desdeBaseDeDatos);
    }

    private static bool EsRaizBarra(MenuPerfilItem fila) =>
        string.Equals(fila.Ubicacion, "nav", StringComparison.OrdinalIgnoreCase)
        || fila.PadreId is null or 0;

    private static bool EsGrupoConfiguracionRaiz(MenuPerfilItem raiz) =>
        raiz.Nombre.Contains("configuración", StringComparison.OrdinalIgnoreCase)
        || raiz.Nombre.Contains("configuracion", StringComparison.OrdinalIgnoreCase)
        || raiz.Link.Contains("/config", StringComparison.OrdinalIgnoreCase);

    private static MenuItemVm CrearItem(MenuPerfilItem fila)
    {
        if (MapeoEnlacesMenu.TryMapear(fila.Link, out var mapeo))
        {
            return new MenuItemVm(
                fila.Nombre,
                mapeo.ClaveModulo,
                mapeo.RutaPagina,
                disponible: true,
                tituloDeshabilitado: null);
        }

        return new MenuItemVm(
            fila.Nombre,
            claveModulo: null,
            rutaPagina: null,
            disponible: false,
            tituloDeshabilitado: "No disponible");
    }
}