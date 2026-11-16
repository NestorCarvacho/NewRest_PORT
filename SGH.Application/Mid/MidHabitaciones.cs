using SGH.Application.Habitacion.Interfaces;
using SGH.Application.Sesion;
using SGH.Domain.Habitacion.Interfaces;

namespace SGH.Application.Mid;

public sealed class MidHabitaciones(IGestionHabitaciones gestion)
{
    public const string Ruta = "Mid/MidHabitaciones.cs";

    public static readonly IReadOnlyList<AccionApi> Opciones =
    [
        Accion("listar", "Listar", "dbo.sp_Habitacion_Listar", "Lista habitaciones del campamento."),
        Accion("catalogo", "Catálogo", "dbo.sp_Habitacion_Catalogo", "Pabellones, módulos, sexos y estados."),
        Accion("obtener", "Obtener", "dbo.sp_Habitacion_Obtener", "Detalle de una habitación."),
        Accion("crear", "Crear", "dbo.HAB_GuardaHabitacion", "Alta de habitación."),
        Accion("editar", "Editar", "dbo.HAB_GuardaHabitacion", "Actualización de habitación."),
        Accion("inactivar", "Inactivar", "dbo.HAB_GuardaHabitacion", "Baja lógica (IsActive=0).")
    ];

    public async Task<EjecucionMid> EjecutarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var bloqueo = AlcanceMid.BloquearSiSinAlcance(sesion);
        if (bloqueo is not null)
            return bloqueo;

