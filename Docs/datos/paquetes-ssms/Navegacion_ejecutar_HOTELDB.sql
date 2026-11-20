SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_menu_obtener_por_perfil
    @PerfilId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Lista ítems de menú activos asignados al perfil (Menus + MenusPerfil).
    --======================================================

    SET NOCOUNT ON;

    SELECT
        m.Id,
        m.Nombre,
        m.Link,
        m.PadreId,
        m.Ubicacion,
        m.Orden,
        m.Icono
    FROM dbo.Menus m
    INNER JOIN dbo.MenusPerfil mp ON mp.MenuId = m.Id
    WHERE mp.PerfilId = @PerfilId
      AND m.IsActive = 1
    ORDER BY m.Ubicacion, m.Orden, m.Nombre;
END
GO

DECLARE @PadreConfig int;
SELECT TOP (1) @PadreConfig = m.Id
FROM dbo.Menus AS m WITH (NOLOCK)
WHERE m.IsActive = 1
  AND m.Ubicacion = N'nav'
  AND (m.Link LIKE N'%/config%' OR m.Nombre LIKE N'%Configur%')
ORDER BY m.Id;

IF @PadreConfig IS NULL
    SET @PadreConfig = 4;

DECLARE @Ahora datetime2 = SYSUTCDATETIME();
DECLARE @Usuario int = 1;

IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE Link = N'/app/config/habitaciones' AND IsActive = 1)
BEGIN
    DECLARE @IdHab int = (SELECT ISNULL(MAX(Id), 0) + 1 FROM dbo.Menus);
    INSERT INTO dbo.Menus (Id, Nombre, Link, PadreId, Ubicacion, Orden, Icono, IsActive, FechaCreacion, UsuarioCreacionId)
    VALUES (@IdHab, N'Habitaciones', N'/app/config/habitaciones', @PadreConfig, N'side', 7, NULL, 1, @Ahora, @Usuario);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE Link = N'/app/config/banos' AND IsActive = 1)
BEGIN
    DECLARE @IdBano int = (SELECT ISNULL(MAX(Id), 0) + 1 FROM dbo.Menus);
    INSERT INTO dbo.Menus (Id, Nombre, Link, PadreId, Ubicacion, Orden, Icono, IsActive, FechaCreacion, UsuarioCreacionId)
    VALUES (@IdBano, N'Baños', N'/app/config/banos', @PadreConfig, N'side', 8, NULL, 1, @Ahora, @Usuario);
END

DECLARE @MenuHab int = (SELECT TOP 1 Id FROM dbo.Menus WHERE Link = N'/app/config/habitaciones' AND IsActive = 1);
DECLARE @MenuBano int = (SELECT TOP 1 Id FROM dbo.Menus WHERE Link = N'/app/config/banos' AND IsActive = 1);

IF @MenuHab IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.MenusPerfil WHERE PerfilId = 1 AND MenuId = @MenuHab)
    INSERT INTO dbo.MenusPerfil (PerfilId, MenuId, FechaCreacion, UsuarioCreacionId)
    VALUES (1, @MenuHab, @Ahora, @Usuario);

IF @MenuBano IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.MenusPerfil WHERE PerfilId = 1 AND MenuId = @MenuBano)
    INSERT INTO dbo.MenusPerfil (PerfilId, MenuId, FechaCreacion, UsuarioCreacionId)
    VALUES (1, @MenuBano, @Ahora, @Usuario);
GO

SET NOCOUNT ON;

DECLARE @PerfilId int = 68;
DECLARE @Ahora datetime2 = SYSUTCDATETIME();
DECLARE @Usuario int = 1;

DECLARE @MenuHab int = (SELECT TOP 1 Id FROM dbo.Menus WITH (NOLOCK) WHERE Link = N'/app/config/habitaciones' AND IsActive = 1);
DECLARE @MenuBano int = (SELECT TOP 1 Id FROM dbo.Menus WITH (NOLOCK) WHERE Link = N'/app/config/banos' AND IsActive = 1);

IF @MenuHab IS NULL OR @MenuBano IS NULL
BEGIN
    RAISERROR(N'Faltan menús Habitaciones/Baños. Ejecute el bloque anterior de este mismo script.', 16, 1);
    RETURN;
END

DECLARE @PadreConfig int;
SELECT TOP (1) @PadreConfig = p.Id
FROM dbo.Menus AS p WITH (NOLOCK)
WHERE p.IsActive = 1
  AND p.Ubicacion = N'nav'
  AND EXISTS (
        SELECT 1
        FROM dbo.Menus AS h WITH (NOLOCK)
        WHERE h.PadreId = p.Id
          AND h.IsActive = 1
          AND h.Link IN (N'/app/config/habitaciones', N'/app/config/banos')
      );

IF @PadreConfig IS NULL
    SELECT TOP (1) @PadreConfig = m.Id
    FROM dbo.Menus AS m WITH (NOLOCK)
    WHERE m.IsActive = 1
      AND m.Ubicacion = N'nav'
      AND (m.Link LIKE N'%/config%' OR m.Nombre LIKE N'%Configur%')
    ORDER BY m.Id;

IF @PadreConfig IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.MenusPerfil WHERE PerfilId = @PerfilId AND MenuId = @PadreConfig)
    INSERT INTO dbo.MenusPerfil (PerfilId, MenuId, FechaCreacion, UsuarioCreacionId)
    VALUES (@PerfilId, @PadreConfig, @Ahora, @Usuario);

IF NOT EXISTS (SELECT 1 FROM dbo.MenusPerfil WHERE PerfilId = @PerfilId AND MenuId = @MenuHab)
    INSERT INTO dbo.MenusPerfil (PerfilId, MenuId, FechaCreacion, UsuarioCreacionId)
    VALUES (@PerfilId, @MenuHab, @Ahora, @Usuario);

IF NOT EXISTS (SELECT 1 FROM dbo.MenusPerfil WHERE PerfilId = @PerfilId AND MenuId = @MenuBano)
    INSERT INTO dbo.MenusPerfil (PerfilId, MenuId, FechaCreacion, UsuarioCreacionId)
    VALUES (@PerfilId, @MenuBano, @Ahora, @Usuario);

SELECT @PerfilId AS PerfilId, @PadreConfig AS PadreConfigId, @MenuHab AS MenuHabitacionesId, @MenuBano AS MenuBanosId;
GO