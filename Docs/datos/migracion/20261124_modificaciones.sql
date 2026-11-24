/*
    HOTELDB — modificaciones 2026-11-24
    Ejecutar en SSMS/sqlcmd contra la base Hoteldb (UTF-8).
    Consolida scripts del repositorio para CU-HAB-02 (tipos de cama), menú y catálogo habitación.

    Requisitos previos:
      - Tabla dbo.TipoCama y columnas en Camas: ejecutar si aún no aplica CU-HAB-02
        SGH.Infrastructure/Habitacion/Persistence/005_hotldb_tipo_cama_inventario.sql
      - Para menú habitaciones/baños: 002_menu_config_habitacion_bano.sql (opcional)

    Contenido (rutas en el repo):
      - SGH.Infrastructure/Habitacion/Persistence/006_sp_tipo_cama_inventario.sql
        (he_sp_tipo_cama_listar, he_sp_habitacion_catalogo octavo result set)
      - SGH.Infrastructure/Navegacion/Persistence/004_menu_config_tipos_cama.sql
      - SGH.Infrastructure/Navegacion/Persistence/005_menusperfil_tipos_cama_por_perfil.sql
    Código relacionado (sin SQL en este lote): página /Habitacion/TiposCama, fallback catálogo en app.
*/

-- ============================================================================
-- Habitación: he_sp_tipo_cama_listar (lista con CampamentoId para Dapper)
-- Origen: 006_sp_tipo_cama_inventario.sql
-- ============================================================================

CREATE OR ALTER PROCEDURE dbo.he_sp_tipo_cama_listar
    @CampamentoId int,
    @SoloActivos int = 1,
    @TipoCamaId int = 0
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-24
    --  description: Catálogo de tipos de cama por campamento.
    --======================================================
    SET NOCOUNT ON;

    SELECT
        t.Id,
        t.CampamentoId,
        t.Nombre,
        t.AnchoCm,
        t.LargoCm,
        t.IsActive,
        (SELECT COUNT(*) FROM dbo.Camas AS c WITH (NOLOCK)
         WHERE c.TipoCamaId = t.Id AND c.IsActive = 1) AS CamasVinculadas
    FROM dbo.TipoCama AS t WITH (NOLOCK)
    WHERE t.CampamentoId = @CampamentoId
      AND (@SoloActivos = 0 OR t.IsActive = 1)
      AND (@TipoCamaId = 0 OR t.Id = @TipoCamaId)
    ORDER BY t.Nombre;
END
GO

-- ============================================================================
-- Habitación: he_sp_habitacion_catalogo (8.º result set: tipos de cama activos)
-- Origen: 006_sp_tipo_cama_inventario.sql / 001_sp_habitacion.sql
-- ============================================================================

