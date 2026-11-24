/*
  Ítem de menú Configuración → Tipos de cama (CU-HAB-02).
  Ejecutar en HOTELDB después de 002_menu_config_habitacion_bano.sql (mismo padre Config).
*/

DECLARE @PadreConfig int;
SELECT TOP (1) @PadreConfig = m.Id
FROM dbo.Menus AS m WITH (NOLOCK)
WHERE m.IsActive = 1
  AND m.Ubicacion = N'nav'
  AND (m.Link LIKE N'%/config%' OR m.Nombre LIKE N'%Configur%')
ORDER BY m.Id;

IF @PadreConfig IS NULL
    SET @PadreConfig = 4; /* convención legado Perfil 1 */

DECLARE @Ahora datetime2 = SYSUTCDATETIME();
DECLARE @Usuario int = 1;

IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE Link = N'/app/config/tiposcama' AND IsActive = 1)
BEGIN
    DECLARE @IdTipo int = (SELECT ISNULL(MAX(Id), 0) + 1 FROM dbo.Menus);
    INSERT INTO dbo.Menus (Id, Nombre, Link, PadreId, Ubicacion, Orden, Icono, IsActive, FechaCreacion, UsuarioCreacionId)
    VALUES (@IdTipo, N'Tipos de cama', N'/app/config/tiposcama', @PadreConfig, N'side', 9, NULL, 1, @Ahora, @Usuario);
END

DECLARE @MenuTipo int = (SELECT TOP 1 Id FROM dbo.Menus WHERE Link = N'/app/config/tiposcama' AND IsActive = 1);

IF @MenuTipo IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.MenusPerfil WHERE PerfilId = 1 AND MenuId = @MenuTipo)
    INSERT INTO dbo.MenusPerfil (PerfilId, MenuId, FechaCreacion, UsuarioCreacionId)
    VALUES (1, @MenuTipo, @Ahora, @Usuario);
GO