/*
  Asigna Configuración → Tipos de cama a uno o más perfiles (MenusPerfil).
  Requiere 004_menu_config_tipos_cama.sql (y conviene 002/003 si el perfil también usa habitaciones/baños).
*/
SET NOCOUNT ON;

DECLARE @PerfilId int = 68;
DECLARE @Ahora datetime2 = SYSUTCDATETIME();
DECLARE @Usuario int = 1;

DECLARE @MenuTipo int = (SELECT TOP 1 Id FROM dbo.Menus WITH (NOLOCK) WHERE Link = N'/app/config/tiposcama' AND IsActive = 1);

IF @MenuTipo IS NULL
BEGIN
    RAISERROR(N'Falta menú Tipos de cama. Ejecute primero 004_menu_config_tipos_cama.sql.', 16, 1);
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
          AND h.Link = N'/app/config/tiposcama'
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

IF NOT EXISTS (SELECT 1 FROM dbo.MenusPerfil WHERE PerfilId = @PerfilId AND MenuId = @MenuTipo)
    INSERT INTO dbo.MenusPerfil (PerfilId, MenuId, FechaCreacion, UsuarioCreacionId)
    VALUES (@PerfilId, @MenuTipo, @Ahora, @Usuario);

SELECT @PerfilId AS PerfilId, @PadreConfig AS PadreConfigId, @MenuTipo AS MenuTiposCamaId;
GO