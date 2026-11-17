/*
    Depuración de INF_Aseos cuando el informe no devuelve filas.
    Ejecutar en HOTELDB (SSMS), sección por sección.
    Mismos predicados de fechas que legado y script optimizado.
*/
SET NOCOUNT ON;

DECLARE
    @empresaMandante  INT = 6955,
    @campamento       INT = 32,
    @modulo           INT = 3407,
    @piso             INT = 0,
    @habitacion       INT = 0,
    @usuarioCreacion  INT = 0,
    @fechaDesde       DATETIME = '2026-10-01',
    @fechaHasta       DATETIME = '2026-11-01',
    @minera           INT = 0,              -- reemplazar con la minera real de la sesión si aplica
    @tipoModuloId     INT = 8,
    @tipoInstalacion  INT = 0,
    @grupoServicioId  INT = 0;

DECLARE @fechaHastaExclusiva DATETIME = DATEADD(DAY, 1, @fechaHasta);

/* --- 1) Parámetros que suelen quedar NULL en pruebas ad-hoc --- */
SELECT
    @usuarioCreacion AS usuarioCreacion,
    @piso AS piso,
    @habitacion AS habitacion,
    CASE
        WHEN @usuarioCreacion IS NULL THEN 'usuarioCreacion NULL: el WHERE legado excluye todo (use 0)'
        WHEN @piso IS NULL THEN 'piso NULL: idem'
        WHEN @habitacion IS NULL THEN 'habitacion NULL: idem'
        ELSE 'Parámetros opcionales OK (0 o valor)'
    END AS alertaParametros;

/* --- 2) ¿El módulo (Pabellón) pertenece al campamento y al tipo de módulo? --- */
SELECT
    p.Id AS PabellonId,
    p.Nombre AS PabellonNombre,
    p.CampamentoId,
    p.TipoModuloId,
    tm.Nombre AS TipoModuloNombre,
    c.Nombre AS CampamentoNombre,
    c.EmpresaMandanteId
FROM dbo.Pabellones AS p WITH (NOLOCK)
LEFT JOIN dbo.Campamentos AS c WITH (NOLOCK) ON c.Id = p.CampamentoId
LEFT JOIN dbo.TiposModulo AS tm WITH (NOLOCK) ON tm.Id = p.TipoModuloId
WHERE p.Id = @modulo;

SELECT CASE
    WHEN NOT EXISTS (SELECT 1 FROM dbo.Pabellones WHERE Id = @modulo)
        THEN 'Pabellón @modulo no existe'
    WHEN NOT EXISTS (
        SELECT 1 FROM dbo.Pabellones
        WHERE Id = @modulo AND CampamentoId = @campamento
    )
        THEN 'Pabellón no está en @campamento: quita @modulo o corrige el id'
    WHEN @tipoModuloId <> 0 AND NOT EXISTS (
        SELECT 1 FROM dbo.Pabellones
        WHERE Id = @modulo AND TipoModuloId = @tipoModuloId
    )
        THEN '@tipoModuloId no coincide con el pabellón: quita tipo o elige otro módulo'
    ELSE 'Ubicación módulo/campamento/tipo coherente'
END AS alertaUbicacion;

/* --- 3) Conteos en capas (mismos JOINs que el informe) --- */
SELECT COUNT(*) AS aseos_en_campamento
FROM dbo.Aseo AS a WITH (NOLOCK)
INNER JOIN dbo.Habitaciones AS h WITH (NOLOCK) ON h.Id = a.HabitacionId
INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
WHERE p.CampamentoId = @campamento;

SELECT COUNT(*) AS mas_filtro_modulo_y_tipo
FROM dbo.Aseo AS a WITH (NOLOCK)
INNER JOIN dbo.Habitaciones AS h WITH (NOLOCK) ON h.Id = a.HabitacionId
INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
WHERE p.CampamentoId = @campamento
  AND (@modulo = 0 OR p.Id = @modulo)
  AND (@tipoModuloId = 0 OR p.TipoModuloId = @tipoModuloId);

SELECT COUNT(*) AS mas_filtro_fechas
FROM dbo.Aseo AS a WITH (NOLOCK)
INNER JOIN dbo.Habitaciones AS h WITH (NOLOCK) ON h.Id = a.HabitacionId
INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
WHERE p.CampamentoId = @campamento
  AND (@modulo = 0 OR p.Id = @modulo)
  AND (@tipoModuloId = 0 OR p.TipoModuloId = @tipoModuloId)
  AND (
        (@fechaDesde IS NULL OR a.FechaCreacion >= @fechaDesde)
            AND (@fechaHastaExclusiva IS NULL OR a.FechaCreacion < @fechaHastaExclusiva)
        OR
        (@fechaDesde IS NULL OR a.FechaInicioAseo >= @fechaDesde)
            AND (@fechaHastaExclusiva IS NULL OR a.FechaInicioAseo < @fechaHastaExclusiva)
      );

/* --- 4) Muestra de fechas cercanas si el conteo de fechas es 0 --- */
SELECT TOP (20)
    a.Id,
    a.FechaCreacion,
    a.FechaInicioAseo,
    p.Id AS PabellonId,
    h.Nombre AS Habitacion
FROM dbo.Aseo AS a WITH (NOLOCK)
INNER JOIN dbo.Habitaciones AS h WITH (NOLOCK) ON h.Id = a.HabitacionId
INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
WHERE p.CampamentoId = @campamento
  AND (@modulo = 0 OR p.Id = @modulo)
ORDER BY ISNULL(a.FechaCreacion, a.FechaInicioAseo) DESC;

GO