        return filtro.Opcion switch
        {
            "listar" => await ListarAsync(filtro, sesion, cancellationToken),
            "catalogo" => await CatalogoAsync(sesion, cancellationToken),
            "obtener" => await ObtenerAsync(filtro, sesion, cancellationToken),
            "crear" => await GuardarAsync(filtro, sesion, esAlta: true, cancellationToken),
            "editar" => await GuardarAsync(filtro, sesion, esAlta: false, cancellationToken),
            "inactivar" => await InactivarAsync(filtro, sesion, cancellationToken),
            _ => EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está implementada en el Mid.")
        };
    }

    private async Task<EjecucionMid> ListarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var filas = await gestion.ListarAsync(new FiltroListadoHabitacion(
            sesion.EmpresaMandanteId,
            sesion.CampamentoId,
            LecturaPeticion.Entero(filtro, "pabellonId"),
            LecturaPeticion.Entero(filtro, "moduloId"),
            LecturaPeticion.Entero(filtro, "tipoInstalacionId"),
            LecturaPeticion.Texto(filtro, "estado"),
            LecturaPeticion.EnteroOpcional(filtro, "soloActivas") ?? 1,
            null), sesion.UsuarioId, cancellationToken);
        return EjecucionMid.Ok(new { filas });
    }

    private async Task<EjecucionMid> CatalogoAsync(DatosSesion sesion, CancellationToken cancellationToken)
    {
        var catalogo = await gestion.CatalogoAsync(sesion.CampamentoId, sesion.EmpresaMandanteId, cancellationToken);
        return EjecucionMid.Ok(catalogo);
    }

    private async Task<EjecucionMid> ObtenerAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var id = LecturaPeticion.Entero(filtro, "id");
        if (id <= 0)
            return EjecucionMid.Fallo(400, "Seleccione una habitación.");
        var detalle = await gestion.ObtenerAsync(id, sesion.CampamentoId, cancellationToken);
        if (detalle is null)
            return EjecucionMid.Fallo(404, "La habitación no existe en el campamento.");
        return EjecucionMid.Ok(detalle);
    }

    private async Task<EjecucionMid> GuardarAsync(
        FiltroPeticion filtro, DatosSesion sesion, bool esAlta, CancellationToken cancellationToken)
    {
        var datos = LeerCuerpo(filtro, sesion.UsuarioId, esAlta);
        if (datos is null)
            return EjecucionMid.Fallo(400, "Revise los datos del formulario.");
        var resultado = await gestion.GuardarAsync(datos, sesion.CampamentoId, cancellationToken);
        if (!resultado.Exito)
            return EjecucionMid.Fallo(400, resultado.Mensaje);
        return EjecucionMid.Ok(new { resultado = true, id = resultado.Id, mensaje = "Habitación guardada." });
    }

    private async Task<EjecucionMid> InactivarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var id = LecturaPeticion.Entero(filtro, "id");
        if (id <= 0)
            return EjecucionMid.Fallo(400, "Seleccione una habitación.");
        var actual = await gestion.ObtenerAsync(id, sesion.CampamentoId, cancellationToken);
        if (actual is null)
            return EjecucionMid.Fallo(404, "La habitación no existe en el campamento.");

        var datos = new DatosGuardarHabitacion(
            actual.Id,
            actual.ModuloId,
            actual.Nombre,
            false,
            actual.CantidadCamas,
            actual.Sexo ?? "M",
            actual.TipoInstalacionId,
            actual.StandardHabitacionId,
            actual.Estado,
            actual.ObservacionBloqueo,
            actual.M2,
            actual.IsReservable,
            actual.PersonasCantidadMin,
            actual.PersonasCantidadMax,
            actual.MinutosMin,
            actual.MinutosMax,
            actual.TieneServiciosAlimentacion,
            actual.PermiteUsoPorTurnos,
            actual.PermiteUsoExclusivo,
            sesion.UsuarioId);

        var resultado = await gestion.GuardarAsync(datos, sesion.CampamentoId, cancellationToken);
        if (!resultado.Exito)
            return EjecucionMid.Fallo(400, resultado.Mensaje);
        return EjecucionMid.Ok(new { resultado = true, mensaje = "Habitación inactivada." });
    }

    private static DatosGuardarHabitacion? LeerCuerpo(FiltroPeticion filtro, int usuarioId, bool esAlta)
    {
        var nombre = LecturaPeticion.Texto(filtro, "nombre");
        var moduloId = LecturaPeticion.Entero(filtro, "moduloId");
        var cantidadCamas = LecturaPeticion.EnteroOpcional(filtro, "cantidadCamas") ?? 1;
        var sexo = LecturaPeticion.Texto(filtro, "sexo");
        var id = esAlta ? 0 : LecturaPeticion.Entero(filtro, "id");
        if (!esAlta && id <= 0)
            return null;

        return new DatosGuardarHabitacion(
            id,
            moduloId,
            nombre ?? "",
            !LecturaPeticion.Bandera(filtro, "inactivo"),
            cantidadCamas,
            sexo ?? "",
            LecturaPeticion.Entero(filtro, "tipoInstalacionId"),
            LecturaPeticion.Entero(filtro, "standardHabitacionId"),
            LecturaPeticion.Texto(filtro, "estado"),
            LecturaPeticion.Texto(filtro, "observacionBloqueo"),
            null,
            LecturaPeticion.Campo(filtro, "isReservable") is null || LecturaPeticion.Bandera(filtro, "isReservable"),
            LecturaPeticion.Entero(filtro, "personasCantidadMin"),
            LecturaPeticion.Entero(filtro, "personasCantidadMax"),
            LecturaPeticion.Entero(filtro, "minutosMin"),
            LecturaPeticion.Entero(filtro, "minutosMax"),
            LecturaPeticion.Bandera(filtro, "tieneServiciosAlimentacion"),
            LecturaPeticion.Bandera(filtro, "usoPorTurnos"),
            LecturaPeticion.Bandera(filtro, "usoExclusivo"),
            usuarioId);
    }

    private static AccionApi Accion(string opcion, string etiqueta, string procedimiento, string descripcion) =>
        new()
        {
            Id = opcion,
            Opcion = opcion,
            Etiqueta = etiqueta,
            Mid = "Habitaciones",
            Procedimiento = procedimiento,
            Descripcion = descripcion
        };
}