CREATE OR ALTER PROCEDURE dbo.he_sp_habitacion_catalogo
    @CampamentoId int,
    @MineraId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-24
    --  description: Catálogos escritorio habitaciones (incluye tipos de cama).
    --======================================================
    SET NOCOUNT ON;

    SELECT p.Id, p.Nombre AS Etiqueta, 0 AS PadreId
    FROM dbo.Pabellones AS p
    WHERE p.IsActive = 1 AND p.CampamentoId = @CampamentoId
    ORDER BY p.Nombre;

    SELECT m.Id, m.Nombre AS Etiqueta, m.PabellonId AS PadreId
    FROM dbo.Modulos AS m
    INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
    WHERE m.IsActive = 1 AND p.CampamentoId = @CampamentoId
    ORDER BY m.Nombre;

    SELECT l.Valor1 AS Id, l.Descripcion AS Etiqueta, 0 AS PadreId
    FROM dbo.Listas AS l WITH (NOLOCK)
    WHERE l.IdTabla = 11 AND l.Tipo = 'Detalle' AND l.Valor1 <> '0'
    ORDER BY l.Descripcion;

    SELECT l.Valor1 AS Id, l.Descripcion AS Etiqueta, 0 AS PadreId
    FROM dbo.Listas AS l WITH (NOLOCK)
    WHERE l.IdTabla = 8 AND l.Tipo = 'Detalle' AND l.Valor1 <> '0'
    ORDER BY l.Descripcion;

    SELECT t.Id, t.Nombre AS Etiqueta, 0 AS PadreId
    FROM dbo.TiposInstalacion AS t WITH (NOLOCK)
    WHERE t.IsActive = 1 AND (@MineraId = 0 OR t.EmpresaMandanteId = @MineraId)
    ORDER BY t.Nombre;

    SELECT s.Id, s.Nombre AS Etiqueta, 0 AS PadreId
    FROM dbo.StandardsHabitacion AS s WITH (NOLOCK)
    WHERE s.IsActive = 1
    ORDER BY s.Nombre;

    SELECT b.Id, b.CodigoNombre + N' (' + b.Ubicacion + N')' AS Etiqueta, 0 AS PadreId
    FROM dbo.Banos AS b WITH (NOLOCK)
    WHERE b.CampamentoId = @CampamentoId
      AND (b.IsActive = 1 OR b.Estado = N'ACTIVO')
      AND b.Estado <> N'INACTIVO'
    ORDER BY b.Ubicacion, b.CodigoNombre;

    IF OBJECT_ID(N'dbo.TipoCama', N'U') IS NOT NULL
        SELECT tc.Id,
               tc.Nombre + N' (' + CAST(tc.AnchoCm AS nvarchar(10)) + N'×' + CAST(tc.LargoCm AS nvarchar(10)) + N' cm)' AS Etiqueta,
               0 AS PadreId
        FROM dbo.TipoCama AS tc WITH (NOLOCK)
        WHERE tc.CampamentoId = @CampamentoId AND tc.IsActive = 1
        ORDER BY tc.Nombre;
END
GO

-- ============================================================================
-- Navegación: menú Configuración → Tipos de cama (perfil 1)
-- Origen: 004_menu_config_tipos_cama.sql
-- ============================================================================

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

-- ============================================================================
-- Navegación: MenusPerfil tipos de cama (ajustar @PerfilId)
-- Origen: 005_menusperfil_tipos_cama_por_perfil.sql
-- ============================================================================

SET NOCOUNT ON;

DECLARE @PerfilId int = 68;
DECLARE @Ahora2 datetime2 = SYSUTCDATETIME();
DECLARE @Usuario2 int = 1;

DECLARE @MenuTipo2 int = (SELECT TOP 1 Id FROM dbo.Menus WITH (NOLOCK) WHERE Link = N'/app/config/tiposcama' AND IsActive = 1);

IF @MenuTipo2 IS NULL
BEGIN
    RAISERROR(N'Falta menú Tipos de cama. Revise el bloque anterior en este mismo script.', 16, 1);
    RETURN;
END

DECLARE @PadreConfig2 int;
SELECT TOP (1) @PadreConfig2 = p.Id
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

IF @PadreConfig2 IS NULL
    SELECT TOP (1) @PadreConfig2 = m.Id
    FROM dbo.Menus AS m WITH (NOLOCK)
    WHERE m.IsActive = 1
      AND m.Ubicacion = N'nav'
      AND (m.Link LIKE N'%/config%' OR m.Nombre LIKE N'%Configur%')
    ORDER BY m.Id;

IF @PadreConfig2 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.MenusPerfil WHERE PerfilId = @PerfilId AND MenuId = @PadreConfig2)
    INSERT INTO dbo.MenusPerfil (PerfilId, MenuId, FechaCreacion, UsuarioCreacionId)
    VALUES (@PerfilId, @PadreConfig2, @Ahora2, @Usuario2);

IF NOT EXISTS (SELECT 1 FROM dbo.MenusPerfil WHERE PerfilId = @PerfilId AND MenuId = @MenuTipo2)
    INSERT INTO dbo.MenusPerfil (PerfilId, MenuId, FechaCreacion, UsuarioCreacionId)
    VALUES (@PerfilId, @MenuTipo2, @Ahora2, @Usuario2);

SELECT @PerfilId AS PerfilId, @PadreConfig2 AS PadreConfigId, @MenuTipo2 AS MenuTiposCamaId;
GO