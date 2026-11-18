/*
  Ítems de menú Configuración → Habitaciones y Baños (CU-HAB / SGH 1.5).
  Ejecutar en HOTELDB. Asignar MenusPerfil a los perfiles que deban verlos.
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

/* Ejemplo: perfil administración (Id = 1). Repetir para otros PerfilId según política. */
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