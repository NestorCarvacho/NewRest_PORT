using SGH.Application.Sesion;
using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.Interfaces;
using SGH.Domain.Consulta;

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
        Accion("crear", "Crear", "dbo.he_sp_solicitud_guardar", "Crea una solicitud."),
        Accion("editar", "Editar", "dbo.he_sp_solicitud_guardar", "Actualiza una solicitud."),
        Accion("anular", "Anular", "dbo.he_sp_solicitud_anular", "Anula una solicitud."),
        Accion("cupos", "Cupos", "dbo.he_sp_solicitud_agregar_cupos", "Agrega cupos."),
        Accion("pasajero", "Pasajero", "dbo.he_sp_solicitud_guardar_pasajero", "Guarda un pasajero."),
        Accion("eliminar", "Eliminar pasajero", "dbo.he_sp_solicitud_eliminar_pasajero", "Quita un pasajero."),
        Accion("asignar", "Asignar", "dbo.he_sp_solicitud_asignar_habitacion", "Asigna una habitación."),
        Accion("desasignar", "Desasignar", "dbo.he_sp_solicitud_desasignar_habitacion", "Quita la habitación preasignada."),
        Accion("generar", "Generar reservas", "dbo.he_sp_solicitud_generar_reservas", "Genera las reservas de la solicitud."),
        Accion("notificar", "Notificar", "correo", "Reenvía el correo de reservas generadas."),
        Accion("plantilla", "Plantilla", "planilla", "Devuelve la planilla de pasajeros."),
        Accion("cargar", "Cargar", "dbo.he_sp_solicitud_cargar_pasajero", "Importa pasajeros desde la planilla.")
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
        var rango = ReglasConsultaListado.NormalizarRango(desde, hasta);

        var criterios = new FiltroSolicitudesDto(
            sesion.EmpresaMandanteId, sesion.CampamentoId, rango.Desde, rango.Hasta,
            LecturaPeticion.Entero(filtro, "grupoId"), LecturaPeticion.Entero(filtro, "solicitanteId"),
            LecturaPeticion.Texto(filtro, "estado") is { Length: > 0 } estado ? estado : "0",
            LecturaPeticion.Entero(filtro, "numero"));
        var filas = await gestion.BuscarAsync(criterios, cancellationToken);
        var (filasRecorte, truncado) = ReglasConsultaListado.AplicarTopeFilas(filas);
        return EjecucionMid.Ok(new
        {
            filas = filasRecorte,
            truncado,
            maxFilas = ReglasConsultaListado.MaxFilas,
            depuracion = RegistrosDepuracion.Invocacion("dbo.he_sp_solicitud_buscar", new
            {
                empresaMandanteId = criterios.EmpresaMandanteId,
                campamentoId = criterios.CampamentoId,
                criterios.Desde,
                criterios.Hasta,
                grupoId = criterios.GrupoId,
                solicitanteId = criterios.SolicitanteId,
                criterios.Estado,
                numero = criterios.Numero
            })
        });
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
        if (detalle is null)
            return EjecucionMid.Fallo(404, "Solicitud no encontrada.");
        var id = LecturaPeticion.Entero(filtro, "id");
        return EjecucionMid.Ok(new
        {
            detalle,
            depuracion = RegistrosDepuracion.Invocacion("dbo.SOL_ObtieneSolicitudAlojamiento", new { id })
        });
    }

    private async Task<EjecucionMid> PasajerosAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var solicitudId = LecturaPeticion.Entero(filtro, "id");
        var pasajeros = await gestion.ListarPasajerosAsync(solicitudId, sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
        return EjecucionMid.Ok(new
        {
            pasajeros,
            depuracion = RegistrosDepuracion.Invocacion("dbo.he_sp_solicitud_listar_pasajeros", new
            {
                solicitudId,
                empresaMandanteId = sesion.EmpresaMandanteId,
                campamentoId = sesion.CampamentoId
            })
        });
    }

    private async Task<EjecucionMid> HabitacionesAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var solicitudId = LecturaPeticion.Entero(filtro, "id");
        var detalle = await gestion.ObtenerAsync(solicitudId, sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
        if (detalle is null)
            return EjecucionMid.Fallo(404, "Solicitud no encontrada.");
        var habitaciones = await gestion.HabitacionesDisponiblesAsync(solicitudId, sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
        return EjecucionMid.Ok(new
        {
            habitaciones,
            depuracion = new
            {
                invocaciones = new object[]
                {
                    RegistrosDepuracion.Invocacion("dbo.he_sp_solicitud_habitaciones_disponibles", new
                    {
                        solicitudId,
                        empresaMandanteId = sesion.EmpresaMandanteId,
                        campamentoId = sesion.CampamentoId
                    }),
                    RegistrosDepuracion.Invocacion("dbo.HAB_ObtieneHabitacionesDisponiblesV2", new
                    {
                        grupoEmpresa = detalle.AreaEmpresaSolicitanteId,
                        empresaContratista = detalle.EmpresaContratistaId,
                        modulo = 0,
                        piso = 0,
                        habitacion = 0,
                        fechaLlegada = detalle.FechaCheckIn,
                        fechaSalida = detalle.FechaCheckOut,
                        empresaMandante = sesion.EmpresaMandanteId,
                        campamento = sesion.CampamentoId,
                        minera = (int?)null,
                        tipoModuloId = 0,
                        tipoInstalacion = 0,
                        grupoServicioId = 0,
                        nota = "En legado @minera es Empresas.MineraId del mandante; NewRest no lo pasa al V2 (0 implícito). tipoModuloId, tipoInstalacion y grupoServicioId tampoco se envían desde solicitud."
                    })
                }
            }
        });
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
