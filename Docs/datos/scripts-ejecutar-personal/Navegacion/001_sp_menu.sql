/* Menú por perfil — join Menus + MenusPerfil (HOTELDB legado). */
CREATE OR ALTER PROCEDURE dbo.sp_Menu_ObtenerPorPerfil
    @PerfilId int
AS
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