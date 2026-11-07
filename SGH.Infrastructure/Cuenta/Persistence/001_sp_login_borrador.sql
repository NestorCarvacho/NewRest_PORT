/*
    Borrador de procedimientos del login.
    No ejecutar el bloque de clave hasta contrastarlo con HOTELDB.
    El contrato de columnas y parámetros es el que lee SGH.Infrastructure.
    Si un nombre físico cambia, se ajusta este script y se mantienen los alias.

    Checklist y hipótesis: Docs/login-pendiente-revision-base-de-datos.md

    Columna nueva, nombre de tabla a confirmar:
    ALTER TABLE dbo.Usuarios ADD Md5Retirado bit NOT NULL
        CONSTRAINT DF_Usuarios_Md5Retirado DEFAULT (0);
    0 = la clave todavía puede ser MD5. 1 = esa fila ya no usa MD5.
*/

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
   EmpresaMandanteId, EmpresaMandanteNombre, CampamentoPerfil. */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ObtenerCampamentosYPerfiles
    @Rut varchar(20)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.USU_ObtieneCampamentosYPerfilesUsuario @UsuarioRut = @Rut;
END
GO

/* BORRADOR. Hoy falla a propósito: la columna del hash no está en el repositorio.
   Al habilitarlo, el SELECT tiene que exponer estos alias:
   Id, Rut, Nombre, Apellidos, HashContrasena, PerfilId, EmpresaMandanteId, Md5Retirado.
   Md5Retirado es bit: 0 sigue en MD5, 1 ya no se usa MD5 para esa fila.
   @LoginInterno llega desde la web en 0. Confirmar si USU_ObtieneUsuarioAuth
   cambia de fila cuando ese valor no es 0. USU_ObtieneUsuarioByRutAndPass no lo recibe. */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ObtenerParaLogin
    @Rut varchar(20),
    @LoginInterno int = 0
AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR('BORRADOR: sp_Usuario_ObtenerParaLogin sin SELECT hasta contrastar la columna de clave. Ver Docs/login-pendiente-revision-base-de-datos.md', 16, 1);
    RETURN;

    /*
    SELECT TOP (1)
        u.Id,
        u.Rut,
        u.Nombre,
        u.Apellidos,
        u.Password AS HashContrasena,
        u.PerfilId,
        u.EmpresaMandanteId,
        ISNULL(u.Md5Retirado, 0) AS Md5Retirado,
        CASE
            WHEN u.BloqueadaHasta IS NOT NULL AND u.BloqueadaHasta > GETDATE() THEN 1
            ELSE 0
        END AS Bloqueada
    FROM dbo.Usuarios AS u
    WHERE u.Rut = @Rut;
    */
END
GO

/* BORRADOR. No llama a USU_CambiaPasswordUsuario: ese procedimiento recibe MD5
   y puede rechazar con -3 si el histórico guarda la huella vigente.
   Códigos: 0 actualizada, -3 reutilizada, cualquier otro se informa como credencial inválida.
   @PasswordHash es BCrypt (cerca de 60 caracteres). @HuellaMd5 es NULL cuando Md5Retirado ya es 1.
   Si viene informada, solo sirve para comparar histórico legado. El procedimiento deja Md5Retirado = 1. */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ActualizarPassword
    @Rut varchar(20),
    @PasswordHash varchar(200),
    @HuellaMd5 varchar(32) = NULL,
    @Ret int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Ret = -1;
    RAISERROR('BORRADOR: sp_Usuario_ActualizarPassword no persiste hasta contrastar histórico y largo de columna. Ver Docs/login-pendiente-revision-base-de-datos.md', 16, 1);
END
GO

/* BORRADOR. Reemplaza MD5 por BCrypt de la misma clave y deja Md5Retirado = 1.
   No pasa por el control de reutilización. */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ReemplazarHash
    @Rut varchar(20),
    @PasswordHash varchar(200),
    @Ret int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Ret = -1;
    RAISERROR('BORRADOR: sp_Usuario_ReemplazarHash no persiste hasta contrastar la columna de clave. Ver Docs/login-pendiente-revision-base-de-datos.md', 16, 1);
END
GO

/* BORRADOR. La clave ya es BCrypt y solo falta apagar MD5: Md5Retirado = 1, sin reescribir el hash. */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_RetirarMd5
    @Rut varchar(20),
    @Ret int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Ret = -1;
    RAISERROR('BORRADOR: sp_Usuario_RetirarMd5 no persiste hasta crear la columna Md5Retirado. Ver Docs/login-pendiente-revision-base-de-datos.md', 16, 1);
END
GO

/* Bloqueo temporal. Los números 5 y 30 son ReglasCuenta.IntentosAntesDeBloquear
   y ReglasCuenta.MinutosBloqueo. Crear antes estas columnas en la tabla de usuarios
   (el nombre dbo.Usuarios es hipótesis, igual que Password y Md5Retirado):

   ALTER TABLE dbo.Usuarios ADD
       IntentosFallidos int NOT NULL
           CONSTRAINT DF_Usuarios_IntentosFallidos DEFAULT (0),
       BloqueadaHasta datetime NULL;
*/
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_RegistrarIntentoFallido
    @Rut varchar(20),
    @Intentos int OUTPUT,
    @Bloqueada bit OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Usuarios
    SET IntentosFallidos = 0,
        BloqueadaHasta = NULL
    WHERE Rut = @Rut
      AND BloqueadaHasta IS NOT NULL
      AND BloqueadaHasta <= GETDATE();

    UPDATE dbo.Usuarios
    SET IntentosFallidos = IntentosFallidos + 1,
        BloqueadaHasta = CASE
            WHEN IntentosFallidos + 1 >= 5 THEN DATEADD(minute, 30, GETDATE())
            ELSE NULL
        END
    WHERE Rut = @Rut
      AND (BloqueadaHasta IS NULL OR BloqueadaHasta <= GETDATE());

    SELECT
        @Intentos = ISNULL(IntentosFallidos, 0),
        @Bloqueada = CASE
            WHEN BloqueadaHasta IS NOT NULL AND BloqueadaHasta > GETDATE() THEN 1
            ELSE 0
        END
    FROM dbo.Usuarios
    WHERE Rut = @Rut;
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
