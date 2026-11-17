using SGH.Application.Sesion;
using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.Interfaces;

namespace SGH.Application.Mid;

public sealed class MidSolicitudes(IGestionSolicitudes gestion, INotificadorSolicitud notificador, IPlanillaPasajeros planilla)
{
    public const string Ruta = "Mid/MidSolicitudes.cs";

    public static readonly IReadOnlyList<AccionApi> Opciones =
    [
        Accion("listar", "Listar", "solicitud", "Devuelve las solicitudes del período."),
        Accion("catalogo", "Catálogo", "solicitud", "Devuelve el catálogo de la solicitud."),
        Accion("organizacion", "Organización", "solicitud", "Devuelve unidades, divisiones y departamentos."),
        Accion("detalle", "Detalle", "solicitud", "Devuelve una solicitud."),
        Accion("pasajeros", "Pasajeros", "solicitud", "Devuelve los pasajeros de la solicitud."),
        Accion("habitaciones", "Habitaciones", "solicitud", "Devuelve las habitaciones disponibles."),
        Accion("crear", "Crear", "dbo.sp_Solicitud_Guardar", "Crea una solicitud."),
        Accion("editar", "Editar", "dbo.sp_Solicitud_Guardar", "Actualiza una solicitud."),
        Accion("anular", "Anular", "dbo.sp_Solicitud_Anular", "Anula una solicitud."),
        Accion("cupos", "Cupos", "dbo.sp_Solicitud_AgregarCupos", "Agrega cupos."),
        Accion("pasajero", "Pasajero", "dbo.sp_Solicitud_GuardarPasajero", "Guarda un pasajero."),
        Accion("eliminar", "Eliminar pasajero", "dbo.sp_Solicitud_EliminarPasajero", "Quita un pasajero."),
        Accion("asignar", "Asignar", "dbo.sp_Solicitud_AsignarHabitacion", "Asigna una habitación."),
        Accion("desasignar", "Desasignar", "dbo.sp_Solicitud_DesasignarHabitacion", "Quita la habitación preasignada."),
        Accion("generar", "Generar reservas", "dbo.sp_Solicitud_GenerarReservas", "Genera las reservas de la solicitud."),
        Accion("notificar", "Notificar", "correo", "Reenvía el correo de reservas generadas."),
        Accion("plantilla", "Plantilla", "planilla", "Devuelve la planilla de pasajeros."),
        Accion("cargar", "Cargar", "dbo.sp_Solicitud_CargarPasajero", "Importa pasajeros desde la planilla.")
    ];

    public async Task<EjecucionMid> EjecutarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var bloqueo = AlcanceMid.BloquearSiSinAlcance(sesion);
        if (bloqueo is not null)
            return bloqueo;

