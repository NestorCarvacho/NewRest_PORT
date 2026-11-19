/*
  Inventario NewRest: qué existe en ESTA base (QA, local, prod).
  Ejecutar en SSMS contra cada servidor y guardar resultado (CSV / pegar en Excel).

  Comparar dos entornos:
  1) QA:   ejecutar → "inventario_qa.csv"
  2) Local: ejecutar → "inventario_local.csv"
  3) Diff (Git Bash / Linux): diff inventario_qa.csv inventario_local.csv
     o en Excel: dos columnas con VLOOKUP por "nuevo" / "legado".

  sqlcmd (ejemplo):
  sqlcmd -S QA -d HOTELDB -E -i he_sp_inventario_comparar.sql -s "|" -W -o inventario_qa.txt
*/
SET NOCOUNT ON;

DECLARE @map TABLE (
    legado sysname NOT NULL PRIMARY KEY,
    nuevo  sysname NOT NULL UNIQUE,
    modulo varchar(20) NOT NULL
);

INSERT INTO @map (legado, nuevo, modulo) VALUES
 (N'sp_Bano_Guardar', N'he_sp_bano_guardar', N'Habitacion'),
 (N'sp_Bano_HabitacionesActivasPorBano', N'he_sp_bano_habitaciones_activas_por_bano', N'Habitacion'),
 (N'sp_Bano_Listar', N'he_sp_bano_listar', N'Habitacion'),
 (N'sp_Cama_Guardar', N'he_sp_cama_guardar', N'Habitacion'),
 (N'sp_Cama_ListarPorHabitacion', N'he_sp_cama_listar_por_habitacion', N'Habitacion'),
 (N'sp_Censo_Buscar', N'he_sp_censo_buscar', N'Censo'),
 (N'sp_Censo_BuscarPorUsuario', N'he_sp_censo_buscar_por_usuario', N'Censo'),
 (N'sp_Censo_FiltroPertenece', N'he_sp_censo_filtro_pertenece', N'Censo'),
 (N'sp_Censo_Guardar', N'he_sp_censo_guardar', N'Censo'),
 (N'sp_Censo_ListarCatalogo', N'he_sp_censo_listar_catalogo', N'Censo'),
 (N'sp_Habitacion_Catalogo', N'he_sp_habitacion_catalogo', N'Habitacion'),
 (N'sp_Habitacion_Guardar', N'he_sp_habitacion_guardar', N'Habitacion'),
 (N'sp_Habitacion_IdPorNombre', N'he_sp_habitacion_id_por_nombre', N'Habitacion'),
 (N'sp_Habitacion_Listar', N'he_sp_habitacion_listar', N'Habitacion'),
 (N'sp_Habitacion_Obtener', N'he_sp_habitacion_obtener', N'Habitacion'),
 (N'sp_Habitacion_SincronizarCamas', N'he_sp_habitacion_sincronizar_camas', N'Habitacion'),
 (N'sp_Menu_ObtenerPorPerfil', N'he_sp_menu_obtener_por_perfil', N'Navegacion'),
 (N'sp_Reserva_Actualizar', N'he_sp_reserva_actualizar', N'Reserva'),
 (N'sp_Reserva_Buscar', N'he_sp_reserva_buscar', N'Reserva'),
 (N'sp_Reserva_Crear', N'he_sp_reserva_crear', N'Reserva'),
 (N'sp_Reserva_EstaEnCampamento', N'he_sp_reserva_esta_en_campamento', N'Reserva'),
 (N'sp_Reserva_FinalizarCheckout', N'he_sp_reserva_finalizar_checkout', N'Reserva'),
 (N'sp_Reserva_ListarCatalogo', N'he_sp_reserva_listar_catalogo', N'Reserva'),
 (N'sp_Reserva_RegistrarMovimiento', N'he_sp_reserva_registrar_movimiento', N'Reserva'),
 (N'sp_Solicitud_AgregarCupos', N'he_sp_solicitud_agregar_cupos', N'Solicitud'),
 (N'sp_Solicitud_Anular', N'he_sp_solicitud_anular', N'Solicitud'),
 (N'sp_Solicitud_AsegurarPeriodo', N'he_sp_solicitud_asegurar_periodo', N'Solicitud'),
 (N'sp_Solicitud_AsignarHabitacion', N'he_sp_solicitud_asignar_habitacion', N'Solicitud'),
 (N'sp_Solicitud_Buscar', N'he_sp_solicitud_buscar', N'Solicitud'),
 (N'sp_Solicitud_CargarPasajero', N'he_sp_solicitud_cargar_pasajero', N'Solicitud'),
 (N'sp_Solicitud_DesasignarHabitacion', N'he_sp_solicitud_desasignar_habitacion', N'Solicitud'),
 (N'sp_Solicitud_EliminarPasajero', N'he_sp_solicitud_eliminar_pasajero', N'Solicitud'),
 (N'sp_Solicitud_GenerarReservas', N'he_sp_solicitud_generar_reservas', N'Solicitud'),
 (N'sp_Solicitud_Guardar', N'he_sp_solicitud_guardar', N'Solicitud'),
 (N'sp_Solicitud_GuardarPasajero', N'he_sp_solicitud_guardar_pasajero', N'Solicitud'),
 (N'sp_Solicitud_HabitacionesDisponibles', N'he_sp_solicitud_habitaciones_disponibles', N'Solicitud'),
 (N'sp_Solicitud_ListarCatalogo', N'he_sp_solicitud_listar_catalogo', N'Solicitud'),
 (N'sp_Solicitud_ListarOrganizacion', N'he_sp_solicitud_listar_organizacion', N'Solicitud'),
 (N'sp_Solicitud_ListarPasajeros', N'he_sp_solicitud_listar_pasajeros', N'Solicitud'),
 (N'sp_Solicitud_ObtenerPrimeraReserva', N'he_sp_solicitud_obtener_primera_reserva', N'Solicitud'),
 (N'sp_TipoCama_Guardar', N'he_sp_tipo_cama_guardar', N'Habitacion'),
 (N'sp_TipoCama_Listar', N'he_sp_tipo_cama_listar', N'Habitacion'),
 (N'sp_Usuario_ActualizarPassword', N'he_sp_usuario_actualizar_password', N'Cuenta'),
 (N'sp_Usuario_CambiarPasswordPorRut', N'he_sp_usuario_cambiar_password_por_rut', N'Cuenta'),
 (N'sp_Usuario_ObtenerCampamentosYPerfiles', N'he_sp_usuario_obtener_campamentos_y_perfiles', N'Cuenta'),
 (N'sp_Usuario_ObtenerHistorialContrasenas', N'he_sp_usuario_obtener_historial_contrasenas', N'Cuenta'),
 (N'sp_Usuario_ObtenerParaLogin', N'he_sp_usuario_obtener_para_login', N'Cuenta'),
 (N'sp_Usuario_ReemplazarHash', N'he_sp_usuario_reemplazar_hash', N'Cuenta'),
 (N'sp_Usuario_RegistrarIntentoFallido', N'he_sp_usuario_registrar_intento_fallido', N'Cuenta'),
 (N'sp_Usuario_ReiniciarIntentos', N'he_sp_usuario_reiniciar_intentos', N'Cuenta'),
 (N'sp_Usuario_RetirarMd5', N'he_sp_usuario_retirar_md5', N'Cuenta'),
 (N'sp_Usuario_ValidarEstado', N'he_sp_usuario_validar_estado', N'Cuenta');

