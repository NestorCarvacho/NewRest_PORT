/*
    Login de SGH para el esquema HOTELDB contrastado en LocalDB.
    Ejecutar de una sola vez: primero ALTER TABLE, luego procedimientos he_sp_usuario_*.

    Password y el historial se amplían a nvarchar(100), suficiente para BCrypt.
    La aplicación compara los hashes del historial porque SQL Server no puede
    verificar BCrypt; el límite histórico se toma de Listas.usrpasshist.

    Incluye PasswordV2 y lógica dual MD5/BCrypt (antes en 002_sp_login_passwordv2_dual.sql).
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ==================================================================================
   SECCIÓN 1 — ALTER TABLE (columnas login, BCrypt, bloqueo)
   ================================================================================== */

IF COL_LENGTH('dbo.Usuarios', 'Password') IS NULL
    THROW 51000, 'No existe dbo.Usuarios.Password.', 1;
IF COL_LENGTH('dbo.Usuarios', 'Password') < 200
    ALTER TABLE dbo.Usuarios ALTER COLUMN Password nvarchar(100) NULL;
GO

IF COL_LENGTH('dbo.HistoricoPassword', 'Password') IS NULL
    THROW 51001, 'No existe dbo.HistoricoPassword.Password.', 1;
IF COL_LENGTH('dbo.HistoricoPassword', 'Password') < 200
    ALTER TABLE dbo.HistoricoPassword ALTER COLUMN Password nvarchar(100) NOT NULL;
GO

IF COL_LENGTH('dbo.Usuarios', 'Md5Retirado') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD Md5Retirado bit NOT NULL
        CONSTRAINT DF_Usuarios_Md5Retirado DEFAULT (0) WITH VALUES;
END
GO

IF COL_LENGTH('dbo.Usuarios', 'PasswordV2') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD PasswordV2 nvarchar(100) NULL;
END
GO

IF COL_LENGTH('dbo.Usuarios', 'IntentosFallidos') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD IntentosFallidos int NOT NULL
        CONSTRAINT DF_Usuarios_IntentosFallidos DEFAULT (0) WITH VALUES;
END
GO

IF COL_LENGTH('dbo.Usuarios', 'BloqueadaHasta') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD BloqueadaHasta datetime NULL;
END
GO

/* ==================================================================================
   SECCIÓN 2 — PROCEDIMIENTOS dbo.he_sp_usuario_*
   ================================================================================== */

