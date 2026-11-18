/*
  Renombrado rápido sp_* → he_sp_* (NewRest).
  1) Ejecutá solo el bloque PREVIEW si querés ver qué se va a tocar.
  2) Ejecutá el bloque APLICAR (descomentar COMMIT si usás transacción).
  Post: re-ejecutar scripts Persistence del repo para actualizar EXEC internos.
*/

SET NOCOUNT ON;

DECLARE @map TABLE (
    legado sysname NOT NULL PRIMARY KEY,
    nuevo  sysname NOT NULL UNIQUE
);

INSERT INTO @map (legado, nuevo) VALUES
        (N'sp_Bano_Guardar', N'he_sp_bano_guardar'),
        (N'sp_Bano_HabitacionesActivasPorBano', N'he_sp_bano_habitaciones_activas_por_bano'),
        (N'sp_Bano_Listar', N'he_sp_bano_listar'),
        (N'sp_Cama_Guardar', N'he_sp_cama_guardar'),
        (N'sp_Cama_ListarPorHabitacion', N'he_sp_cama_listar_por_habitacion'),
        (N'sp_Censo_Buscar', N'he_sp_censo_buscar'),
        (N'sp_Censo_BuscarPorUsuario', N'he_sp_censo_buscar_por_usuario'),
        (N'sp_Censo_FiltroPertenece', N'he_sp_censo_filtro_pertenece'),
        (N'sp_Censo_Guardar', N'he_sp_censo_guardar'),
        (N'sp_Censo_ListarCatalogo', N'he_sp_censo_listar_catalogo'),
        (N'sp_Habitacion_Catalogo', N'he_sp_habitacion_catalogo'),
        (N'sp_Habitacion_Guardar', N'he_sp_habitacion_guardar'),
        (N'sp_Habitacion_IdPorNombre', N'he_sp_habitacion_id_por_nombre'),
        (N'sp_Habitacion_Listar', N'he_sp_habitacion_listar'),
        (N'sp_Habitacion_Obtener', N'he_sp_habitacion_obtener'),
        (N'sp_Habitacion_SincronizarCamas', N'he_sp_habitacion_sincronizar_camas'),
        (N'sp_Menu_ObtenerPorPerfil', N'he_sp_menu_obtener_por_perfil'),
        (N'sp_Reserva_Actualizar', N'he_sp_reserva_actualizar'),
        (N'sp_Reserva_Buscar', N'he_sp_reserva_buscar'),
        (N'sp_Reserva_Crear', N'he_sp_reserva_crear'),
        (N'sp_Reserva_EstaEnCampamento', N'he_sp_reserva_esta_en_campamento'),
        (N'sp_Reserva_FinalizarCheckout', N'he_sp_reserva_finalizar_checkout'),
        (N'sp_Reserva_ListarCatalogo', N'he_sp_reserva_listar_catalogo'),
        (N'sp_Reserva_RegistrarMovimiento', N'he_sp_reserva_registrar_movimiento'),
        (N'sp_Solicitud_AgregarCupos', N'he_sp_solicitud_agregar_cupos'),
        (N'sp_Solicitud_Anular', N'he_sp_solicitud_anular'),
        (N'sp_Solicitud_AsegurarPeriodo', N'he_sp_solicitud_asegurar_periodo'),
        (N'sp_Solicitud_AsignarHabitacion', N'he_sp_solicitud_asignar_habitacion'),
        (N'sp_Solicitud_Buscar', N'he_sp_solicitud_buscar'),
        (N'sp_Solicitud_CargarPasajero', N'he_sp_solicitud_cargar_pasajero'),
        (N'sp_Solicitud_DesasignarHabitacion', N'he_sp_solicitud_desasignar_habitacion'),
        (N'sp_Solicitud_EliminarPasajero', N'he_sp_solicitud_eliminar_pasajero'),
        (N'sp_Solicitud_GenerarReservas', N'he_sp_solicitud_generar_reservas'),
        (N'sp_Solicitud_Guardar', N'he_sp_solicitud_guardar'),
        (N'sp_Solicitud_GuardarPasajero', N'he_sp_solicitud_guardar_pasajero'),
        (N'sp_Solicitud_HabitacionesDisponibles', N'he_sp_solicitud_habitaciones_disponibles'),
        (N'sp_Solicitud_ListarCatalogo', N'he_sp_solicitud_listar_catalogo'),
        (N'sp_Solicitud_ListarOrganizacion', N'he_sp_solicitud_listar_organizacion'),
        (N'sp_Solicitud_ListarPasajeros', N'he_sp_solicitud_listar_pasajeros'),
        (N'sp_Solicitud_ObtenerPrimeraReserva', N'he_sp_solicitud_obtener_primera_reserva'),
        (N'sp_TipoCama_Guardar', N'he_sp_tipo_cama_guardar'),
        (N'sp_TipoCama_Listar', N'he_sp_tipo_cama_listar'),
        (N'sp_Usuario_ActualizarPassword', N'he_sp_usuario_actualizar_password'),
        (N'sp_Usuario_CambiarPasswordPorRut', N'he_sp_usuario_cambiar_password_por_rut'),
        (N'sp_Usuario_ObtenerCampamentosYPerfiles', N'he_sp_usuario_obtener_campamentos_y_perfiles'),
        (N'sp_Usuario_ObtenerHistorialContrasenas', N'he_sp_usuario_obtener_historial_contrasenas'),
        (N'sp_Usuario_ObtenerParaLogin', N'he_sp_usuario_obtener_para_login'),
        (N'sp_Usuario_ReemplazarHash', N'he_sp_usuario_reemplazar_hash'),
        (N'sp_Usuario_RegistrarIntentoFallido', N'he_sp_usuario_registrar_intento_fallido'),
        (N'sp_Usuario_ReiniciarIntentos', N'he_sp_usuario_reiniciar_intentos'),
        (N'sp_Usuario_RetirarMd5', N'he_sp_usuario_retirar_md5'),
        (N'sp_Usuario_ValidarEstado', N'he_sp_usuario_validar_estado');