SELECT
    @@SERVERNAME AS servidor,
    DB_NAME() AS base_datos,
    GETDATE() AS fecha_consulta,
    m.modulo,
    m.legado,
    m.nuevo,
    CASE WHEN OBJECT_ID(N'dbo.' + m.legado, N'P') IS NOT NULL THEN 1 ELSE 0 END AS tiene_sp_legado,
    CASE WHEN OBJECT_ID(N'dbo.' + m.nuevo, N'P') IS NOT NULL THEN 1 ELSE 0 END AS tiene_he_sp_proc,
    CASE WHEN OBJECT_ID(N'dbo.' + m.nuevo, N'SN') IS NOT NULL THEN 1 ELSE 0 END AS tiene_he_sp_sinonimo,
    CASE
        WHEN OBJECT_ID(N'dbo.' + m.nuevo, N'P') IS NOT NULL THEN 'OK app (procedimiento)'
        WHEN OBJECT_ID(N'dbo.' + m.nuevo, N'SN') IS NOT NULL THEN 'puente sinonimo'
        WHEN OBJECT_ID(N'dbo.' + m.legado, N'P') IS NOT NULL THEN 'falta rename / script'
        ELSE 'no desplegado'
    END AS estado_newrest
FROM @map AS m
ORDER BY m.modulo, m.nuevo;

PRINT '';
PRINT '--- Resumen ---';

SELECT estado_newrest, COUNT(*) AS cantidad
FROM (
    SELECT
        CASE
            WHEN OBJECT_ID(N'dbo.' + m.nuevo, N'P') IS NOT NULL THEN 'OK app (procedimiento)'
            WHEN OBJECT_ID(N'dbo.' + m.nuevo, N'SN') IS NOT NULL THEN 'puente sinonimo'
            WHEN OBJECT_ID(N'dbo.' + m.legado, N'P') IS NOT NULL THEN 'falta rename / script'
            ELSE 'no desplegado'
        END AS estado_newrest
    FROM @map AS m
) AS x
GROUP BY estado_newrest
ORDER BY estado_newrest;

PRINT '';
PRINT '--- Todos he_sp_* en esta base (fuera del catálogo 52) ---';

SELECT p.name, p.modify_date
FROM sys.procedures AS p
WHERE p.schema_id = SCHEMA_ID('dbo')
  AND p.name LIKE 'he_sp[_]%'
  AND NOT EXISTS (SELECT 1 FROM @map AS m WHERE m.nuevo = p.name)
ORDER BY p.name;