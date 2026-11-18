/*
  Asigna Configuración → Habitaciones y Baños a uno o más perfiles (MenusPerfil).
  Requiere ítems creados por 002_menu_config_habitacion_bano.sql.
  Ejemplo: perfil operativo 68.
*/
SET NOCOUNT ON;

DECLARE @PerfilId int = 68;
DECLARE @Ahora datetime2 = SYSUTCDATETIME();
DECLARE @Usuario int = 1;

DECLARE @MenuHab int = (SELECT TOP 1 Id FROM dbo.Menus WITH (NOLOCK) WHERE Link = N'/app/config/habitaciones' AND IsActive = 1);
DECLARE @MenuBano int = (SELECT TOP 1 Id FROM dbo.Menus WITH (NOLOCK) WHERE Link = N'/app/config/banos' AND IsActive = 1);

IF @MenuHab IS NULL OR @MenuBano IS NULL
BEGIN
    RAISERROR(N'Faltan menús Habitaciones/Baños. Ejecute primero 002_menu_config_habitacion_bano.sql.', 16, 1);
    RETURN;
END

/* Raíz nav de Configuración (padre de los ítems side) */
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