/* ========== PREVIEW (solo lectura) ========== */
SELECT
    m.legado,
    m.nuevo,
    CASE
        WHEN OBJECT_ID(N'dbo.' + m.legado, N'P') IS NULL AND OBJECT_ID(N'dbo.' + m.nuevo, N'P') IS NOT NULL THEN 'ya he_sp'
        WHEN OBJECT_ID(N'dbo.' + m.legado, N'P') IS NULL AND OBJECT_ID(N'dbo.' + m.nuevo, N'SN') IS NOT NULL THEN 'solo sinonimo'
        WHEN OBJECT_ID(N'dbo.' + m.legado, N'P') IS NOT NULL AND OBJECT_ID(N'dbo.' + m.nuevo, N'P') IS NOT NULL THEN 'duplicado (se borra legado)'
        WHEN OBJECT_ID(N'dbo.' + m.legado, N'P') IS NOT NULL AND OBJECT_ID(N'dbo.' + m.nuevo, N'P') IS NULL THEN 'renombrar'
        ELSE 'no existe'
    END AS accion
FROM @map AS m
ORDER BY m.legado;

/* ========== APLICAR ========== */
DECLARE @legado sysname, @nuevo sysname, @sql nvarchar(4000);

DECLARE c CURSOR LOCAL FAST_FORWARD FOR
    SELECT legado, nuevo FROM @map ORDER BY legado;

OPEN c;
FETCH NEXT FROM c INTO @legado, @nuevo;

WHILE @@FETCH_STATUS = 0
BEGIN
  IF OBJECT_ID(N'dbo.' + @nuevo, N'SN') IS NOT NULL
  BEGIN
    SET @sql = N'DROP SYNONYM ' + QUOTENAME(N'dbo.' + @nuevo);
    EXEC sp_executesql @sql;
  END;

  IF OBJECT_ID(N'dbo.' + @legado, N'P') IS NOT NULL AND OBJECT_ID(N'dbo.' + @nuevo, N'P') IS NOT NULL
  BEGIN
    SET @sql = N'DROP PROCEDURE ' + QUOTENAME(N'dbo.' + @legado);
    EXEC sp_executesql @sql;
  END;

  IF OBJECT_ID(N'dbo.' + @legado, N'P') IS NOT NULL AND OBJECT_ID(N'dbo.' + @nuevo, N'P') IS NULL
  BEGIN
    SET @sql = N'EXEC sys.sp_rename @objname = N''dbo.' + REPLACE(@legado, '''', '''''') + N''', @newname = N''' + REPLACE(@nuevo, '''', '''''') + N''', @objtype = N''OBJECT'';';
    EXEC sp_executesql @sql;
    PRINT N'Renombrado: ' + @legado + N' -> ' + @nuevo;
  END;

  FETCH NEXT FROM c INTO @legado, @nuevo;
END;

CLOSE c;
DEALLOCATE c;

/* Verificación */
SELECT name, type_desc, modify_date
FROM sys.objects
WHERE name LIKE 'he_sp_%' AND type IN ('P', 'SN')
ORDER BY name;
