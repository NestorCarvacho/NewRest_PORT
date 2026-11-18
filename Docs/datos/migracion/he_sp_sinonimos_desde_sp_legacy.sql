/* Puente QA: sinónimos he_sp_* → sp_* legacy (eliminar tras ejecutar scripts del repo). */
SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.sp_Bano_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_bano_guardar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_bano_guardar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_bano_guardar FOR dbo.sp_Bano_Guardar;');
GO

IF OBJECT_ID(N'dbo.sp_Bano_HabitacionesActivasPorBano', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_bano_habitaciones_activas_por_bano', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_bano_habitaciones_activas_por_bano', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_bano_habitaciones_activas_por_bano FOR dbo.sp_Bano_HabitacionesActivasPorBano;');
GO

IF OBJECT_ID(N'dbo.sp_Bano_Listar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_bano_listar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_bano_listar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_bano_listar FOR dbo.sp_Bano_Listar;');
GO

IF OBJECT_ID(N'dbo.sp_Cama_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_cama_guardar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_cama_guardar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_cama_guardar FOR dbo.sp_Cama_Guardar;');
GO

IF OBJECT_ID(N'dbo.sp_Cama_ListarPorHabitacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_cama_listar_por_habitacion', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_cama_listar_por_habitacion', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_cama_listar_por_habitacion FOR dbo.sp_Cama_ListarPorHabitacion;');
GO

IF OBJECT_ID(N'dbo.sp_Censo_Buscar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_buscar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_censo_buscar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_censo_buscar FOR dbo.sp_Censo_Buscar;');
GO

IF OBJECT_ID(N'dbo.sp_Censo_BuscarPorUsuario', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_buscar_por_usuario', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_censo_buscar_por_usuario', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_censo_buscar_por_usuario FOR dbo.sp_Censo_BuscarPorUsuario;');
GO

IF OBJECT_ID(N'dbo.sp_Censo_FiltroPertenece', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_filtro_pertenece', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_censo_filtro_pertenece', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_censo_filtro_pertenece FOR dbo.sp_Censo_FiltroPertenece;');
GO

IF OBJECT_ID(N'dbo.sp_Censo_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_guardar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_censo_guardar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_censo_guardar FOR dbo.sp_Censo_Guardar;');
GO

IF OBJECT_ID(N'dbo.sp_Censo_ListarCatalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_censo_listar_catalogo', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_censo_listar_catalogo', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_censo_listar_catalogo FOR dbo.sp_Censo_ListarCatalogo;');
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Catalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_catalogo', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_catalogo', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_habitacion_catalogo FOR dbo.sp_Habitacion_Catalogo;');
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_guardar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_guardar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_habitacion_guardar FOR dbo.sp_Habitacion_Guardar;');
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_IdPorNombre', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_id_por_nombre', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_id_por_nombre', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_habitacion_id_por_nombre FOR dbo.sp_Habitacion_IdPorNombre;');
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Listar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_listar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_listar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_habitacion_listar FOR dbo.sp_Habitacion_Listar;');
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_Obtener', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_obtener', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_obtener', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_habitacion_obtener FOR dbo.sp_Habitacion_Obtener;');
GO

IF OBJECT_ID(N'dbo.sp_Habitacion_SincronizarCamas', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_sincronizar_camas', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_habitacion_sincronizar_camas', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_habitacion_sincronizar_camas FOR dbo.sp_Habitacion_SincronizarCamas;');
GO

IF OBJECT_ID(N'dbo.sp_Menu_ObtenerPorPerfil', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_menu_obtener_por_perfil', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_menu_obtener_por_perfil', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_menu_obtener_por_perfil FOR dbo.sp_Menu_ObtenerPorPerfil;');
GO

IF OBJECT_ID(N'dbo.sp_Reserva_Actualizar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_actualizar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_reserva_actualizar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_reserva_actualizar FOR dbo.sp_Reserva_Actualizar;');
GO

IF OBJECT_ID(N'dbo.sp_Reserva_Buscar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_buscar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_reserva_buscar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_reserva_buscar FOR dbo.sp_Reserva_Buscar;');
GO

IF OBJECT_ID(N'dbo.sp_Reserva_Crear', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_crear', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_reserva_crear', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_reserva_crear FOR dbo.sp_Reserva_Crear;');
GO

IF OBJECT_ID(N'dbo.sp_Reserva_EstaEnCampamento', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_esta_en_campamento', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_reserva_esta_en_campamento', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_reserva_esta_en_campamento FOR dbo.sp_Reserva_EstaEnCampamento;');
GO

IF OBJECT_ID(N'dbo.sp_Reserva_FinalizarCheckout', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_finalizar_checkout', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_reserva_finalizar_checkout', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_reserva_finalizar_checkout FOR dbo.sp_Reserva_FinalizarCheckout;');
GO

IF OBJECT_ID(N'dbo.sp_Reserva_ListarCatalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_listar_catalogo', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_reserva_listar_catalogo', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_reserva_listar_catalogo FOR dbo.sp_Reserva_ListarCatalogo;');
GO

IF OBJECT_ID(N'dbo.sp_Reserva_RegistrarMovimiento', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_reserva_registrar_movimiento', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_reserva_registrar_movimiento', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_reserva_registrar_movimiento FOR dbo.sp_Reserva_RegistrarMovimiento;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_AgregarCupos', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_agregar_cupos', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_agregar_cupos', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_agregar_cupos FOR dbo.sp_Solicitud_AgregarCupos;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_Anular', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_anular', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_anular', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_anular FOR dbo.sp_Solicitud_Anular;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_AsegurarPeriodo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_asegurar_periodo', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_asegurar_periodo', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_asegurar_periodo FOR dbo.sp_Solicitud_AsegurarPeriodo;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_AsignarHabitacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_asignar_habitacion', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_asignar_habitacion', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_asignar_habitacion FOR dbo.sp_Solicitud_AsignarHabitacion;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_Buscar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_buscar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_buscar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_buscar FOR dbo.sp_Solicitud_Buscar;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_CargarPasajero', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_cargar_pasajero', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_cargar_pasajero', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_cargar_pasajero FOR dbo.sp_Solicitud_CargarPasajero;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_DesasignarHabitacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_desasignar_habitacion', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_desasignar_habitacion', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_desasignar_habitacion FOR dbo.sp_Solicitud_DesasignarHabitacion;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_EliminarPasajero', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_eliminar_pasajero', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_eliminar_pasajero', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_eliminar_pasajero FOR dbo.sp_Solicitud_EliminarPasajero;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_GenerarReservas', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_generar_reservas', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_generar_reservas', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_generar_reservas FOR dbo.sp_Solicitud_GenerarReservas;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_guardar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_guardar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_guardar FOR dbo.sp_Solicitud_Guardar;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_GuardarPasajero', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_guardar_pasajero', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_guardar_pasajero', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_guardar_pasajero FOR dbo.sp_Solicitud_GuardarPasajero;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_HabitacionesDisponibles', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_habitaciones_disponibles', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_habitaciones_disponibles', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_habitaciones_disponibles FOR dbo.sp_Solicitud_HabitacionesDisponibles;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ListarCatalogo', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_catalogo', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_catalogo', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_listar_catalogo FOR dbo.sp_Solicitud_ListarCatalogo;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ListarOrganizacion', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_organizacion', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_organizacion', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_listar_organizacion FOR dbo.sp_Solicitud_ListarOrganizacion;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ListarPasajeros', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_pasajeros', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_listar_pasajeros', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_listar_pasajeros FOR dbo.sp_Solicitud_ListarPasajeros;');
GO

IF OBJECT_ID(N'dbo.sp_Solicitud_ObtenerPrimeraReserva', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_obtener_primera_reserva', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_solicitud_obtener_primera_reserva', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_solicitud_obtener_primera_reserva FOR dbo.sp_Solicitud_ObtenerPrimeraReserva;');
GO

IF OBJECT_ID(N'dbo.sp_TipoCama_Guardar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_tipo_cama_guardar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_tipo_cama_guardar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_tipo_cama_guardar FOR dbo.sp_TipoCama_Guardar;');
GO

IF OBJECT_ID(N'dbo.sp_TipoCama_Listar', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_tipo_cama_listar', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_tipo_cama_listar', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_tipo_cama_listar FOR dbo.sp_TipoCama_Listar;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ActualizarPassword', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_actualizar_password', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_actualizar_password', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_actualizar_password FOR dbo.sp_Usuario_ActualizarPassword;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_CambiarPasswordPorRut', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_cambiar_password_por_rut', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_cambiar_password_por_rut', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_cambiar_password_por_rut FOR dbo.sp_Usuario_CambiarPasswordPorRut;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerCampamentosYPerfiles', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_campamentos_y_perfiles', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_campamentos_y_perfiles', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_obtener_campamentos_y_perfiles FOR dbo.sp_Usuario_ObtenerCampamentosYPerfiles;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerHistorialContrasenas', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_historial_contrasenas', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_historial_contrasenas', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_obtener_historial_contrasenas FOR dbo.sp_Usuario_ObtenerHistorialContrasenas;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerParaLogin', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_para_login', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_para_login', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_obtener_para_login FOR dbo.sp_Usuario_ObtenerParaLogin;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ReemplazarHash', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reemplazar_hash', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reemplazar_hash', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_reemplazar_hash FOR dbo.sp_Usuario_ReemplazarHash;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_RegistrarIntentoFallido', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_registrar_intento_fallido', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_registrar_intento_fallido', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_registrar_intento_fallido FOR dbo.sp_Usuario_RegistrarIntentoFallido;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ReiniciarIntentos', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reiniciar_intentos', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reiniciar_intentos', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_reiniciar_intentos FOR dbo.sp_Usuario_ReiniciarIntentos;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_RetirarMd5', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_retirar_md5', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_retirar_md5', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_retirar_md5 FOR dbo.sp_Usuario_RetirarMd5;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ValidarEstado', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_validar_estado', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_validar_estado', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_validar_estado FOR dbo.sp_Usuario_ValidarEstado;');
GO
