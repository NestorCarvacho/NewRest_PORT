/* Censo QR: catálogo ampliado y detalle de habitación.
   20261123 ncarvacho: Censo QR — catálogo con tipos, habitaciones con capacidad y he_sp_censo_obtener_habitacion_qr. */
SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_censo_listar_catalogo
    @CampamentoId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.Id, p.Nombre AS Etiqueta, 0 AS PadreId
    FROM dbo.Pabellones AS p
    WHERE p.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY p.Nombre;

    SELECT m.Id, m.Nombre AS Etiqueta, m.PabellonId AS PadreId
    FROM dbo.Modulos AS m
    INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
    WHERE m.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY m.Nombre;

    SELECT h.Id, h.Nombre AS Etiqueta, h.ModuloId AS PadreId
    FROM dbo.Habitaciones AS h
    INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
    WHERE h.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY h.Nombre;

    SELECT t.Id, t.Nombre AS Etiqueta, 0 AS PadreId
    FROM dbo.TiposInstalacion AS t WITH (NOLOCK)
    WHERE t.IsActive = 1
      AND (t.EmpresaMandanteId IS NULL OR EXISTS (
          SELECT 1 FROM dbo.Campamentos AS c
          WHERE c.Id = @CampamentoId AND c.EmpresaMandanteId = t.EmpresaMandanteId))
    ORDER BY t.Nombre;

    SELECT
        h.Id,
        h.Nombre AS Etiqueta,
        h.ModuloId AS PadreId,
        ISNULL(h.TipoInstalacionId, 0) AS TipoInstalacionId,
        ISNULL((
            SELECT COUNT(*)
            FROM dbo.Camas AS c WITH (NOLOCK)
            WHERE c.HabitacionId = h.Id AND c.IsActive = 1 AND c.Habilitada = 1
        ), CASE WHEN ISNULL(h.CantidadCamas, 0) > 0 THEN h.CantidadCamas ELSE 0 END) AS Capacidad
    FROM dbo.Habitaciones AS h
    INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
    WHERE h.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY h.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_censo_obtener_habitacion_qr
    @CampamentoId int,
    @HabitacionId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        h.Id AS HabitacionId,
        h.Nombre AS Habitacion,
        p.Nombre AS Modulo,
        m.Nombre AS Piso,
        ISNULL((
            SELECT COUNT(*)
            FROM dbo.Camas AS c WITH (NOLOCK)
            WHERE c.HabitacionId = h.Id AND c.IsActive = 1 AND c.Habilitada = 1
        ), CASE WHEN ISNULL(h.CantidadCamas, 0) > 0 THEN h.CantidadCamas ELSE 0 END) AS Capacidad
    FROM dbo.Habitaciones AS h
    INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
    WHERE h.Id = @HabitacionId
      AND h.IsActive = 1
      AND p.CampamentoId = @CampamentoId;
END
GO