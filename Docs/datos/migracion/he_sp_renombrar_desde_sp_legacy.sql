/* Renombra en HOTELDB los dbo.sp_* del repo a dbo.he_sp_* (mismo cuerpo, nuevo nombre).
   Idempotente. Quita sinónimos he_sp_* previos. Si ya existen ambos P, elimina el sp_* legado.
   Después ejecutar los .sql de SGH.Infrastructure/**/Persistence/ para actualizar EXEC internos a he_sp_*. */
SET NOCOUNT ON;

-- sp_Bano_Guardar -> he_sp_bano_guardar
IF OBJECT_ID(N'dbo.he_sp_bano_guardar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_bano_guardar;
GO

IF OBJECT_ID(N'dbo.sp_Bano_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_bano_guardar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Bano_Guardar;
GO

IF OBJECT_ID(N'dbo.sp_Bano_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_bano_guardar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Bano_Guardar', N'he_sp_bano_guardar', N'OBJECT';
GO

-- sp_Bano_HabitacionesActivasPorBano -> he_sp_bano_habitaciones_activas_por_bano
IF OBJECT_ID(N'dbo.he_sp_bano_habitaciones_activas_por_bano', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_bano_habitaciones_activas_por_bano;
GO

IF OBJECT_ID(N'dbo.sp_Bano_HabitacionesActivasPorBano', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_bano_habitaciones_activas_por_bano', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Bano_HabitacionesActivasPorBano;
GO

IF OBJECT_ID(N'dbo.sp_Bano_HabitacionesActivasPorBano', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_bano_habitaciones_activas_por_bano', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Bano_HabitacionesActivasPorBano', N'he_sp_bano_habitaciones_activas_por_bano', N'OBJECT';
GO

-- sp_Bano_Listar -> he_sp_bano_listar
IF OBJECT_ID(N'dbo.he_sp_bano_listar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_bano_listar;
GO

IF OBJECT_ID(N'dbo.sp_Bano_Listar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_bano_listar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Bano_Listar;
GO

IF OBJECT_ID(N'dbo.sp_Bano_Listar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_bano_listar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Bano_Listar', N'he_sp_bano_listar', N'OBJECT';
GO

-- sp_Cama_Guardar -> he_sp_cama_guardar
IF OBJECT_ID(N'dbo.he_sp_cama_guardar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_cama_guardar;
GO

IF OBJECT_ID(N'dbo.sp_Cama_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_cama_guardar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Cama_Guardar;
GO

IF OBJECT_ID(N'dbo.sp_Cama_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_cama_guardar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Cama_Guardar', N'he_sp_cama_guardar', N'OBJECT';
GO

-- sp_Cama_ListarPorHabitacion -> he_sp_cama_listar_por_habitacion
IF OBJECT_ID(N'dbo.he_sp_cama_listar_por_habitacion', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_cama_listar_por_habitacion;
GO

IF OBJECT_ID(N'dbo.sp_Cama_ListarPorHabitacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_cama_listar_por_habitacion', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Cama_ListarPorHabitacion;
GO

IF OBJECT_ID(N'dbo.sp_Cama_ListarPorHabitacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_cama_listar_por_habitacion', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Cama_ListarPorHabitacion', N'he_sp_cama_listar_por_habitacion', N'OBJECT';
GO

-- sp_Censo_Buscar -> he_sp_censo_buscar
IF OBJECT_ID(N'dbo.he_sp_censo_buscar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_censo_buscar;
GO

IF OBJECT_ID(N'dbo.sp_Censo_Buscar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_buscar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Censo_Buscar;
GO

IF OBJECT_ID(N'dbo.sp_Censo_Buscar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_buscar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Censo_Buscar', N'he_sp_censo_buscar', N'OBJECT';
GO

-- sp_Censo_BuscarPorUsuario -> he_sp_censo_buscar_por_usuario
IF OBJECT_ID(N'dbo.he_sp_censo_buscar_por_usuario', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_censo_buscar_por_usuario;
GO

IF OBJECT_ID(N'dbo.sp_Censo_BuscarPorUsuario', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_buscar_por_usuario', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Censo_BuscarPorUsuario;
GO

IF OBJECT_ID(N'dbo.sp_Censo_BuscarPorUsuario', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_buscar_por_usuario', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Censo_BuscarPorUsuario', N'he_sp_censo_buscar_por_usuario', N'OBJECT';
GO

-- sp_Censo_FiltroPertenece -> he_sp_censo_filtro_pertenece
IF OBJECT_ID(N'dbo.he_sp_censo_filtro_pertenece', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_censo_filtro_pertenece;
GO

IF OBJECT_ID(N'dbo.sp_Censo_FiltroPertenece', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_filtro_pertenece', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Censo_FiltroPertenece;
GO

IF OBJECT_ID(N'dbo.sp_Censo_FiltroPertenece', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_filtro_pertenece', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Censo_FiltroPertenece', N'he_sp_censo_filtro_pertenece', N'OBJECT';
GO

-- sp_Censo_Guardar -> he_sp_censo_guardar
IF OBJECT_ID(N'dbo.he_sp_censo_guardar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_censo_guardar;
GO

IF OBJECT_ID(N'dbo.sp_Censo_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_guardar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Censo_Guardar;
GO

IF OBJECT_ID(N'dbo.sp_Censo_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_guardar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Censo_Guardar', N'he_sp_censo_guardar', N'OBJECT';
GO

-- sp_Censo_ListarCatalogo -> he_sp_censo_listar_catalogo
IF OBJECT_ID(N'dbo.he_sp_censo_listar_catalogo', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_censo_listar_catalogo;
GO

IF OBJECT_ID(N'dbo.sp_Censo_ListarCatalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_listar_catalogo', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Censo_ListarCatalogo;
GO

IF OBJECT_ID(N'dbo.sp_Censo_ListarCatalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_listar_catalogo', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Censo_ListarCatalogo', N'he_sp_censo_listar_catalogo', N'OBJECT';
GO

-- sp_Habitacion_Catalogo -> he_sp_habitacion_catalogo
IF OBJECT_ID(N'dbo.he_sp_habitacion_catalogo', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_habitacion_catalogo;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Catalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_catalogo', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Habitacion_Catalogo;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Catalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_catalogo', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Habitacion_Catalogo', N'he_sp_habitacion_catalogo', N'OBJECT';
GO

-- sp_Habitacion_Guardar -> he_sp_habitacion_guardar
IF OBJECT_ID(N'dbo.he_sp_habitacion_guardar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_habitacion_guardar;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_guardar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Habitacion_Guardar;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_guardar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Habitacion_Guardar', N'he_sp_habitacion_guardar', N'OBJECT';
GO

-- sp_Habitacion_IdPorNombre -> he_sp_habitacion_id_por_nombre
IF OBJECT_ID(N'dbo.he_sp_habitacion_id_por_nombre', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_habitacion_id_por_nombre;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_IdPorNombre', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_id_por_nombre', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Habitacion_IdPorNombre;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_IdPorNombre', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_id_por_nombre', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Habitacion_IdPorNombre', N'he_sp_habitacion_id_por_nombre', N'OBJECT';
GO

-- sp_Habitacion_Listar -> he_sp_habitacion_listar
IF OBJECT_ID(N'dbo.he_sp_habitacion_listar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_habitacion_listar;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Listar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_listar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Habitacion_Listar;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Listar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_listar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Habitacion_Listar', N'he_sp_habitacion_listar', N'OBJECT';
GO

-- sp_Habitacion_Obtener -> he_sp_habitacion_obtener
IF OBJECT_ID(N'dbo.he_sp_habitacion_obtener', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_habitacion_obtener;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Obtener', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_obtener', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Habitacion_Obtener;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Obtener', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_obtener', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Habitacion_Obtener', N'he_sp_habitacion_obtener', N'OBJECT';
GO

-- sp_Habitacion_SincronizarCamas -> he_sp_habitacion_sincronizar_camas
IF OBJECT_ID(N'dbo.he_sp_habitacion_sincronizar_camas', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_habitacion_sincronizar_camas;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_SincronizarCamas', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_sincronizar_camas', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Habitacion_SincronizarCamas;
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_SincronizarCamas', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_sincronizar_camas', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Habitacion_SincronizarCamas', N'he_sp_habitacion_sincronizar_camas', N'OBJECT';
GO

-- sp_Menu_ObtenerPorPerfil -> he_sp_menu_obtener_por_perfil
IF OBJECT_ID(N'dbo.he_sp_menu_obtener_por_perfil', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_menu_obtener_por_perfil;
GO

IF OBJECT_ID(N'dbo.sp_Menu_ObtenerPorPerfil', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_menu_obtener_por_perfil', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Menu_ObtenerPorPerfil;
GO

IF OBJECT_ID(N'dbo.sp_Menu_ObtenerPorPerfil', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_menu_obtener_por_perfil', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Menu_ObtenerPorPerfil', N'he_sp_menu_obtener_por_perfil', N'OBJECT';
GO

-- sp_Reserva_Actualizar -> he_sp_reserva_actualizar
IF OBJECT_ID(N'dbo.he_sp_reserva_actualizar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_reserva_actualizar;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_Actualizar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_actualizar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Reserva_Actualizar;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_Actualizar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_actualizar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Reserva_Actualizar', N'he_sp_reserva_actualizar', N'OBJECT';
GO

-- sp_Reserva_Buscar -> he_sp_reserva_buscar
IF OBJECT_ID(N'dbo.he_sp_reserva_buscar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_reserva_buscar;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_Buscar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_buscar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Reserva_Buscar;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_Buscar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_buscar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Reserva_Buscar', N'he_sp_reserva_buscar', N'OBJECT';
GO

-- sp_Reserva_Crear -> he_sp_reserva_crear
IF OBJECT_ID(N'dbo.he_sp_reserva_crear', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_reserva_crear;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_Crear', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_crear', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Reserva_Crear;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_Crear', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_crear', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Reserva_Crear', N'he_sp_reserva_crear', N'OBJECT';
GO

-- sp_Reserva_EstaEnCampamento -> he_sp_reserva_esta_en_campamento
IF OBJECT_ID(N'dbo.he_sp_reserva_esta_en_campamento', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_reserva_esta_en_campamento;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_EstaEnCampamento', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_esta_en_campamento', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Reserva_EstaEnCampamento;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_EstaEnCampamento', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_esta_en_campamento', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Reserva_EstaEnCampamento', N'he_sp_reserva_esta_en_campamento', N'OBJECT';
GO

-- sp_Reserva_FinalizarCheckout -> he_sp_reserva_finalizar_checkout
IF OBJECT_ID(N'dbo.he_sp_reserva_finalizar_checkout', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_reserva_finalizar_checkout;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_FinalizarCheckout', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_finalizar_checkout', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Reserva_FinalizarCheckout;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_FinalizarCheckout', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_finalizar_checkout', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Reserva_FinalizarCheckout', N'he_sp_reserva_finalizar_checkout', N'OBJECT';
GO

-- sp_Reserva_ListarCatalogo -> he_sp_reserva_listar_catalogo
IF OBJECT_ID(N'dbo.he_sp_reserva_listar_catalogo', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_reserva_listar_catalogo;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_ListarCatalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_listar_catalogo', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Reserva_ListarCatalogo;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_ListarCatalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_listar_catalogo', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Reserva_ListarCatalogo', N'he_sp_reserva_listar_catalogo', N'OBJECT';
GO

-- sp_Reserva_RegistrarMovimiento -> he_sp_reserva_registrar_movimiento
IF OBJECT_ID(N'dbo.he_sp_reserva_registrar_movimiento', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_reserva_registrar_movimiento;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_RegistrarMovimiento', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_registrar_movimiento', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Reserva_RegistrarMovimiento;
GO

IF OBJECT_ID(N'dbo.sp_Reserva_RegistrarMovimiento', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_registrar_movimiento', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Reserva_RegistrarMovimiento', N'he_sp_reserva_registrar_movimiento', N'OBJECT';
GO

-- sp_Solicitud_AgregarCupos -> he_sp_solicitud_agregar_cupos
IF OBJECT_ID(N'dbo.he_sp_solicitud_agregar_cupos', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_agregar_cupos;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_AgregarCupos', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_agregar_cupos', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_AgregarCupos;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_AgregarCupos', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_agregar_cupos', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_AgregarCupos', N'he_sp_solicitud_agregar_cupos', N'OBJECT';
GO

-- sp_Solicitud_Anular -> he_sp_solicitud_anular
IF OBJECT_ID(N'dbo.he_sp_solicitud_anular', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_anular;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_Anular', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_anular', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_Anular;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_Anular', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_anular', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_Anular', N'he_sp_solicitud_anular', N'OBJECT';
GO

-- sp_Solicitud_AsegurarPeriodo -> he_sp_solicitud_asegurar_periodo
IF OBJECT_ID(N'dbo.he_sp_solicitud_asegurar_periodo', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_asegurar_periodo;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_AsegurarPeriodo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_asegurar_periodo', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_AsegurarPeriodo;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_AsegurarPeriodo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_asegurar_periodo', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_AsegurarPeriodo', N'he_sp_solicitud_asegurar_periodo', N'OBJECT';
GO

-- sp_Solicitud_AsignarHabitacion -> he_sp_solicitud_asignar_habitacion
IF OBJECT_ID(N'dbo.he_sp_solicitud_asignar_habitacion', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_asignar_habitacion;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_AsignarHabitacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_asignar_habitacion', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_AsignarHabitacion;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_AsignarHabitacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_asignar_habitacion', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_AsignarHabitacion', N'he_sp_solicitud_asignar_habitacion', N'OBJECT';
GO

-- sp_Solicitud_Buscar -> he_sp_solicitud_buscar
IF OBJECT_ID(N'dbo.he_sp_solicitud_buscar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_buscar;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_Buscar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_buscar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_Buscar;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_Buscar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_buscar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_Buscar', N'he_sp_solicitud_buscar', N'OBJECT';
GO

-- sp_Solicitud_CargarPasajero -> he_sp_solicitud_cargar_pasajero
IF OBJECT_ID(N'dbo.he_sp_solicitud_cargar_pasajero', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_cargar_pasajero;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_CargarPasajero', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_cargar_pasajero', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_CargarPasajero;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_CargarPasajero', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_cargar_pasajero', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_CargarPasajero', N'he_sp_solicitud_cargar_pasajero', N'OBJECT';
GO

-- sp_Solicitud_DesasignarHabitacion -> he_sp_solicitud_desasignar_habitacion
IF OBJECT_ID(N'dbo.he_sp_solicitud_desasignar_habitacion', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_desasignar_habitacion;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_DesasignarHabitacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_desasignar_habitacion', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_DesasignarHabitacion;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_DesasignarHabitacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_desasignar_habitacion', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_DesasignarHabitacion', N'he_sp_solicitud_desasignar_habitacion', N'OBJECT';
GO

-- sp_Solicitud_EliminarPasajero -> he_sp_solicitud_eliminar_pasajero
IF OBJECT_ID(N'dbo.he_sp_solicitud_eliminar_pasajero', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_eliminar_pasajero;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_EliminarPasajero', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_eliminar_pasajero', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_EliminarPasajero;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_EliminarPasajero', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_eliminar_pasajero', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_EliminarPasajero', N'he_sp_solicitud_eliminar_pasajero', N'OBJECT';
GO

-- sp_Solicitud_GenerarReservas -> he_sp_solicitud_generar_reservas
IF OBJECT_ID(N'dbo.he_sp_solicitud_generar_reservas', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_generar_reservas;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_GenerarReservas', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_generar_reservas', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_GenerarReservas;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_GenerarReservas', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_generar_reservas', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_GenerarReservas', N'he_sp_solicitud_generar_reservas', N'OBJECT';
GO

-- sp_Solicitud_Guardar -> he_sp_solicitud_guardar
IF OBJECT_ID(N'dbo.he_sp_solicitud_guardar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_guardar;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_guardar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_Guardar;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_guardar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_Guardar', N'he_sp_solicitud_guardar', N'OBJECT';
GO

-- sp_Solicitud_GuardarPasajero -> he_sp_solicitud_guardar_pasajero
IF OBJECT_ID(N'dbo.he_sp_solicitud_guardar_pasajero', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_guardar_pasajero;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_GuardarPasajero', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_guardar_pasajero', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_GuardarPasajero;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_GuardarPasajero', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_guardar_pasajero', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_GuardarPasajero', N'he_sp_solicitud_guardar_pasajero', N'OBJECT';
GO

-- sp_Solicitud_HabitacionesDisponibles -> he_sp_solicitud_habitaciones_disponibles
IF OBJECT_ID(N'dbo.he_sp_solicitud_habitaciones_disponibles', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_habitaciones_disponibles;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_HabitacionesDisponibles', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_habitaciones_disponibles', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_HabitacionesDisponibles;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_HabitacionesDisponibles', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_habitaciones_disponibles', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_HabitacionesDisponibles', N'he_sp_solicitud_habitaciones_disponibles', N'OBJECT';
GO

-- sp_Solicitud_ListarCatalogo -> he_sp_solicitud_listar_catalogo
IF OBJECT_ID(N'dbo.he_sp_solicitud_listar_catalogo', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_listar_catalogo;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ListarCatalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_catalogo', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_ListarCatalogo;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ListarCatalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_catalogo', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_ListarCatalogo', N'he_sp_solicitud_listar_catalogo', N'OBJECT';
GO

-- sp_Solicitud_ListarOrganizacion -> he_sp_solicitud_listar_organizacion
IF OBJECT_ID(N'dbo.he_sp_solicitud_listar_organizacion', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_listar_organizacion;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ListarOrganizacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_organizacion', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_ListarOrganizacion;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ListarOrganizacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_organizacion', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_ListarOrganizacion', N'he_sp_solicitud_listar_organizacion', N'OBJECT';
GO

-- sp_Solicitud_ListarPasajeros -> he_sp_solicitud_listar_pasajeros
IF OBJECT_ID(N'dbo.he_sp_solicitud_listar_pasajeros', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_listar_pasajeros;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ListarPasajeros', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_pasajeros', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_ListarPasajeros;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ListarPasajeros', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_pasajeros', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_ListarPasajeros', N'he_sp_solicitud_listar_pasajeros', N'OBJECT';
GO

-- sp_Solicitud_ObtenerPrimeraReserva -> he_sp_solicitud_obtener_primera_reserva
IF OBJECT_ID(N'dbo.he_sp_solicitud_obtener_primera_reserva', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_solicitud_obtener_primera_reserva;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ObtenerPrimeraReserva', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_obtener_primera_reserva', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Solicitud_ObtenerPrimeraReserva;
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ObtenerPrimeraReserva', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_obtener_primera_reserva', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Solicitud_ObtenerPrimeraReserva', N'he_sp_solicitud_obtener_primera_reserva', N'OBJECT';
GO

-- sp_TipoCama_Guardar -> he_sp_tipo_cama_guardar
IF OBJECT_ID(N'dbo.he_sp_tipo_cama_guardar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_tipo_cama_guardar;
GO

IF OBJECT_ID(N'dbo.sp_TipoCama_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_tipo_cama_guardar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_TipoCama_Guardar;
GO

IF OBJECT_ID(N'dbo.sp_TipoCama_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_tipo_cama_guardar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_TipoCama_Guardar', N'he_sp_tipo_cama_guardar', N'OBJECT';
GO

-- sp_TipoCama_Listar -> he_sp_tipo_cama_listar
IF OBJECT_ID(N'dbo.he_sp_tipo_cama_listar', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_tipo_cama_listar;
GO

IF OBJECT_ID(N'dbo.sp_TipoCama_Listar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_tipo_cama_listar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_TipoCama_Listar;
GO

IF OBJECT_ID(N'dbo.sp_TipoCama_Listar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_tipo_cama_listar', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_TipoCama_Listar', N'he_sp_tipo_cama_listar', N'OBJECT';
GO

-- sp_Usuario_ActualizarPassword -> he_sp_usuario_actualizar_password
IF OBJECT_ID(N'dbo.he_sp_usuario_actualizar_password', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_usuario_actualizar_password;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ActualizarPassword', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_actualizar_password', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_ActualizarPassword;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ActualizarPassword', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_actualizar_password', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Usuario_ActualizarPassword', N'he_sp_usuario_actualizar_password', N'OBJECT';
GO

-- sp_Usuario_CambiarPasswordPorRut -> he_sp_usuario_cambiar_password_por_rut
IF OBJECT_ID(N'dbo.he_sp_usuario_cambiar_password_por_rut', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_usuario_cambiar_password_por_rut;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_CambiarPasswordPorRut', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_cambiar_password_por_rut', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_CambiarPasswordPorRut;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_CambiarPasswordPorRut', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_cambiar_password_por_rut', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Usuario_CambiarPasswordPorRut', N'he_sp_usuario_cambiar_password_por_rut', N'OBJECT';
GO

-- sp_Usuario_ObtenerCampamentosYPerfiles -> he_sp_usuario_obtener_campamentos_y_perfiles
IF OBJECT_ID(N'dbo.he_sp_usuario_obtener_campamentos_y_perfiles', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_usuario_obtener_campamentos_y_perfiles;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerCampamentosYPerfiles', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_campamentos_y_perfiles', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_ObtenerCampamentosYPerfiles;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerCampamentosYPerfiles', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_campamentos_y_perfiles', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Usuario_ObtenerCampamentosYPerfiles', N'he_sp_usuario_obtener_campamentos_y_perfiles', N'OBJECT';
GO

-- sp_Usuario_ObtenerHistorialContrasenas -> he_sp_usuario_obtener_historial_contrasenas
IF OBJECT_ID(N'dbo.he_sp_usuario_obtener_historial_contrasenas', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_usuario_obtener_historial_contrasenas;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerHistorialContrasenas', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_historial_contrasenas', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_ObtenerHistorialContrasenas;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerHistorialContrasenas', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_historial_contrasenas', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Usuario_ObtenerHistorialContrasenas', N'he_sp_usuario_obtener_historial_contrasenas', N'OBJECT';
GO

-- sp_Usuario_ObtenerParaLogin -> he_sp_usuario_obtener_para_login
IF OBJECT_ID(N'dbo.he_sp_usuario_obtener_para_login', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_usuario_obtener_para_login;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerParaLogin', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_para_login', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_ObtenerParaLogin;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerParaLogin', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_para_login', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Usuario_ObtenerParaLogin', N'he_sp_usuario_obtener_para_login', N'OBJECT';
GO

-- sp_Usuario_ReemplazarHash -> he_sp_usuario_reemplazar_hash
IF OBJECT_ID(N'dbo.he_sp_usuario_reemplazar_hash', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_usuario_reemplazar_hash;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ReemplazarHash', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reemplazar_hash', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_ReemplazarHash;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ReemplazarHash', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reemplazar_hash', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Usuario_ReemplazarHash', N'he_sp_usuario_reemplazar_hash', N'OBJECT';
GO

-- sp_Usuario_RegistrarIntentoFallido -> he_sp_usuario_registrar_intento_fallido
IF OBJECT_ID(N'dbo.he_sp_usuario_registrar_intento_fallido', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_usuario_registrar_intento_fallido;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_RegistrarIntentoFallido', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_registrar_intento_fallido', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_RegistrarIntentoFallido;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_RegistrarIntentoFallido', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_registrar_intento_fallido', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Usuario_RegistrarIntentoFallido', N'he_sp_usuario_registrar_intento_fallido', N'OBJECT';
GO

-- sp_Usuario_ReiniciarIntentos -> he_sp_usuario_reiniciar_intentos
IF OBJECT_ID(N'dbo.he_sp_usuario_reiniciar_intentos', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_usuario_reiniciar_intentos;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ReiniciarIntentos', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reiniciar_intentos', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_ReiniciarIntentos;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ReiniciarIntentos', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reiniciar_intentos', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Usuario_ReiniciarIntentos', N'he_sp_usuario_reiniciar_intentos', N'OBJECT';
GO

-- sp_Usuario_RetirarMd5 -> he_sp_usuario_retirar_md5
IF OBJECT_ID(N'dbo.he_sp_usuario_retirar_md5', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_usuario_retirar_md5;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_RetirarMd5', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_retirar_md5', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_RetirarMd5;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_RetirarMd5', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_retirar_md5', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Usuario_RetirarMd5', N'he_sp_usuario_retirar_md5', N'OBJECT';
GO

-- sp_Usuario_ValidarEstado -> he_sp_usuario_validar_estado
IF OBJECT_ID(N'dbo.he_sp_usuario_validar_estado', N'SN') IS NOT NULL
    DROP SYNONYM dbo.he_sp_usuario_validar_estado;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ValidarEstado', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_validar_estado', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_ValidarEstado;
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ValidarEstado', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_validar_estado', N'P') IS NULL
    EXEC sys.sp_rename N'dbo.sp_Usuario_ValidarEstado', N'he_sp_usuario_validar_estado', N'OBJECT';
GO
