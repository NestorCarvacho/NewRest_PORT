/*
    Censo. La búsqueda envuelve HAB_ObtieneCensos.
    Módulo de la pantalla es @modulo (pabellón) y Piso es @piso (módulo físico).
    El guardado envuelve CSO_GuardaCenso y conserva resultado y mensaje de cada fila.
*/

CREATE OR ALTER PROCEDURE dbo.he_sp_censo_buscar
    @EmpresaMandanteId int,
    @CampamentoId int,
    @ModuloId int,
    @PisoId int,
    @HabitacionId int,
    @FechaCenso datetime2,
    @Turno varchar(10)
AS
BEGIN
    SET NOCOUNT ON;

    IF @CampamentoId <= 0
        RETURN;

    IF @ModuloId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Pabellones
        WHERE Id = @ModuloId
          AND CampamentoId = @CampamentoId
          AND IsActive = 1)
        RETURN;

    IF @PisoId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Modulos AS m
        INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
        WHERE m.Id = @PisoId
          AND p.CampamentoId = @CampamentoId
          AND m.IsActive = 1)
        RETURN;

    IF @HabitacionId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Habitaciones AS h
        INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
        INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
        WHERE h.Id = @HabitacionId
          AND p.CampamentoId = @CampamentoId
          AND h.IsActive = 1)
        RETURN;

    EXEC dbo.HAB_ObtieneCensos
        @empresaMandante = @EmpresaMandanteId,
        @campamento = @CampamentoId,
        @modulo = @ModuloId,
        @piso = @PisoId,
        @habitacion = @HabitacionId,
        @fechaCenso = @FechaCenso,
        @turno = @Turno;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_censo_buscar_por_usuario
    @EmpresaMandanteId int,
    @CampamentoId int,
    @Fecha date,
    @UsuarioId int
AS
BEGIN
    SET NOCOUNT ON;

    EXEC dbo.CSO_ObtieneCensoByUser
        @empresaMandante = @EmpresaMandanteId,
        @campamento = @CampamentoId,
        @fecha = @Fecha,
        @usuario = @UsuarioId;
END
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

CREATE OR ALTER PROCEDURE dbo.he_sp_censo_filtro_pertenece
    @CampamentoId int,
    @ModuloId int,
    @PisoId int,
    @HabitacionId int
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @pertenece bit = 1;

    IF @CampamentoId <= 0
        SET @pertenece = 0;

    IF @ModuloId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Pabellones
        WHERE Id = @ModuloId
          AND CampamentoId = @CampamentoId
          AND IsActive = 1)
        SET @pertenece = 0;

    IF @PisoId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Modulos AS m
        INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
        WHERE m.Id = @PisoId
          AND p.CampamentoId = @CampamentoId
          AND m.IsActive = 1)
        SET @pertenece = 0;

    IF @HabitacionId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Habitaciones AS h
        INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
        INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
        WHERE h.Id = @HabitacionId
          AND p.CampamentoId = @CampamentoId
          AND h.IsActive = 1)
        SET @pertenece = 0;

    SELECT @pertenece AS Pertenece;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_censo_guardar
    @Id varchar(20),
    @HabitacionId int,
    @Cantidad int,
    @Fecha datetime2,
    @Turno varchar(10),
    @UsuarioId int,
    @CampamentoId int
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Habitaciones AS h
        INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
        INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
        WHERE h.Id = @HabitacionId
          AND p.CampamentoId = @CampamentoId
          AND h.IsActive = 1)
    BEGIN
        SELECT 1 AS Resultado, N'La habitación no corresponde al campamento de la sesión.' AS Mensaje;
        RETURN;
    END

    DECLARE @ret int;
    EXEC dbo.CSO_GuardaCenso
        @id = @Id,
        @habitacion = @HabitacionId,
        @cantidad = @Cantidad,
        @fecha = @Fecha,
        @tipoTurno = @Turno,
        @usuario = @UsuarioId,
        @ret = @ret OUTPUT;
END
GO
