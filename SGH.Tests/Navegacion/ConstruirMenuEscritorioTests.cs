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
        Assert.Equal("fas fa-bed", menu.Grupos[0].Icono);
        Assert.True(menu.Grupos[0].Columnas[0].Items.Count > 0);
        Assert.Equal("Configuración", menu.Grupos.Single(g => g.EsConfiguracion).Titulo);
        Assert.Equal("fas fa-cog", menu.Grupos.Single(g => g.EsConfiguracion).Icono);
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

    [Fact]
    public void Perfil1_gestion_migrados_solicitud_reserva_censo()
    {
        var filas = DatosPerfil1Recorte();
        var menu = ConstruirMenuEscritorio.DesdeFilas(filas, true);
        var gestion = menu.Grupos.First(g => g.Titulo == "Gestión").Columnas[0].Items;

        Assert.True(gestion.Single(i => i.Nombre == "Solicitud").Disponible);
        Assert.True(gestion.Single(i => i.Nombre == "Reserva").Disponible);
        Assert.True(gestion.Single(i => i.Nombre == "Censo").Disponible);
        Assert.False(gestion.Single(i => i.Nombre == "Solicitud Empresa").Disponible);
    }

    [Fact]
    public void Perfil1_configuracion_usuarios_migrado()
    {
        var filas = DatosPerfil1Recorte();
        var menu = ConstruirMenuEscritorio.DesdeFilas(filas, true);
        var config = menu.Grupos.Single(g => g.EsConfiguracion).Columnas[0].Items;

        var usuarios = config.Single(i => i.Nombre == "Usuarios");
        Assert.True(usuarios.Disponible);
        Assert.Equal(MenuClaves.Usuarios, usuarios.ClaveModulo);

        var habitaciones = config.Single(i => i.Nombre == "Habitaciones");
        Assert.True(habitaciones.Disponible);
        Assert.Equal(MenuClaves.Habitaciones, habitaciones.ClaveModulo);
        Assert.Equal("/Habitacion/Datos", habitaciones.RutaPagina);

        var banos = config.Single(i => i.Nombre == "Baños");
        Assert.True(banos.Disponible);
        Assert.Equal(MenuClaves.Banos, banos.ClaveModulo);
        Assert.Equal("/Habitacion/Banos", banos.RutaPagina);

        var tipos = config.Single(i => i.Nombre == "Tipos de cama");
        Assert.True(tipos.Disponible);
        Assert.Equal(MenuClaves.TiposCama, tipos.ClaveModulo);
        Assert.Equal("/Habitacion/TiposCama", tipos.RutaPagina);
    }

    [Fact]
    public void Perfil1_omite_nav_sin_hijos_dashboard()
    {
        var filas = DatosPerfil1Recorte();
        var menu = ConstruirMenuEscritorio.DesdeFilas(filas, true);

        Assert.DoesNotContain(menu.Grupos, g => g.Titulo == "Dashboard");
        Assert.Contains(menu.Grupos, g => g.Titulo == "Gestión");
    }

    private static List<MenuPerfilItem> DatosPerfil1Recorte() =>
    [
        new(1, "Dashboard", "/app/dashboard", null, "nav", 1, null),
        new(2, "Gestión", "/app/gestion", null, "nav", 2, null),
        new(4, "Configuración", "/app/configuracion", null, "nav", 4, null),
        new(6, "Solicitud", "/app/gestion/solicitud", 2, "side", 1, null),
        new(7, "Reserva", "/app/gestion/reserva", 2, "side", 2, null),
        new(8, "Censo", "/app/gestion/censo", 2, "side", 3, null),
        new(80, "Solicitud Empresa", "/app/gestion/solicitudempresa", 2, "side", 6, null),
        new(18, "Usuarios", "/app/config/usuario", 4, "side", 6, null),
        new(901, "Habitaciones", "/app/config/habitaciones", 4, "side", 7, null),
        new(902, "Baños", "/app/config/banos", 4, "side", 8, null),
        new(903, "Tipos de cama", "/app/config/tiposcama", 4, "side", 9, null),
    ];

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