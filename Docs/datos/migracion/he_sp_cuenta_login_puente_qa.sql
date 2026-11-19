/* Puente mínimo LOGIN en QA — ejecutar en HOTELDB si la app ya pide he_sp_usuario_* y la base solo tiene sp_Usuario_*.
   Después conviene desplegar SGH.Infrastructure/Cuenta/Persistence/001_sp_login.sql (+ 002 si usan PasswordV2). */

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.sp_Usuario_ValidarEstado', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_validar_estado', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_validar_estado', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_validar_estado FOR dbo.sp_Usuario_ValidarEstado;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerCampamentosYPerfiles', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_campamentos_y_perfiles', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_campamentos_y_perfiles', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_obtener_campamentos_y_perfiles FOR dbo.sp_Usuario_ObtenerCampamentosYPerfiles;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerParaLogin', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_para_login', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_para_login', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_obtener_para_login FOR dbo.sp_Usuario_ObtenerParaLogin;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ActualizarPassword', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_actualizar_password', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_actualizar_password', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_actualizar_password FOR dbo.sp_Usuario_ActualizarPassword;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_CambiarPasswordPorRut', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_cambiar_password_por_rut', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_cambiar_password_por_rut', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_cambiar_password_por_rut FOR dbo.sp_Usuario_CambiarPasswordPorRut;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ReemplazarHash', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reemplazar_hash', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reemplazar_hash', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_reemplazar_hash FOR dbo.sp_Usuario_ReemplazarHash;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_RetirarMd5', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_retirar_md5', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_retirar_md5', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_retirar_md5 FOR dbo.sp_Usuario_RetirarMd5;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_RegistrarIntentoFallido', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_registrar_intento_fallido', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_registrar_intento_fallido', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_registrar_intento_fallido FOR dbo.sp_Usuario_RegistrarIntentoFallido;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ObtenerHistorialContrasenas', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_historial_contrasenas', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_obtener_historial_contrasenas', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_obtener_historial_contrasenas FOR dbo.sp_Usuario_ObtenerHistorialContrasenas;');
GO

IF OBJECT_ID(N'dbo.sp_Usuario_ReiniciarIntentos', N'P') IS NOT NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reiniciar_intentos', N'P') IS NULL AND OBJECT_ID(N'dbo.he_sp_usuario_reiniciar_intentos', N'SN') IS NULL
    EXEC(N'CREATE SYNONYM dbo.he_sp_usuario_reiniciar_intentos FOR dbo.sp_Usuario_ReiniciarIntentos;');
GO