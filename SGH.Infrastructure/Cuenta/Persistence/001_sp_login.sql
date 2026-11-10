/*
    Login de SGH para el esquema HOTELDB contrastado en LocalDB.
    Password y el historial se amplían a nvarchar(100), suficiente para BCrypt.
    La aplicación compara los hashes del historial porque SQL Server no puede
    verificar BCrypt; el límite histórico se toma de Listas.usrpasshist.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

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

/* Envoltorio del legado USU_ValidaEstadoUsuario (@rut, @ret OUTPUT). */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ValidarEstado
    @Rut varchar(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ret int;
    EXEC dbo.USU_ValidaEstadoUsuario @rut = @Rut, @ret = @ret OUTPUT;
    SELECT @ret AS Estado;
END
GO

/* Envoltorio del legado USU_ObtieneCampamentosYPerfilesUsuario (@UsuarioRut).
   Debe devolver, con estos nombres o equivalentes que Dapper ya mapea:
   Id, PerfilId, PerfilNombre, CampamentoId, CampamentoNombre,
   EmpresaMandanteId, EmpresaMandanteNombre, CampamentoPerfil, UsuarioId,
   UsuarioNombre. Los dos últimos identifican la cuenta asociada al perfil elegido. */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ObtenerCampamentosYPerfiles
    @Rut varchar(20)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.USU_ObtieneCampamentosYPerfilesUsuario @UsuarioRut = @Rut;
END
GO

/* Devuelve una fila activa para verificar la clave en la aplicación.
   En RUT duplicados la fila con la fecha de creación más reciente es estable;
   la selección final del usuario se toma de PerfilesUsuario en el segundo paso. */
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
        u.PerfilId,
        u.EmpresaMandanteId,
        u.Md5Retirado,
        CASE
            WHEN u.BloqueadaHasta IS NOT NULL AND u.BloqueadaHasta > GETDATE() THEN 1
            ELSE 0
        END AS Bloqueada
    FROM dbo.Usuarios AS u
    WHERE u.Rut = @Rut
      AND u.IsActive = 1
      AND NULLIF(LTRIM(RTRIM(u.Password)), N'') IS NOT NULL
    ORDER BY u.FechaCreacion DESC, u.Id DESC;
END
GO

/* Actualiza las filas del RUT como lo hacía el procedimiento legado y añade el
   nuevo BCrypt al historial. @HuellaMd5 conserva la comprobación histórica MD5. */
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
    SET Password = @PasswordHash,
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

/* Migra las huellas MD5 vigentes a BCrypt sin aplicar la regla de historial. */
/* Cambio administrativo por RUT. La aplicación debe generar el BCrypt antes
   de llamar al procedimiento; nunca se recibe una contraseña en claro. */
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
    SET Password = @PasswordHash,
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
    SET Password = @PasswordHash,
        Md5Retirado = 1
    WHERE Rut = @Rut
      AND LEN(Password) = 32
      AND Password NOT LIKE '%[^0-9A-Fa-f]%';

    IF @@ROWCOUNT > 0
        SET @Ret = 0;
END
GO

/* La clave ya es BCrypt y solo falta apagar MD5, sin reescribir el hash. */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_RetirarMd5
    @Rut varchar(20),
    @Ret int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Ret = -1;
    UPDATE dbo.Usuarios
    SET Md5Retirado = 1
    WHERE Rut = @Rut
      AND Password LIKE '$2%'
      AND LEN(Password) >= 50;

    IF @@ROWCOUNT > 0
        SET @Ret = 0;
END
GO

/* Los umbrales coinciden con ReglasCuenta: cinco intentos y treinta minutos. */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_RegistrarIntentoFallido
    @Rut varchar(20),
    @Intentos int OUTPUT,
    @Bloqueada bit OUTPUT
AS
BEGIN
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

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ObtenerHistorialContrasenas
    @Rut varchar(20)
AS
BEGIN
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

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ReiniciarIntentos
    @Rut varchar(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Usuarios
    SET IntentosFallidos = 0,
        BloqueadaHasta = NULL
    WHERE Rut = @Rut;
END
GO