CREATE OR ALTER PROCEDURE dbo.he_sp_usuario_validar_estado
    @Rut varchar(20)
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Envoltorio de USU_ValidaEstadoUsuario; devuelve el código de estado del RUT.
    --======================================================

    SET NOCOUNT ON;

    DECLARE @ret int;
    EXEC dbo.USU_ValidaEstadoUsuario @rut = @Rut, @ret = @ret OUTPUT;
    SELECT @ret AS Estado;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_usuario_obtener_campamentos_y_perfiles
    @Rut varchar(20)
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Envoltorio de USU_ObtieneCampamentosYPerfilesUsuario; perfiles y campamentos del login.
    --======================================================

    SET NOCOUNT ON;
    EXEC dbo.USU_ObtieneCampamentosYPerfilesUsuario @UsuarioRut = @Rut;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_usuario_obtener_para_login
    @Rut varchar(20),
    @LoginInterno int = 0
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Fila activa del usuario con hashes MD5/BCrypt y bloqueo para verificar credenciales.
    --======================================================

    SET NOCOUNT ON;
    SELECT TOP (1)
        u.Id,
        u.Rut,
        u.Nombre,
        u.Apellidos,
        u.Password AS HashContrasena,
        u.PasswordV2 AS HashContrasenaV2,
        u.PerfilId,
        u.EmpresaMandanteId,
        u.Md5Retirado,
        CASE
            WHEN u.BloqueadaHasta IS NOT NULL AND u.BloqueadaHasta > GETDATE() THEN 1
            ELSE 0
        END AS Bloqueada
    FROM dbo.Usuarios AS u WITH (NOLOCK)
    WHERE u.Rut = @Rut
      AND u.IsActive = 1
      AND (
            NULLIF(LTRIM(RTRIM(u.Password)), N'') IS NOT NULL
            OR NULLIF(LTRIM(RTRIM(u.PasswordV2)), N'') IS NOT NULL
          )
    ORDER BY u.FechaCreacion DESC, u.Id DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_usuario_actualizar_password
    @Rut varchar(20),
    @PasswordHash varchar(200),
    @HuellaMd5 varchar(32) = NULL,
    @Ret int OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Graba BCrypt en PasswordV2, historial y valida huella MD5 según Listas.usrpasshist.
    --======================================================

    SET NOCOUNT ON;
    SET @Ret = -1;
    SET XACT_ABORT ON;

    IF @PasswordHash IS NULL OR LEN(@PasswordHash) <> 60 OR @PasswordHash NOT LIKE '$2%'
        RETURN;

    IF @HuellaMd5 IS NOT NULL
       AND (LEN(@HuellaMd5) <> 32 OR @HuellaMd5 LIKE '%[^0-9A-Fa-f]%')
        RETURN;

    DECLARE @CantidadHistorico int;
    SELECT @CantidadHistorico = TRY_CONVERT(int, Descripcion)
    FROM dbo.Listas
    WHERE IdTabla = 34 AND Valor1 = 'usrpasshist';

    SET @CantidadHistorico = ISNULL(@CantidadHistorico, 0);
    IF @CantidadHistorico < 0
        SET @CantidadHistorico = 0;

    IF @HuellaMd5 IS NOT NULL AND EXISTS
    (
        SELECT 1
        FROM
        (
            SELECT TOP (@CantidadHistorico) Password
            FROM dbo.HistoricoPassword
            WHERE Rut = @Rut
            ORDER BY FechaCreacion DESC, Id DESC
        ) AS h
        WHERE h.Password = @HuellaMd5
    )
    BEGIN
        SET @Ret = -3;
        RETURN;
    END

    DECLARE @UsuarioCreacion int;
    SELECT TOP (1) @UsuarioCreacion = Id
    FROM dbo.Usuarios
    WHERE Rut = @Rut
    ORDER BY CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, FechaCreacion DESC, Id DESC;

    IF @UsuarioCreacion IS NULL
        RETURN;

    BEGIN TRANSACTION;

    UPDATE dbo.Usuarios
    SET PasswordV2 = @PasswordHash,
        Md5Retirado = 1,
        IntentosFallidos = 0,
        BloqueadaHasta = NULL
    WHERE Rut = @Rut;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        RETURN;
    END

    INSERT INTO dbo.HistoricoPassword (Rut, Password, FechaCreacion, UsuarioCreacion)
    VALUES (@Rut, @PasswordHash, SYSDATETIME(), @UsuarioCreacion);

    COMMIT TRANSACTION;
    SET @Ret = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_usuario_cambiar_password_por_rut
    @Rut varchar(25),
    @PasswordHash varchar(200),
    @Ret int OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Cambio administrativo por RUT; solo hash BCrypt generado en la aplicación.
    --======================================================

    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @Ret = -1;

    IF NULLIF(LTRIM(RTRIM(@Rut)), '') IS NULL
       OR @PasswordHash IS NULL
       OR LEN(@PasswordHash) <> 60
       OR @PasswordHash NOT LIKE '$2%'
        RETURN;

    DECLARE @UsuarioId int;
    SELECT TOP (1) @UsuarioId = Id
    FROM dbo.Usuarios
    WHERE Rut = @Rut
      AND IsActive = 1
    ORDER BY FechaCreacion DESC, Id DESC;

    IF @UsuarioId IS NULL
        RETURN;

    BEGIN TRANSACTION;

    UPDATE dbo.Usuarios
    SET PasswordV2 = @PasswordHash,
        Md5Retirado = 1,
        IntentosFallidos = 0,
        BloqueadaHasta = NULL
    WHERE Id = @UsuarioId;

    IF @@ROWCOUNT <> 1
    BEGIN
        ROLLBACK TRANSACTION;
        RETURN;
    END

    INSERT INTO dbo.HistoricoPassword
        (Rut, Password, FechaCreacion, UsuarioCreacion)
    VALUES
        (@Rut, @PasswordHash, SYSDATETIME(), @UsuarioId);

    COMMIT TRANSACTION;
    SET @Ret = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_usuario_reemplazar_hash
    @Rut varchar(20),
    @PasswordHash varchar(200),
    @Ret int OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Migra huella MD5 vigente en Password a PasswordV2 cuando V2 está vacío.
    --======================================================

    SET NOCOUNT ON;
    SET @Ret = -1;
    IF @PasswordHash IS NULL OR LEN(@PasswordHash) <> 60 OR @PasswordHash NOT LIKE '$2%'
        RETURN;

    UPDATE dbo.Usuarios
    SET PasswordV2 = @PasswordHash,
        Md5Retirado = 1
    WHERE Rut = @Rut
      AND NULLIF(LTRIM(RTRIM(PasswordV2)), N'') IS NULL
      AND LEN(Password) = 32
      AND Password NOT LIKE '%[^0-9A-Fa-f]%';

    IF @@ROWCOUNT > 0
        SET @Ret = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_usuario_retirar_md5
    @Rut varchar(20),
    @Ret int OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Marca Md5Retirado; copia BCrypt desde Password a PasswordV2 sin alterar Password legado.
    --======================================================

    SET NOCOUNT ON;
    SET @Ret = -1;
    UPDATE dbo.Usuarios
    SET Md5Retirado = 1,
        PasswordV2 = CASE
            WHEN NULLIF(LTRIM(RTRIM(PasswordV2)), N'') IS NULL
                 AND Password LIKE '$2%'
                 AND LEN(Password) >= 50 THEN Password
            ELSE PasswordV2
        END
    WHERE Rut = @Rut
      AND (
            NULLIF(LTRIM(RTRIM(PasswordV2)), N'') IS NOT NULL
            OR (Password LIKE '$2%' AND LEN(Password) >= 50)
          );

    IF @@ROWCOUNT > 0
        SET @Ret = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_usuario_registrar_intento_fallido
    @Rut varchar(20),
    @Intentos int OUTPUT,
    @Bloqueada bit OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Registra intento fallido y bloqueo temporal según ReglasCuenta (umbrales en aplicación).
    --======================================================

    SET NOCOUNT ON;

    DECLARE @Ahora datetime = GETDATE();
    SET @Intentos = 0;
    SET @Bloqueada = 0;

    UPDATE dbo.Usuarios
    SET IntentosFallidos = 0,
        BloqueadaHasta = NULL
    WHERE Rut = @Rut
      AND BloqueadaHasta IS NOT NULL
      AND BloqueadaHasta <= @Ahora;

    UPDATE dbo.Usuarios
    SET IntentosFallidos = IntentosFallidos + 1,
        BloqueadaHasta = CASE
            WHEN IntentosFallidos + 1 >= 5 THEN DATEADD(minute, 10, @Ahora)
            ELSE NULL
        END
    WHERE Rut = @Rut
      AND (BloqueadaHasta IS NULL OR BloqueadaHasta <= @Ahora);

    SELECT
        @Intentos = ISNULL(MAX(IntentosFallidos), 0),
        @Bloqueada = CONVERT(bit, ISNULL(MAX(CASE
            WHEN BloqueadaHasta IS NOT NULL AND BloqueadaHasta > @Ahora THEN 1
            ELSE 0
        END), 0))
    FROM dbo.Usuarios
    WHERE Rut = @Rut;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_usuario_obtener_historial_contrasenas
    @Rut varchar(20)
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Devuelve las últimas contraseñas del historial según usrpasshist en Listas.
    --======================================================

    SET NOCOUNT ON;

    DECLARE @Cantidad int;
    SELECT @Cantidad = TRY_CONVERT(int, Descripcion)
    FROM dbo.Listas
    WHERE IdTabla = 34 AND Valor1 = 'usrpasshist';

    SET @Cantidad = ISNULL(@Cantidad, 0);
    IF @Cantidad < 0
        SET @Cantidad = 0;

    SELECT TOP (@Cantidad) Password
    FROM dbo.HistoricoPassword
    WHERE Rut = @Rut
    ORDER BY FechaCreacion DESC, Id DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_usuario_reiniciar_intentos
    @Rut varchar(20)
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Limpia intentos fallidos y bloqueo tras autenticación exitosa.
    --======================================================

    SET NOCOUNT ON;

    UPDATE dbo.Usuarios
    SET IntentosFallidos = 0,
        BloqueadaHasta = NULL
    WHERE Rut = @Rut;
END
GO