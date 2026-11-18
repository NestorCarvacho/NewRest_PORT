/* CU-HAB-02 — TipoCama y camas con inventario */

CREATE OR ALTER PROCEDURE dbo.he_sp_tipo_cama_listar
    @CampamentoId int,
    @SoloActivos int = 1,
    @TipoCamaId int = 0
AS
BEGIN
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

CREATE OR ALTER PROCEDURE dbo.he_sp_tipo_cama_guardar
    @Id int,
    @CampamentoId int,
    @Nombre nvarchar(100),
    @AnchoCm smallint,
    @LargoCm smallint,
    @IsActive bit,
    @UsuarioId int,
    @IdResultado int OUTPUT,
    @Mensaje nvarchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Mensaje = NULL;
    SET @IdResultado = 0;

    IF @CampamentoId <= 0
    BEGIN
        SET @Mensaje = N'Campamento inválido.';
        RETURN;
    END

    IF NULLIF(LTRIM(RTRIM(@Nombre)), N'') IS NULL
    BEGIN
        SET @Mensaje = N'Ingrese el nombre del tipo de cama.';
        RETURN;
    END

    IF @AnchoCm <= 0 OR @LargoCm <= 0
    BEGIN
        SET @Mensaje = N'Ancho y largo deben ser mayores que cero.';
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM dbo.TipoCama WITH (NOLOCK)
        WHERE CampamentoId = @CampamentoId
          AND Nombre = @Nombre
          AND IsActive = 1
          AND Id <> @Id)
    BEGIN
        SET @Mensaje = N'Ya existe un tipo de cama activo con ese nombre.';
        RETURN;
    END

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.TipoCama (CampamentoId, Nombre, AnchoCm, LargoCm, IsActive, UsuarioCreacionId)
        VALUES (@CampamentoId, @Nombre, @AnchoCm, @LargoCm, @IsActive, @UsuarioId);
        SET @IdResultado = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.TipoCama WHERE Id = @Id AND CampamentoId = @CampamentoId)
        BEGIN
            SET @Mensaje = N'El tipo de cama no pertenece al campamento.';
            RETURN;
        END

        UPDATE dbo.TipoCama
        SET Nombre = @Nombre,
            AnchoCm = @AnchoCm,
            LargoCm = @LargoCm,
            IsActive = @IsActive,
            FechaActualizacion = SYSUTCDATETIME(),
            UsuarioActualizacionId = @UsuarioId
        WHERE Id = @Id;
        SET @IdResultado = @Id;
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_cama_listar_por_habitacion
    @HabitacionId int,
    @CampamentoId int,
    @SoloActivas int = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id,
        c.HabitacionId,
        c.CodigoCama,
        c.Habilitada,
        c.Orden,
        c.IsActive,
        c.TipoCamaId,
        t.Nombre AS TipoCamaNombre,
        t.AnchoCm AS TipoCamaAnchoCm,
        t.LargoCm AS TipoCamaLargoCm,
        c.FechaColchon,
        c.ColchonObservacion,
        c.FechaCambioRopa,
        c.RopaCamaObservacion
    FROM dbo.Camas AS c WITH (NOLOCK)
    INNER JOIN dbo.Habitaciones AS h WITH (NOLOCK) ON h.Id = c.HabitacionId
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    LEFT JOIN dbo.TipoCama AS t WITH (NOLOCK) ON t.Id = c.TipoCamaId
    WHERE c.HabitacionId = @HabitacionId
      AND p.CampamentoId = @CampamentoId
      AND (@SoloActivas = 0 OR c.IsActive = 1)
    ORDER BY c.Orden, c.CodigoCama;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_cama_guardar
    @Id int,
    @HabitacionId int,
    @CodigoCama nvarchar(50),
    @Habilitada bit,
    @Orden int,
    @IsActive bit,
    @TipoCamaId int = NULL,
    @FechaColchon date = NULL,
    @ColchonObservacion nvarchar(500) = NULL,
    @FechaCambioRopa date = NULL,
    @RopaCamaObservacion nvarchar(500) = NULL,
    @CampamentoId int,
    @UsuarioId int,
    @IdResultado int OUTPUT,
    @Mensaje nvarchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Mensaje = NULL;
    SET @IdResultado = 0;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Habitaciones AS h
        INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
        INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
        WHERE h.Id = @HabitacionId AND p.CampamentoId = @CampamentoId)
    BEGIN
        SET @Mensaje = N'La habitación no pertenece al campamento.';
        RETURN;
    END

    IF @TipoCamaId IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM dbo.TipoCama
        WHERE Id = @TipoCamaId AND CampamentoId = @CampamentoId AND IsActive = 1)
    BEGIN
        SET @Mensaje = N'El tipo de cama no es válido para el campamento.';
        RETURN;
    END

    IF @Id > 0
    BEGIN
        DECLARE @HabitacionRegistrada int;
        SELECT @HabitacionRegistrada = c.HabitacionId
        FROM dbo.Camas AS c
        WHERE c.Id = @Id;

        IF @HabitacionRegistrada IS NULL
        BEGIN
            SET @Mensaje = N'La cama no existe.';
            RETURN;
        END

        IF @HabitacionRegistrada <> @HabitacionId
        BEGIN
            SET @Mensaje = N'Una cama no puede asignarse a otra habitación.';
            RETURN;
        END
    END

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.Camas (
            HabitacionId, CodigoCama, Habilitada, Orden, IsActive,
            TipoCamaId, FechaColchon, ColchonObservacion, FechaCambioRopa, RopaCamaObservacion,
            UsuarioCreacionId)
        VALUES (
            @HabitacionId, @CodigoCama, @Habilitada, @Orden, @IsActive,
            @TipoCamaId, @FechaColchon, @ColchonObservacion, @FechaCambioRopa, @RopaCamaObservacion,
            @UsuarioId);
        SET @IdResultado = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE c
        SET CodigoCama = @CodigoCama,
            Habilitada = @Habilitada,
            Orden = @Orden,
            IsActive = @IsActive,
            TipoCamaId = @TipoCamaId,
            FechaColchon = @FechaColchon,
            ColchonObservacion = @ColchonObservacion,
            FechaCambioRopa = @FechaCambioRopa,
            RopaCamaObservacion = @RopaCamaObservacion,
            FechaActualizacion = SYSUTCDATETIME(),
            UsuarioActualizacionId = @UsuarioId
        FROM dbo.Camas AS c
        WHERE c.Id = @Id AND c.HabitacionId = @HabitacionId;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @Mensaje = N'No se pudo actualizar la cama.';
            RETURN;
        END

        SET @IdResultado = @Id;
    END

    DECLARE @fisicas int, @operativas int;
    SELECT @fisicas = COUNT(*) FROM dbo.Camas WHERE HabitacionId = @HabitacionId AND IsActive = 1;
    SELECT @operativas = COUNT(*) FROM dbo.Camas WHERE HabitacionId = @HabitacionId AND IsActive = 1 AND Habilitada = 1;

    UPDATE dbo.Habitaciones
    SET CantidadCamas = CASE WHEN @fisicas > 0 THEN @fisicas ELSE CantidadCamas END
    WHERE Id = @HabitacionId;
END
GO

/* Octavo result set: tipos de cama activos (requiere dbo.TipoCama). */
CREATE OR ALTER PROCEDURE dbo.he_sp_habitacion_catalogo
    @CampamentoId int,
    @MineraId int
AS
BEGIN
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