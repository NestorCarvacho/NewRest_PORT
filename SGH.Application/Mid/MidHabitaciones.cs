using SGH.Application.Habitacion.Interfaces;
using SGH.Application.Sesion;
using SGH.Domain.Habitacion.Interfaces;

namespace SGH.Application.Mid;

public sealed class MidHabitaciones(IGestionHabitaciones gestion, IPlanillaCamas planilla)
{
    public const string Ruta = "Mid/MidHabitaciones.cs";

    public static readonly IReadOnlyList<AccionApi> Opciones =
    [
        Accion("listar", "Listar", "dbo.he_sp_habitacion_listar", "Lista habitaciones del campamento."),
        Accion("catalogo", "Catálogo", "dbo.he_sp_habitacion_catalogo", "Pabellones, módulos, sexos y estados."),
        Accion("obtener", "Obtener", "dbo.he_sp_habitacion_obtener", "Detalle de una habitación."),
        Accion("crear", "Crear", "dbo.HAB_GuardaHabitacion", "Alta de habitación."),
        Accion("editar", "Editar", "dbo.HAB_GuardaHabitacion", "Actualización de habitación."),
        Accion("inactivar", "Inactivar", "dbo.HAB_GuardaHabitacion", "Baja lógica (IsActive=0)."),
        Accion("listarbanos", "Listar baños", "dbo.he_sp_bano_listar", "Baños del campamento."),
        Accion("crearbano", "Crear baño", "dbo.he_sp_bano_guardar", "Alta de baño."),
        Accion("editarbano", "Editar baño", "dbo.he_sp_bano_guardar", "Actualización de baño."),
        Accion("inactivarbano", "Inactivar baño", "dbo.he_sp_bano_guardar", "Baja lógica de baño."),
        Accion("activarbano", "Activar baño", "dbo.he_sp_bano_guardar", "Reactivación de baño."),
        Accion("listarcamas", "Listar camas", "dbo.he_sp_cama_listar_por_habitacion", "Camas de una habitación."),
        Accion("guardarcama", "Guardar cama", "dbo.he_sp_cama_guardar", "Alta o edición de cama."),
        Accion("listartiposcama", "Listar tipos de cama", "dbo.he_sp_tipo_cama_listar", "Catálogo tipos de cama."),
        Accion("creartipocama", "Crear tipo de cama", "dbo.he_sp_tipo_cama_guardar", "Alta tipo de cama."),
        Accion("editartipocama", "Editar tipo de cama", "dbo.he_sp_tipo_cama_guardar", "Edición tipo de cama."),
        Accion("inactivartipocama", "Inactivar tipo de cama", "dbo.he_sp_tipo_cama_guardar", "Baja lógica tipo de cama."),
        Accion("activartipocama", "Activar tipo de cama", "dbo.he_sp_tipo_cama_guardar", "Reactivación tipo de cama."),
        Accion("plantillacamas", "Plantilla camas", "planilla", "Descarga plantilla Excel de carga masiva."),
        Accion("cargarcamas", "Cargar camas", "carga", "Importa camas desde la planilla.")
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
            "listarbanos" => await ListarBanosAsync(filtro, sesion, cancellationToken),
            "crearbano" => await GuardarBanoAsync(filtro, sesion, esAlta: true, cancellationToken),
            "editarbano" => await GuardarBanoAsync(filtro, sesion, esAlta: false, cancellationToken),
            "inactivarbano" => await InactivarBanoAsync(filtro, sesion, cancellationToken),
            "activarbano" => await ActivarBanoAsync(filtro, sesion, cancellationToken),
            "listarcamas" => await ListarCamasAsync(filtro, sesion, cancellationToken),
            "guardarcama" => await GuardarCamaAsync(filtro, sesion, cancellationToken),
            "listartiposcama" => await ListarTiposCamaAsync(filtro, sesion, cancellationToken),
            "creartipocama" => await GuardarTipoCamaAsync(filtro, sesion, esAlta: true, cancellationToken),
            "editartipocama" => await GuardarTipoCamaAsync(filtro, sesion, esAlta: false, cancellationToken),
            "inactivartipocama" => await InactivarTipoCamaAsync(filtro, sesion, cancellationToken),
            "activartipocama" => await ActivarTipoCamaAsync(filtro, sesion, cancellationToken),
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
            null,
            LecturaPeticion.Entero(filtro, "habitacionId")), sesion.UsuarioId, cancellationToken);
        return EjecucionMid.Ok(RespuestaListado.ConTope(filas));
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
        var datos = LeerCuerpoHabitacion(filtro, sesion.UsuarioId, esAlta);
        if (datos is null)
            return EjecucionMid.Fallo(400, "Revise los datos del formulario.");
        var resultado = await gestion.GuardarAsync(
            datos, sesion.CampamentoId, sesion.EmpresaMandanteId, cancellationToken);
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
            actual.BanoId,
            actual.TiempoHigienizacionMin,
            sesion.UsuarioId);

        var resultado = await gestion.GuardarAsync(
            datos, sesion.CampamentoId, sesion.EmpresaMandanteId, cancellationToken);
        if (!resultado.Exito)
            return EjecucionMid.Fallo(400, resultado.Mensaje);
        return EjecucionMid.Ok(new { resultado = true, mensaje = "Habitación inactivada." });
    }

    private async Task<EjecucionMid> ListarBanosAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var soloActivos = LecturaPeticion.EnteroOpcional(filtro, "soloActivos") ?? 1;
        var banoId = LecturaPeticion.Entero(filtro, "banoId");
        var filas = await gestion.ListarBanosAsync(sesion.CampamentoId, soloActivos, banoId, cancellationToken);
        return EjecucionMid.Ok(RespuestaListado.ConTope(filas));
    }

    private async Task<EjecucionMid> GuardarBanoAsync(
        FiltroPeticion filtro, DatosSesion sesion, bool esAlta, CancellationToken cancellationToken)
    {
        var datos = LeerCuerpoBano(filtro, sesion.UsuarioId, esAlta);
        if (datos is null)
            return EjecucionMid.Fallo(400, "Revise los datos del baño.");
        if (!esAlta && LecturaPeticion.Campo(filtro, "estado") is null)
        {
            var listado = await gestion.ListarBanosAsync(sesion.CampamentoId, 0, 0, cancellationToken);
            var actual = listado.FirstOrDefault(b => b.Id == datos.Id);
            if (actual is not null)
                datos = datos with { Estado = actual.Estado, IsActive = actual.IsActive };
        }
        var resultado = await gestion.GuardarBanoAsync(datos, sesion.CampamentoId, cancellationToken);
        if (!resultado.Exito)
            return EjecucionMid.Fallo(400, resultado.Mensaje);
        return EjecucionMid.Ok(new { resultado = true, id = resultado.Id, mensaje = "Baño guardado." });
    }

    private async Task<EjecucionMid> InactivarBanoAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var id = LecturaPeticion.Entero(filtro, "id");
        if (id <= 0)
            return EjecucionMid.Fallo(400, "Seleccione un baño.");
        var filas = await gestion.ListarBanosAsync(sesion.CampamentoId, 0, 0, cancellationToken);
        var actual = filas.FirstOrDefault(b => b.Id == id);
        if (actual is null)
            return EjecucionMid.Fallo(404, "El baño no existe en el campamento.");

        var datos = new DatosGuardarBano(
            actual.Id,
            actual.CodigoNombre,
            actual.Ubicacion,
            actual.TipoSexo,
            "INACTIVO",
            false,
            sesion.UsuarioId);
        var resultado = await gestion.GuardarBanoAsync(datos, sesion.CampamentoId, cancellationToken);
        if (!resultado.Exito)
            return EjecucionMid.Fallo(400, resultado.Mensaje);
        return EjecucionMid.Ok(new { resultado = true, mensaje = "Baño inactivado." });
    }

    private async Task<EjecucionMid> ActivarBanoAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var id = LecturaPeticion.Entero(filtro, "id");
        if (id <= 0)
            return EjecucionMid.Fallo(400, "Seleccione un baño.");
        var filas = await gestion.ListarBanosAsync(sesion.CampamentoId, 0, 0, cancellationToken);
        var actual = filas.FirstOrDefault(b => b.Id == id);
        if (actual is null)
            return EjecucionMid.Fallo(404, "El baño no existe en el campamento.");

        var datos = new DatosGuardarBano(
            actual.Id,
            actual.CodigoNombre,
            actual.Ubicacion,
            actual.TipoSexo,
            "ACTIVO",
            true,
            sesion.UsuarioId);
        var resultado = await gestion.GuardarBanoAsync(datos, sesion.CampamentoId, cancellationToken);
        if (!resultado.Exito)
            return EjecucionMid.Fallo(400, resultado.Mensaje);
        return EjecucionMid.Ok(new { resultado = true, mensaje = "Baño activado." });
    }

    private async Task<EjecucionMid> ListarCamasAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var habitacionId = LecturaPeticion.Entero(filtro, "habitacionId");
        if (habitacionId <= 0)
            habitacionId = LecturaPeticion.Entero(filtro, "id");
        if (habitacionId <= 0)
            return EjecucionMid.Fallo(400, "Seleccione una habitación.");
        var soloActivas = LecturaPeticion.EnteroOpcional(filtro, "soloActivas") ?? 1;
        var filas = await gestion.ListarCamasAsync(habitacionId, sesion.CampamentoId, soloActivas, cancellationToken);
        return EjecucionMid.Ok(new { filas });
    }

    private async Task<EjecucionMid> GuardarCamaAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var datos = LeerCuerpoCama(filtro, sesion.UsuarioId);
        if (datos is null)
            return EjecucionMid.Fallo(400, "Revise los datos de la cama.");
        var resultado = await gestion.GuardarCamaAsync(datos, sesion.CampamentoId, cancellationToken);
        if (!resultado.Exito)
            return EjecucionMid.Fallo(400, resultado.Mensaje);
        return EjecucionMid.Ok(new { resultado = true, id = resultado.Id, mensaje = "Cama guardada." });
    }

    private async Task<EjecucionMid> ListarTiposCamaAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var soloActivos = LecturaPeticion.EnteroOpcional(filtro, "soloActivos") ?? 1;
        var tipoCamaId = LecturaPeticion.Entero(filtro, "tipoCamaId");
        var filas = await gestion.ListarTiposCamaAsync(sesion.CampamentoId, soloActivos, tipoCamaId, cancellationToken);
        return EjecucionMid.Ok(RespuestaListado.ConTope(filas));
    }

    private async Task<EjecucionMid> GuardarTipoCamaAsync(
        FiltroPeticion filtro, DatosSesion sesion, bool esAlta, CancellationToken cancellationToken)
    {
        var datos = LeerCuerpoTipoCama(filtro, sesion.UsuarioId, esAlta);
        if (datos is null)
            return EjecucionMid.Fallo(400, "Revise los datos del tipo de cama.");
        var resultado = await gestion.GuardarTipoCamaAsync(datos, sesion.CampamentoId, cancellationToken);
        if (!resultado.Exito)
            return EjecucionMid.Fallo(400, resultado.Mensaje);
        return EjecucionMid.Ok(new { resultado = true, id = resultado.Id, mensaje = "Tipo de cama guardado." });
    }

    private async Task<EjecucionMid> InactivarTipoCamaAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var id = LecturaPeticion.Entero(filtro, "id");
        if (id <= 0)
            return EjecucionMid.Fallo(400, "Seleccione un tipo de cama.");
        var filas = await gestion.ListarTiposCamaAsync(sesion.CampamentoId, 0, 0, cancellationToken);
        var actual = filas.FirstOrDefault(t => t.Id == id);
        if (actual is null)
            return EjecucionMid.Fallo(404, "El tipo de cama no existe en el campamento.");

        var datos = new DatosGuardarTipoCama(actual.Id, actual.Nombre, actual.AnchoCm, actual.LargoCm, false, sesion.UsuarioId);
        var resultado = await gestion.GuardarTipoCamaAsync(datos, sesion.CampamentoId, cancellationToken);
        if (!resultado.Exito)
            return EjecucionMid.Fallo(400, resultado.Mensaje);
        return EjecucionMid.Ok(new { resultado = true, mensaje = "Tipo de cama inactivado." });
    }

    private async Task<EjecucionMid> ActivarTipoCamaAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var id = LecturaPeticion.Entero(filtro, "id");
        if (id <= 0)
            return EjecucionMid.Fallo(400, "Seleccione un tipo de cama.");
        var filas = await gestion.ListarTiposCamaAsync(sesion.CampamentoId, 0, 0, cancellationToken);
        var actual = filas.FirstOrDefault(t => t.Id == id);
        if (actual is null)
            return EjecucionMid.Fallo(404, "El tipo de cama no existe en el campamento.");

        var datos = new DatosGuardarTipoCama(actual.Id, actual.Nombre, actual.AnchoCm, actual.LargoCm, true, sesion.UsuarioId);
        var resultado = await gestion.GuardarTipoCamaAsync(datos, sesion.CampamentoId, cancellationToken);
        if (!resultado.Exito)
            return EjecucionMid.Fallo(400, resultado.Mensaje);
        return EjecucionMid.Ok(new { resultado = true, mensaje = "Tipo de cama activado." });
    }

    private static DatosGuardarHabitacion? LeerCuerpoHabitacion(FiltroPeticion filtro, int usuarioId, bool esAlta)
    {
        var nombre = LecturaPeticion.Texto(filtro, "nombre");
        var moduloId = LecturaPeticion.Entero(filtro, "moduloId");
        var cantidadCamas = LecturaPeticion.EnteroOpcional(filtro, "cantidadCamas") ?? 1;
        var sexo = LecturaPeticion.Texto(filtro, "sexo");
        var id = esAlta ? 0 : LecturaPeticion.Entero(filtro, "id");
        if (!esAlta && id <= 0)
            return null;
        var banoId = LecturaPeticion.EnteroOpcional(filtro, "banoId");
        if (banoId is 0)
            banoId = null;
        var tiempoHig = LecturaPeticion.EnteroOpcional(filtro, "tiempoHigienizacionMin")
            ?? LecturaPeticion.EnteroOpcional(filtro, "minutosMin") ?? 0;

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
            banoId,
            tiempoHig,
            usuarioId);
    }

    private static DatosGuardarBano? LeerCuerpoBano(FiltroPeticion filtro, int usuarioId, bool esAlta)
    {
        var id = esAlta ? 0 : LecturaPeticion.Entero(filtro, "id");
        if (!esAlta && id <= 0)
            return null;
        var codigo = LecturaPeticion.Texto(filtro, "codigoNombre") ?? LecturaPeticion.Texto(filtro, "nombre");
        var ubicacion = LecturaPeticion.Texto(filtro, "ubicacion");
        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(ubicacion))
            return null;
        var estado = (LecturaPeticion.Texto(filtro, "estado") ?? "ACTIVO").Trim();
        var isActive = !string.Equals(estado, "INACTIVO", StringComparison.OrdinalIgnoreCase);
        if (LecturaPeticion.Bandera(filtro, "inactivo"))
            isActive = false;
        if (esAlta)
        {
            estado = "ACTIVO";
            isActive = true;
        }
        return new DatosGuardarBano(
            id,
            codigo.Trim(),
            ubicacion.Trim(),
            LecturaPeticion.Texto(filtro, "tipoSexo") ?? "MIXTO",
            estado,
            isActive,
            usuarioId);
    }

    private static DatosGuardarCama? LeerCuerpoCama(FiltroPeticion filtro, int usuarioId)
    {
        var habitacionId = LecturaPeticion.Entero(filtro, "habitacionId");
        var codigo = LecturaPeticion.Texto(filtro, "codigoCama");
        if (habitacionId <= 0 || string.IsNullOrWhiteSpace(codigo))
            return null;
        var tipoCamaId = LecturaPeticion.EnteroOpcional(filtro, "tipoCamaId");
        if (tipoCamaId is 0)
            tipoCamaId = null;

        var incluyeInventarioMandante = PeticionIncluyeInventarioMandante(filtro);

        return new DatosGuardarCama(
            LecturaPeticion.Entero(filtro, "id"),
            habitacionId,
            codigo.Trim(),
            LecturaPeticion.Campo(filtro, "habilitada") is null || LecturaPeticion.Bandera(filtro, "habilitada"),
            LecturaPeticion.EnteroOpcional(filtro, "orden") ?? 0,
            !LecturaPeticion.Bandera(filtro, "inactivo"),
            tipoCamaId,
            LecturaPeticion.FechaOpcional(filtro, "fechaColchon"),
            LecturaPeticion.Texto(filtro, "colchonObservacion"),
            LecturaPeticion.FechaOpcional(filtro, "fechaCambioRopa"),
            LecturaPeticion.Texto(filtro, "ropaCamaObservacion"),
            incluyeInventarioMandante,
            usuarioId);
    }

    private static bool PeticionIncluyeInventarioMandante(FiltroPeticion filtro) =>
        LecturaPeticion.Campo(filtro, "fechaColchon") is not null
        || LecturaPeticion.Campo(filtro, "colchonObservacion") is not null
        || LecturaPeticion.Campo(filtro, "fechaCambioRopa") is not null
        || LecturaPeticion.Campo(filtro, "ropaCamaObservacion") is not null;

    private static DatosGuardarTipoCama? LeerCuerpoTipoCama(FiltroPeticion filtro, int usuarioId, bool esAlta)
    {
        var nombre = LecturaPeticion.Texto(filtro, "nombre");
        var ancho = LecturaPeticion.EnteroOpcional(filtro, "anchoCm") ?? (short)0;
        var largo = LecturaPeticion.EnteroOpcional(filtro, "largoCm") ?? (short)0;
        if (string.IsNullOrWhiteSpace(nombre) || ancho <= 0 || largo <= 0)
            return null;

        var id = esAlta ? 0 : LecturaPeticion.Entero(filtro, "id");
        if (!esAlta && id <= 0)
            return null;

        var isActive = !LecturaPeticion.Bandera(filtro, "inactivo");
        if (esAlta)
            isActive = true;

        return new DatosGuardarTipoCama(
            id,
            nombre.Trim(),
            (short)ancho,
            (short)largo,
            isActive,
            usuarioId);
    }

    public async Task<EjecucionMid> PlantillaCamasAsync(DatosSesion sesion, CancellationToken cancellationToken)
    {
        var bloqueo = AlcanceMid.BloquearSiSinAlcance(sesion);
        if (bloqueo is not null)
            return bloqueo;

        var tipos = await gestion.ListarTiposCamaAsync(sesion.CampamentoId, 0, 0, cancellationToken);
        return EjecucionMid.Descarga(
            planilla.CrearPlantilla(sesion.CampamentoId, tipos),
            "PlantillaCamas.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    public async Task<EjecucionMid> CargarCamasAsync(Stream archivo, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var bloqueo = AlcanceMid.BloquearSiSinAlcance(sesion);
        if (bloqueo is not null)
            return bloqueo;

        var filas = planilla.Leer(archivo);
        var resultados = await gestion.CargarCamasAsync(sesion.CampamentoId, sesion.UsuarioId, filas, cancellationToken);
        return EjecucionMid.Ok(new { filas = resultados });
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