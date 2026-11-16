using SGH.Domain.Cuenta;

namespace SGH.Application.Navegacion;

/// <summary>
/// Respaldo cuando la BD no devuelve filas (entorno sin HOTELDB o perfil sin MenusPerfil cargado).
/// Aproxima el menú fijo previo filtrado por tipo de perfil.
/// </summary>
internal static class MenuRespaldoEstatico
{
    public static MenuEscritorioVm ParaPerfil(int perfilId)
    {
        var esPasajero = MatrizPerfiles.EsPasajero(perfilId);
        var gestion = new List<MenuItemVm>
        {
            Item("Solicitud", MenuClaves.Solicitud, "/Solicitud/Datos"),
            Item("Reserva", MenuClaves.Reserva, "/Reserva/Datos"),
        };

        if (!esPasajero)
            gestion.Add(Item("Censo", MenuClaves.Censo, "/Censo/Datos"));

        var grupos = new List<MenuGrupoVm>
        {
            new(
                "Gestión",
                esConfiguracion: false,
                new[]
                {
                    new MenuColumnaVm("Gestión", gestion)
                })
        };

        if (!esPasajero)
        {
            var config = new List<MenuItemVm>
            {
                Item("Habitaciones", MenuClaves.Habitaciones, "/Habitacion/Datos"),
                Item("Usuarios", MenuClaves.Usuarios, "/Cuenta/Usuarios"),
            };
            grupos.Add(new MenuGrupoVm(
                "Configuración",
                esConfiguracion: true,
                new[] { new MenuColumnaVm("Ubicación y acceso", config) }));
        }

        return new MenuEscritorioVm(grupos, desdeBaseDeDatos: false);
    }

    private static MenuItemVm Item(string nombre, string clave, string ruta) =>
        new(nombre, clave, ruta, disponible: true, tituloDeshabilitado: null);
}