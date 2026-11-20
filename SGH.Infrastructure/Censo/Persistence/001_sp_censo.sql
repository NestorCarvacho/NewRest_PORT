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
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-19
    --  description: Obtiene los censos de un campamento, con opción de filtrar por módulo, piso y habitación.
    --======================================================

    SET NOCOUNT ON;

    IF @CampamentoId <= 0
        RETURN;

    IF @ModuloId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Pabellones WITH (NOLOCK)
        WHERE Id = @ModuloId
          AND CampamentoId = @CampamentoId
          AND IsActive = 1)
        RETURN;

    IF @PisoId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Modulos AS m WITH (NOLOCK)
        INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
        WHERE m.Id = @PisoId
          AND p.CampamentoId = @CampamentoId
          AND m.IsActive = 1)
        RETURN;

    IF @HabitacionId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Habitaciones AS h WITH (NOLOCK)
        INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
        INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
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
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-19
    --  description: Obtiene los censos de un usuario en un campamento específico.
    --======================================================

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
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-19
    --  description: Lista los pabellones, módulos y habitaciones de un campamento específico.
    --======================================================

    SET NOCOUNT ON;

    SELECT p.Id, p.Nombre AS Etiqueta, 0 AS PadreId
    FROM dbo.Pabellones AS p WITH (NOLOCK)
    WHERE p.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY p.Nombre;

    SELECT m.Id, m.Nombre AS Etiqueta, m.PabellonId AS PadreId
    FROM dbo.Modulos AS m WITH (NOLOCK)
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    WHERE m.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY m.Nombre;

    SELECT h.Id, h.Nombre AS Etiqueta, h.ModuloId AS PadreId
    FROM dbo.Habitaciones AS h WITH (NOLOCK)
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    WHERE h.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY h.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_censo_filtro_pertenece
    @CampamentoId int,
    @ModuloId int,
    @PisoId int,
    @HabitacionId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-19
    --  description: Verifica si pabellón, piso (módulo) u habitación del filtro pertenecen al campamento.
    --======================================================

    SET NOCOUNT ON;

    DECLARE @pertenece bit = 1;

    IF @CampamentoId <= 0
        SET @pertenece = 0;

    IF @ModuloId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Pabellones WITH (NOLOCK)
        WHERE Id = @ModuloId
          AND CampamentoId = @CampamentoId
          AND IsActive = 1)
        SET @pertenece = 0;

    IF @PisoId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Modulos AS m WITH (NOLOCK)
        INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
        WHERE m.Id = @PisoId
          AND p.CampamentoId = @CampamentoId
          AND m.IsActive = 1)
        SET @pertenece = 0;

    IF @HabitacionId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Habitaciones AS h WITH (NOLOCK)
        INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
        INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
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
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-19
    --  description: Persiste camas ocupadas de una fila de censo vía CSO_GuardaCenso; valida habitación en campamento.
    --======================================================

    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Habitaciones AS h WITH (NOLOCK)
        INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
        INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
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