        return filtro.Opcion switch
        {
            "listar" => await ListarAsync(filtro, sesion, cancellationToken),
            "catalogo" => await CatalogoAsync(filtro, sesion, cancellationToken),
            "organizacion" => await OrganizacionAsync(filtro, sesion, cancellationToken),
            "detalle" => await DetalleAsync(filtro, sesion, cancellationToken),
            "pasajeros" => await PasajerosAsync(filtro, sesion, cancellationToken),
            "habitaciones" => await HabitacionesAsync(filtro, sesion, cancellationToken),
            "crear" => await GuardarAsync(filtro, sesion, 0, cancellationToken),
            "editar" => await GuardarAsync(filtro, sesion, LecturaPeticion.Entero(filtro, "id"), cancellationToken),
            "anular" => await AnularAsync(filtro, sesion, cancellationToken),
            "cupos" => await CuposAsync(filtro, sesion, cancellationToken),
            "pasajero" => await PasajeroAsync(filtro, sesion, cancellationToken),
            "eliminar" => await EliminarAsync(filtro, sesion, cancellationToken),
            "asignar" => await AsignarAsync(filtro, sesion, cancellationToken),
            "desasignar" => await DesasignarAsync(filtro, sesion, cancellationToken),
            "generar" => await GenerarAsync(filtro, sesion, cancellationToken),
            "notificar" => await NotificarOpcionAsync(filtro, sesion, cancellationToken),
            "plantilla" => EjecucionMid.Descarga(planilla.CrearPlantilla(), "PlantillaPasajeros.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            _ => EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está implementada en el Mid.")
        };
    }

    public async Task<EjecucionMid> CargarAsync(int id, Stream archivo, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var bloqueo = AlcanceMid.BloquearSiSinAlcance(sesion);
        if (bloqueo is not null)
            return bloqueo;

        if (id <= 0)
            return EjecucionMid.Fallo(400, "Seleccione una solicitud y una planilla .xlsx válida.");

        var filas = planilla.Leer(archivo);
        var resultados = await gestion.CargarPasajerosAsync(id, sesion.EmpresaMandanteId, sesion.CampamentoId, sesion.UsuarioId, filas, cancellationToken);
        return EjecucionMid.Ok(new { filas = resultados });
    }

    private async Task<EjecucionMid> ListarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var desde = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "desde"));
        var hasta = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "hasta"));
        if (desde is null || hasta is null)
            return EjecucionMid.Fallo(400, "Ingrese fechas y filtros válidos.");

        var filas = await gestion.BuscarAsync(new FiltroSolicitudesDto(
            sesion.EmpresaMandanteId, sesion.CampamentoId, desde.Value, hasta.Value,
            LecturaPeticion.Entero(filtro, "grupoId"), LecturaPeticion.Entero(filtro, "solicitanteId"),
            LecturaPeticion.Texto(filtro, "estado") is { Length: > 0 } estado ? estado : "0",
            LecturaPeticion.Entero(filtro, "numero")), cancellationToken);
        return EjecucionMid.Ok(new { filas });
    }

    private async Task<EjecucionMid> CatalogoAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken) =>
        EjecucionMid.Ok(await gestion.CatalogoAsync(sesion.EmpresaMandanteId, sesion.CampamentoId, LecturaPeticion.Entero(filtro, "grupoId"), cancellationToken));

    private async Task<EjecucionMid> OrganizacionAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken) =>
        EjecucionMid.Ok(await gestion.OrganizacionAsync(
            sesion.EmpresaMandanteId, sesion.CampamentoId,
            LecturaPeticion.Entero(filtro, "unidadId"), LecturaPeticion.Entero(filtro, "divisionId"), cancellationToken));

    private async Task<EjecucionMid> DetalleAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var detalle = await gestion.ObtenerAsync(LecturaPeticion.Entero(filtro, "id"), sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
        return detalle is null
            ? EjecucionMid.Fallo(404, "Solicitud no encontrada.")
            : EjecucionMid.Ok(detalle);
    }

    private async Task<EjecucionMid> PasajerosAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var pasajeros = await gestion.ListarPasajerosAsync(LecturaPeticion.Entero(filtro, "id"), sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
        return EjecucionMid.Ok(new { pasajeros });
    }

    private async Task<EjecucionMid> HabitacionesAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var habitaciones = await gestion.HabitacionesDisponiblesAsync(LecturaPeticion.Entero(filtro, "id"), sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
        return EjecucionMid.Ok(new { habitaciones });
    }

    private async Task<EjecucionMid> GuardarAsync(FiltroPeticion filtro, DatosSesion sesion, int id, CancellationToken cancellationToken)
    {
        if (filtro.Opcion == "editar" && id <= 0)
            return EjecucionMid.Fallo(400, "La solicitud no llegó completa o contiene datos inválidos.");

        var resultado = await gestion.GuardarAsync(new DatosSolicitudDto(
            id, sesion.EmpresaMandanteId, sesion.CampamentoId, sesion.UsuarioId,
            LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fechaSolicitud")),
            LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fechaCheckIn")),
            LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fechaCheckOut")),
            LecturaPeticion.Entero(filtro, "grupoId"),
            LecturaPeticion.EnteroOpcional(filtro, "solicitanteId"),
            LecturaPeticion.Bandera(filtro, "conSistemaTurno"),
            LecturaPeticion.EnteroOpcional(filtro, "turnoId"),
            LecturaPeticion.EnteroOpcional(filtro, "turnoRotacionId"),
            LecturaPeticion.Texto(filtro, "observacion"),
            LecturaPeticion.Texto(filtro, "adc"),
            LecturaPeticion.Texto(filtro, "emailContactoCampamento") ?? "",
            LecturaPeticion.Texto(filtro, "numeroContactoCampamento"),
            LecturaPeticion.EnteroOpcional(filtro, "unidadNegocioId"),
            LecturaPeticion.EnteroOpcional(filtro, "divisionId"),
            LecturaPeticion.EnteroOpcional(filtro, "departamentoId"),
            LecturaPeticion.Texto(filtro, "numeroContrato"),
            LecturaPeticion.Bandera(filtro, "reservaVisita")), cancellationToken);
        return EjecucionMid.Ok(new { id = resultado });
    }

    private async Task<EjecucionMid> AnularAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var id = LecturaPeticion.Entero(filtro, "id");
        if (id <= 0)
            return EjecucionMid.Fallo(400, "Seleccione una solicitud válida.");

        await gestion.AnularAsync(id, sesion.EmpresaMandanteId, sesion.CampamentoId, sesion.UsuarioId, cancellationToken);
        return EjecucionMid.Ok(new { listo = true });
    }

    private async Task<EjecucionMid> CuposAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        await gestion.AgregarCuposAsync(
            LecturaPeticion.Entero(filtro, "id"), sesion.EmpresaMandanteId, sesion.CampamentoId,
            sesion.UsuarioId, LecturaPeticion.Entero(filtro, "cantidad"), cancellationToken);
        return EjecucionMid.Ok(new { listo = true });
    }

    private async Task<EjecucionMid> PasajeroAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var llegada = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fechaCheckIn"));
        if (llegada is null)
            return EjecucionMid.Fallo(400, "Ingrese la fecha de check-in del pasajero.");

        var id = await gestion.GuardarPasajeroAsync(new GuardarPasajeroSolicitudDto(
            LecturaPeticion.Entero(filtro, "solicitudId"),
            LecturaPeticion.Entero(filtro, "pasajeroId"),
            LecturaPeticion.Entero(filtro, "usuarioPasajeroId"),
            sesion.EmpresaMandanteId, sesion.CampamentoId, sesion.UsuarioId,
            llegada.Value, LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fechaCheckOut"))), cancellationToken);
        return EjecucionMid.Ok(new { id });
    }

    private async Task<EjecucionMid> EliminarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        await gestion.EliminarPasajeroAsync(
            LecturaPeticion.Entero(filtro, "id"), LecturaPeticion.Entero(filtro, "pasajeroId"),
            sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
        return EjecucionMid.Ok(new { listo = true });
    }

    private async Task<EjecucionMid> AsignarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        await gestion.AsignarHabitacionAsync(
            LecturaPeticion.Entero(filtro, "solicitudId"), LecturaPeticion.Entero(filtro, "pasajeroId"),
            LecturaPeticion.Entero(filtro, "habitacionId"), sesion.EmpresaMandanteId, sesion.CampamentoId,
            sesion.UsuarioId, cancellationToken);
        return EjecucionMid.Ok(new { listo = true });
    }

    private async Task<EjecucionMid> DesasignarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        await gestion.DesasignarHabitacionAsync(
            LecturaPeticion.Entero(filtro, "solicitudId"), LecturaPeticion.Entero(filtro, "pasajeroId"),
            sesion.EmpresaMandanteId, sesion.CampamentoId, sesion.UsuarioId, cancellationToken);
        return EjecucionMid.Ok(new { listo = true });
    }

    private async Task<EjecucionMid> GenerarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var id = LecturaPeticion.Entero(filtro, "id");
        if (id <= 0)
            return EjecucionMid.Fallo(400, "Seleccione una solicitud válida.");

        var primera = await gestion.GenerarReservasAsync(id, sesion.EmpresaMandanteId, sesion.CampamentoId, sesion.UsuarioId, cancellationToken);
        var advertencia = await NotificarAsync(id, sesion, cancellationToken);
        return EjecucionMid.Ok(new { primeraReservaId = primera, advertenciaCorreo = advertencia });
    }

    private async Task<EjecucionMid> NotificarOpcionAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var id = LecturaPeticion.Entero(filtro, "id");
        if (id <= 0)
            return EjecucionMid.Fallo(400, "Seleccione una solicitud válida.");

        var error = await NotificarAsync(id, sesion, cancellationToken);
        return error is null ? EjecucionMid.Ok(new { listo = true }) : EjecucionMid.Fallo(400, error);
    }

    private async Task<string?> NotificarAsync(int id, DatosSesion sesion, CancellationToken cancellationToken)
    {
        try
        {
            var solicitud = await gestion.ObtenerAsync(id, sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
            if (solicitud is null || solicitud.Estado != "R")
                return "La solicitud no existe en la sesión o no tiene reservas generadas.";

            var pasajeros = await gestion.ListarPasajerosAsync(id, sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
            await notificador.EnviarReservasGeneradasAsync(solicitud, pasajeros, cancellationToken);
            return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return "Las reservas están generadas, pero no se pudo enviar el correo. Revise la configuración y vuelva a intentarlo.";
        }
    }

    private static AccionApi Accion(string opcion, string etiqueta, string procedimiento, string descripcion) => new()
    {
        Id = opcion,
        Opcion = opcion,
        Etiqueta = etiqueta,
        Mid = Ruta,
        Procedimiento = procedimiento,
        Descripcion = descripcion
    };
}
