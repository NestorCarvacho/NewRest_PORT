using SGH.Application.Navegacion;
using SGH.Domain.Navegacion;

namespace SGH.Tests.Navegacion;

public sealed class ConstruirMenuEscritorioTests
{
    [Fact]
    public void Perfil137_forma_cuatro_grupos_nav_con_hijos_side()
    {
        var filas = DatosPerfil137();
        var menu = ConstruirMenuEscritorio.DesdeFilas(filas, desdeBaseDeDatos: true);

        Assert.Equal(4, menu.Grupos.Count);
        Assert.Equal("Gestión", menu.Grupos[0].Titulo);
        Assert.True(menu.Grupos[0].Columnas[0].Items.Count > 0);
        Assert.Equal("Configuración", menu.Grupos.Single(g => g.EsConfiguracion).Titulo);
    }

    [Fact]
    public void Censo_gestion_mapea_informe_censo_no()
    {
        var filas = DatosPerfil137();
        var menu = ConstruirMenuEscritorio.DesdeFilas(filas, true);
        var gestion = menu.Grupos.First(g => g.Titulo == "Gestión");
        var censo = gestion.Columnas[0].Items.Single(i => i.Nombre == "Censo");
        var informeCenso = menu.Grupos.First(g => g.Titulo == "Informes").Columnas[0].Items.Single(i => i.Nombre == "Censo");

        Assert.True(censo.Disponible);
        Assert.Equal(MenuClaves.Censo, censo.ClaveModulo);
        Assert.False(informeCenso.Disponible);
    }

    private static List<MenuPerfilItem> DatosPerfil137() =>
    [
        new(2, "Gestión", "/app/gestion", null, "nav", 2, "fas fa-bed"),
        new(3, "Informes", "/app/informe", null, "nav", 3, "fas fa-chart-bar"),
        new(4, "Configuración", "/app/configuracion", null, "nav", 4, "fas fa-cog"),
        new(5, "Operaciones", "/app/operaciones", null, "nav", 5, "fas fa-clipboard-check"),
        new(97, "Asignación Servicio", "/app/gestion/asignacionaseo", 2, "side", 1, null),
        new(8, "Censo", "/app/gestion/censo", 2, "side", 3, null),
        new(27, "Censo", "/app/gestion/informe/censo", 3, "side", 4, null),
        new(25, "Censo QR", "/app/gestion/miscensos", 5, "side", 3, null),
        new(20, "Códigos QR", "/app/config/codigoqr", 4, "side", 8, null),
    ];
}