/*
    Incremental QA — ejecutar después de 001_sp_login.sql si ese archivo ya se aplicó
    sin columna PasswordV2 ni los procedimientos dual MD5/BCrypt (commit PasswordV2 dual).

    Instalación en frío: basta con 001_sp_login.sql (ya incluye este contenido).
    No volver a ejecutar 001 solo para estos cambios en QA.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('dbo.Usuarios', 'PasswordV2') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD PasswordV2 nvarchar(100) NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ObtenerParaLogin
    @Rut varchar(20),
    @LoginInterno int = 0
AS
BEGIN
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

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ActualizarPassword
    @Rut varchar(20),
    @PasswordHash varchar(200),
    @HuellaMd5 varchar(32) = NULL,
    @Ret int OUTPUT
AS
BEGIN
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

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_CambiarPasswordPorRut
    @Rut varchar(25),
    @PasswordHash varchar(200),
    @Ret int OUTPUT
AS
BEGIN
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

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ReemplazarHash
    @Rut varchar(20),
    @PasswordHash varchar(200),
    @Ret int OUTPUT
AS
BEGIN
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

/* Marca migración dual: copia BCrypt erróneo en Password a PasswordV2 sin tocar Password legado. */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_RetirarMd5
    @Rut varchar(20),
    @Ret int OUTPUT
AS
BEGIN
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