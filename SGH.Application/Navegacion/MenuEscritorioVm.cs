namespace SGH.Application.Navegacion;

public sealed class MenuEscritorioVm
{
    public MenuEscritorioVm(IReadOnlyList<MenuGrupoVm> grupos, bool desdeBaseDeDatos)
    {
        Grupos = grupos;
        DesdeBaseDeDatos = desdeBaseDeDatos;
    }

    public IReadOnlyList<MenuGrupoVm> Grupos { get; }
    public bool DesdeBaseDeDatos { get; }
}

public sealed class MenuGrupoVm
{
    public MenuGrupoVm(string titulo, bool esConfiguracion, IReadOnlyList<MenuColumnaVm> columnas, string? icono = null)
    {
        Titulo = titulo;
        EsConfiguracion = esConfiguracion;
        Columnas = columnas;
        Icono = icono;
    }

    public string Titulo { get; }
    public bool EsConfiguracion { get; }
    public IReadOnlyList<MenuColumnaVm> Columnas { get; }
    /// <summary>Clase Font Awesome 5 (ej. <c>fas fa-bed</c>) de la raíz <c>nav</c>.</summary>
    public string? Icono { get; }
}

public sealed class MenuColumnaVm
{
    public MenuColumnaVm(string titulo, IReadOnlyList<MenuItemVm> items)
    {
        Titulo = titulo;
        Items = items;
    }

    public string Titulo { get; }
    public IReadOnlyList<MenuItemVm> Items { get; }
}

public sealed class MenuItemVm
{
    public MenuItemVm(
        string nombre,
        string? claveModulo,
        string? rutaPagina,
        bool disponible,
        string? tituloDeshabilitado)
    {
        Nombre = nombre;
        ClaveModulo = claveModulo;
        RutaPagina = rutaPagina;
        Disponible = disponible;
        TituloDeshabilitado = tituloDeshabilitado;
    }

    public string Nombre { get; }
    public string? ClaveModulo { get; }
    public string? RutaPagina { get; }
    public bool Disponible { get; }
    public string? TituloDeshabilitado { get; }
}