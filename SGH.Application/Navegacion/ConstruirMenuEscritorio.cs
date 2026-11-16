using SGH.Domain.Navegacion;

namespace SGH.Application.Navegacion;

public static class ConstruirMenuEscritorio
{
    private static readonly StringComparer Comparador = StringComparer.OrdinalIgnoreCase;

    public static MenuEscritorioVm DesdeFilas(IReadOnlyList<MenuPerfilItem> filas, bool desdeBaseDeDatos)
    {
        if (filas.Count == 0)
            return new MenuEscritorioVm(Array.Empty<MenuGrupoVm>(), desdeBaseDeDatos);

        var gruposPorUbicacion = filas
            .GroupBy(f => string.IsNullOrWhiteSpace(f.Ubicacion) ? "Otros" : f.Ubicacion.Trim(), Comparador)
            .OrderBy(g => OrdenGrupo(g.Key), Comparador)
            .ThenBy(g => g.Key, Comparador);

        var grupos = new List<MenuGrupoVm>();
        foreach (var grupo in gruposPorUbicacion)
        {
            var items = grupo
                .OrderBy(f => f.Orden)
                .ThenBy(f => f.Nombre, Comparador)
                .Select(CrearItem)
                .ToList();

            var tituloGrupo = TituloGrupo(grupo.Key);
            var esConfig = EsGrupoConfiguracion(grupo.Key);
            var columna = new MenuColumnaVm(tituloGrupo, items);
            grupos.Add(new MenuGrupoVm(tituloGrupo, esConfig, new[] { columna }));
        }

        return new MenuEscritorioVm(grupos, desdeBaseDeDatos);
    }

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

    private static bool EsGrupoConfiguracion(string ubicacion) =>
        ubicacion.Contains("config", StringComparison.OrdinalIgnoreCase)
        || ubicacion.Contains("campamento", StringComparison.OrdinalIgnoreCase)
        || ubicacion.Contains("administr", StringComparison.OrdinalIgnoreCase);

    private static string TituloGrupo(string ubicacion) =>
        string.IsNullOrWhiteSpace(ubicacion) ? "Otros" : ubicacion;

    private static string OrdenGrupo(string ubicacion)
    {
        if (EsGrupoConfiguracion(ubicacion))
            return "zzz";

        return ubicacion;
    }
}