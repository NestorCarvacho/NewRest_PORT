/* Ejecutar en QA y en local; comparar resultados (mismas columnas, distinto @@SERVERNAME).
   Lista los objetos dbo que existen entre los 52 he_sp_* y 52 sp_* del catálogo NewRest. */

SET NOCOUNT ON;

DECLARE @catalogo TABLE (name sysname NOT NULL PRIMARY KEY);

INSERT INTO @catalogo (name) VALUES
    (N'he_sp_bano_guardar'),
    (N'he_sp_bano_habitaciones_activas_por_bano'),
    (N'he_sp_bano_listar'),
    (N'he_sp_cama_guardar'),
    (N'he_sp_cama_listar_por_habitacion'),
    (N'he_sp_censo_buscar'),
    (N'he_sp_censo_buscar_por_usuario'),
    (N'he_sp_censo_filtro_pertenece'),
    (N'he_sp_censo_guardar'),
    (N'he_sp_censo_listar_catalogo'),
    (N'he_sp_censo_obtener_habitacion_qr'),
    (N'he_sp_habitacion_catalogo'),
    (N'he_sp_habitacion_guardar'),
    (N'he_sp_habitacion_id_por_nombre'),
    (N'he_sp_habitacion_listar'),
    (N'he_sp_habitacion_obtener'),
    (N'he_sp_habitacion_sincronizar_camas'),
    (N'he_sp_menu_obtener_por_perfil'),
    (N'he_sp_reserva_actualizar'),
    (N'he_sp_reserva_buscar'),
    (N'he_sp_reserva_crear'),
    (N'he_sp_reserva_esta_en_campamento'),
    (N'he_sp_reserva_finalizar_checkout'),
    (N'he_sp_reserva_listar_catalogo'),
    (N'he_sp_reserva_registrar_movimiento'),
    (N'he_sp_solicitud_agregar_cupos'),
    (N'he_sp_solicitud_anular'),
    (N'he_sp_solicitud_asegurar_periodo'),
    (N'he_sp_solicitud_asignar_habitacion'),
    (N'he_sp_solicitud_buscar'),
    (N'he_sp_solicitud_cargar_pasajero'),
    (N'he_sp_solicitud_desasignar_habitacion'),
    (N'he_sp_solicitud_eliminar_pasajero'),
    (N'he_sp_solicitud_generar_reservas'),
    (N'he_sp_solicitud_guardar'),
    (N'he_sp_solicitud_guardar_pasajero'),
    (N'he_sp_solicitud_habitaciones_disponibles'),
    (N'he_sp_solicitud_listar_catalogo'),
    (N'he_sp_solicitud_listar_organizacion'),
    (N'he_sp_solicitud_listar_pasajeros'),
    (N'he_sp_solicitud_obtener_primera_reserva'),
    (N'he_sp_tipo_cama_guardar'),
    (N'he_sp_tipo_cama_listar'),
    (N'he_sp_usuario_actualizar_password'),
    (N'he_sp_usuario_cambiar_password_por_rut'),
    (N'he_sp_usuario_obtener_campamentos_y_perfiles'),
    (N'he_sp_usuario_obtener_historial_contrasenas'),
    (N'he_sp_usuario_obtener_para_login'),
    (N'he_sp_usuario_reemplazar_hash'),
    (N'he_sp_usuario_registrar_intento_fallido'),
    (N'he_sp_usuario_reiniciar_intentos'),
    (N'he_sp_usuario_retirar_md5'),
    (N'he_sp_usuario_validar_estado'),
    (N'sp_Bano_Guardar'),
    (N'sp_Bano_HabitacionesActivasPorBano'),
    (N'sp_Bano_Listar'),
    (N'sp_Cama_Guardar'),
    (N'sp_Cama_ListarPorHabitacion'),
    (N'sp_Censo_Buscar'),
    (N'sp_Censo_BuscarPorUsuario'),
    (N'sp_Censo_FiltroPertenece'),
    (N'sp_Censo_Guardar'),
    (N'sp_Censo_ListarCatalogo'),
    (N'sp_Habitacion_Catalogo'),
    (N'sp_Habitacion_Guardar'),
    (N'sp_Habitacion_IdPorNombre'),
    (N'sp_Habitacion_Listar'),
    (N'sp_Habitacion_Obtener'),
    (N'sp_Habitacion_SincronizarCamas'),
    (N'sp_Menu_ObtenerPorPerfil'),
    (N'sp_Reserva_Actualizar'),
    (N'sp_Reserva_Buscar'),
    (N'sp_Reserva_Crear'),
    (N'sp_Reserva_EstaEnCampamento'),
    (N'sp_Reserva_FinalizarCheckout'),
    (N'sp_Reserva_ListarCatalogo'),
    (N'sp_Reserva_RegistrarMovimiento'),
    (N'sp_Solicitud_AgregarCupos'),
    (N'sp_Solicitud_Anular'),
    (N'sp_Solicitud_AsegurarPeriodo'),
    (N'sp_Solicitud_AsignarHabitacion'),
    (N'sp_Solicitud_Buscar'),
    (N'sp_Solicitud_CargarPasajero'),
    (N'sp_Solicitud_DesasignarHabitacion'),
    (N'sp_Solicitud_EliminarPasajero'),
    (N'sp_Solicitud_GenerarReservas'),
    (N'sp_Solicitud_Guardar'),
    (N'sp_Solicitud_GuardarPasajero'),
    (N'sp_Solicitud_HabitacionesDisponibles'),
    (N'sp_Solicitud_ListarCatalogo'),
    (N'sp_Solicitud_ListarOrganizacion'),
    (N'sp_Solicitud_ListarPasajeros'),
    (N'sp_Solicitud_ObtenerPrimeraReserva'),
    (N'sp_TipoCama_Guardar'),
    (N'sp_TipoCama_Listar'),
    (N'sp_Usuario_ActualizarPassword'),
    (N'sp_Usuario_CambiarPasswordPorRut'),
    (N'sp_Usuario_ObtenerCampamentosYPerfiles'),
    (N'sp_Usuario_ObtenerHistorialContrasenas'),
    (N'sp_Usuario_ObtenerParaLogin'),
    (N'sp_Usuario_ReemplazarHash'),
    (N'sp_Usuario_RegistrarIntentoFallido'),
    (N'sp_Usuario_ReiniciarIntentos'),
    (N'sp_Usuario_RetirarMd5'),
    (N'sp_Usuario_ValidarEstado');

/* --- Los que SÍ están en esta base --- */
SELECT
    @@SERVERNAME AS servidor,
    DB_NAME() AS base_datos,
    o.name,
    o.type_desc,
    o.modify_date
FROM sys.objects AS o
INNER JOIN @catalogo AS c ON c.name = o.name
WHERE o.schema_id = SCHEMA_ID('dbo')
ORDER BY o.name;

/* --- Los que NO están (del catálogo) --- */
SELECT
    @@SERVERNAME AS servidor,
    DB_NAME() AS base_datos,
    c.name AS falta_en_base
FROM @catalogo AS c
WHERE NOT EXISTS (
    SELECT 1
    FROM sys.objects AS o
    WHERE o.schema_id = SCHEMA_ID('dbo')
      AND o.name = c.name
)
ORDER BY c.name;

SELECT
    @@SERVERNAME AS servidor,
    DB_NAME() AS base_datos,
    SUM(CASE WHEN o.object_id IS NOT NULL THEN 1 ELSE 0 END) AS existen,
    COUNT(*) AS catalogo_total
FROM @catalogo AS c
LEFT JOIN sys.objects AS o
    ON o.schema_id = SCHEMA_ID('dbo') AND o.name = c.name;
