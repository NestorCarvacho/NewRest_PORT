SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Banos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Banos
    (
        Id                   int            NOT NULL IDENTITY(1, 1),
        CampamentoId         int            NOT NULL,
        CodigoNombre         nvarchar(100)  NOT NULL,
        Ubicacion            nvarchar(200)  NOT NULL,
        TipoSexo             varchar(10)    NOT NULL,
        Estado               varchar(20)    NOT NULL CONSTRAINT DF_Banos_Estado DEFAULT ('ACTIVO'),
        IsActive             bit            NOT NULL CONSTRAINT DF_Banos_IsActive DEFAULT (1),
        FechaCreacion        datetime2(7)   NOT NULL CONSTRAINT DF_Banos_FechaCreacion DEFAULT (SYSUTCDATETIME()),
        UsuarioCreacionId    int            NULL,
        FechaActualizacion   datetime2(7)   NULL,
        UsuarioActualizacionId int          NULL,
        CONSTRAINT PK_Banos PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Banos_Campamento FOREIGN KEY (CampamentoId) REFERENCES dbo.Campamentos (Id)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_Banos_Campamento_Codigo_Ubicacion
        ON dbo.Banos (CampamentoId, CodigoNombre, Ubicacion)
        WHERE IsActive = 1;
END
GO

IF OBJECT_ID(N'dbo.Camas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Camas
    (
        Id                   int            NOT NULL IDENTITY(1, 1),
        HabitacionId         int            NOT NULL,
        CodigoCama           nvarchar(50)   NOT NULL,
        Habilitada           bit            NOT NULL CONSTRAINT DF_Camas_Habilitada DEFAULT (1),
        Orden                int            NOT NULL CONSTRAINT DF_Camas_Orden DEFAULT (0),
        IsActive             bit            NOT NULL CONSTRAINT DF_Camas_IsActive DEFAULT (1),
        FechaCreacion        datetime2(7)   NOT NULL CONSTRAINT DF_Camas_FechaCreacion DEFAULT (SYSUTCDATETIME()),
        UsuarioCreacionId    int            NULL,
        FechaActualizacion   datetime2(7)   NULL,
        UsuarioActualizacionId int          NULL,
        CONSTRAINT PK_Camas PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Camas_Habitacion FOREIGN KEY (HabitacionId) REFERENCES dbo.Habitaciones (Id)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_Camas_Habitacion_Codigo
        ON dbo.Camas (HabitacionId, CodigoCama)
        WHERE IsActive = 1;
END
GO

IF COL_LENGTH(N'dbo.Habitaciones', N'BanoId') IS NULL
    ALTER TABLE dbo.Habitaciones ADD BanoId int NULL;
GO

IF COL_LENGTH(N'dbo.Habitaciones', N'BanoId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Habitaciones_Banos')
BEGIN
    ALTER TABLE dbo.Habitaciones WITH NOCHECK
        ADD CONSTRAINT FK_Habitaciones_Banos FOREIGN KEY (BanoId) REFERENCES dbo.Banos (Id);
END
GO

IF COL_LENGTH(N'dbo.Habitaciones', N'PermiteUsoExclusivo') IS NULL
    ALTER TABLE dbo.Habitaciones ADD PermiteUsoExclusivo bit NOT NULL CONSTRAINT DF_Habitaciones_PermiteUsoExclusivo DEFAULT (0);
GO

IF COL_LENGTH(N'dbo.Habitaciones', N'PermiteUsoPorTurnos') IS NULL
    ALTER TABLE dbo.Habitaciones ADD PermiteUsoPorTurnos bit NOT NULL CONSTRAINT DF_Habitaciones_PermiteUsoPorTurnos DEFAULT (0);
GO

IF COL_LENGTH(N'dbo.Habitaciones', N'TiempoHigienizacionMin') IS NULL
    ALTER TABLE dbo.Habitaciones ADD TiempoHigienizacionMin int NULL;
GO

IF OBJECT_ID(N'dbo.Camas', N'U') IS NOT NULL
BEGIN
    ;WITH n AS (
        SELECT 1 AS numero
        UNION ALL
        SELECT numero + 1 FROM n WHERE numero < 100
    )
    INSERT INTO dbo.Camas (HabitacionId, CodigoCama, Habilitada, Orden, IsActive)
    SELECT h.Id,
           CONCAT(N'Cama ', n.numero),
           CASE WHEN h.IsActive = 1 THEN 1 ELSE 0 END,
           n.numero,
           1
    FROM dbo.Habitaciones AS h WITH (NOLOCK)
    INNER JOIN n ON n.numero <= CASE WHEN ISNULL(h.CantidadCamas, 0) < 1 THEN 1 ELSE h.CantidadCamas END
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Camas AS c WHERE c.HabitacionId = h.Id AND c.IsActive = 1)
    OPTION (MAXRECURSION 200);
END
GO

UPDATE h
SET PermiteUsoPorTurnos = CASE WHEN ISNULL(h.MinutosMin, 0) > 0 OR ISNULL(h.MinutosMax, 0) > 0 THEN 1 ELSE 0 END,
    PermiteUsoExclusivo = CASE WHEN ISNULL(h.PersonasCantidadMax, 0) >= ISNULL(h.CantidadCamas, 1) AND h.CantidadCamas > 0 THEN 1 ELSE 0 END,
    TiempoHigienizacionMin = CASE WHEN h.TiempoHigienizacionMin IS NULL THEN h.MinutosMin ELSE h.TiempoHigienizacionMin END
FROM dbo.Habitaciones AS h
WHERE h.PermiteUsoPorTurnos = 0
  AND (ISNULL(h.MinutosMin, 0) > 0 OR ISNULL(h.MinutosMax, 0) > 0);
GO

GO

IF OBJECT_ID(N'dbo.TipoCama', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TipoCama
    (
        Id                     int            NOT NULL IDENTITY(1, 1),
        CampamentoId           int            NOT NULL,
        Nombre                 nvarchar(100)  NOT NULL,
        AnchoCm                smallint       NOT NULL,
        LargoCm                smallint       NOT NULL,
        IsActive               bit            NOT NULL CONSTRAINT DF_TipoCama_IsActive DEFAULT (1),
        FechaCreacion          datetime2(7)   NOT NULL CONSTRAINT DF_TipoCama_FechaCreacion DEFAULT (SYSUTCDATETIME()),
        UsuarioCreacionId      int            NULL,
        FechaActualizacion     datetime2(7)   NULL,
        UsuarioActualizacionId int            NULL,
        CONSTRAINT PK_TipoCama PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_TipoCama_Campamento FOREIGN KEY (CampamentoId) REFERENCES dbo.Campamentos (Id),
        CONSTRAINT CK_TipoCama_AnchoPos CHECK (AnchoCm > 0),
        CONSTRAINT CK_TipoCama_LargoPos CHECK (LargoCm > 0)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_TipoCama_Campamento_Nombre
        ON dbo.TipoCama (CampamentoId, Nombre)
        WHERE IsActive = 1;
END
GO

IF COL_LENGTH(N'dbo.Camas', N'TipoCamaId') IS NULL
    ALTER TABLE dbo.Camas ADD TipoCamaId int NULL;
GO

IF COL_LENGTH(N'dbo.Camas', N'TipoCamaId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Camas_TipoCama')
BEGIN
    ALTER TABLE dbo.Camas WITH NOCHECK
        ADD CONSTRAINT FK_Camas_TipoCama FOREIGN KEY (TipoCamaId) REFERENCES dbo.TipoCama (Id);
END
GO

IF COL_LENGTH(N'dbo.Camas', N'FechaColchon') IS NULL
    ALTER TABLE dbo.Camas ADD FechaColchon date NULL;
GO

IF COL_LENGTH(N'dbo.Camas', N'ColchonObservacion') IS NULL
    ALTER TABLE dbo.Camas ADD ColchonObservacion nvarchar(500) NULL;
GO

IF COL_LENGTH(N'dbo.Camas', N'FechaCambioRopa') IS NULL
    ALTER TABLE dbo.Camas ADD FechaCambioRopa date NULL;
GO

IF COL_LENGTH(N'dbo.Camas', N'RopaCamaObservacion') IS NULL
    ALTER TABLE dbo.Camas ADD RopaCamaObservacion nvarchar(500) NULL;
GO

GO

UPDATE dbo.Banos
SET IsActive = 1
WHERE Estado = N'ACTIVO'
  AND IsActive = 0;
GO

GO

CREATE OR ALTER PROCEDURE dbo.he_sp_bano_listar
    @CampamentoId int,
    @SoloActivos int = 1,
    @BanoId int = 0
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Lista baños del campamento y cantidad de habitaciones vinculadas.
    --======================================================

SET NOCOUNT ON;

    SELECT
        b.Id,
        b.CodigoNombre,
        b.Ubicacion,
        b.TipoSexo,
        b.Estado,
        CAST(ISNULL(b.IsActive, 0) AS bit) AS IsActive,
        (SELECT COUNT(*) FROM dbo.Habitaciones AS h WITH (NOLOCK)
         WHERE h.BanoId = b.Id AND h.IsActive = 1) AS HabitacionesVinculadas
    FROM dbo.Banos AS b WITH (NOLOCK)
    WHERE b.CampamentoId = @CampamentoId
      AND (@SoloActivos = 0 OR b.IsActive = 1)
      AND (@BanoId = 0 OR b.Id = @BanoId)
    ORDER BY b.Ubicacion, b.CodigoNombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_bano_habitaciones_activas_por_bano
    @BanoId int,
    @CampamentoId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Habitaciones activas asociadas a un baño en el campamento.
    --======================================================

SET NOCOUNT ON;

    SELECT
        h.Id,
        h.Nombre
    FROM dbo.Habitaciones AS h WITH (NOLOCK)
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    WHERE h.BanoId = @BanoId
      AND h.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY h.Nombre, h.Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_bano_guardar
    @Id int,
    @CampamentoId int,
    @CodigoNombre nvarchar(100),
    @Ubicacion nvarchar(200),
    @TipoSexo varchar(10),
    @Estado varchar(20),
    @IsActive bit,
    @UsuarioId int,
    @IdResultado int OUTPUT,
    @Mensaje nvarchar(500) OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Alta y edición de baño; aplica RN-01 inactivación y RN-02 compatibilidad de sexo.
    --======================================================

SET NOCOUNT ON;
    SET @Mensaje = NULL;
    SET @IdResultado = 0;

    IF @CampamentoId <= 0
    BEGIN
        SET @Mensaje = N'Campamento inválido.';
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM dbo.Banos WITH (NOLOCK)
        WHERE CampamentoId = @CampamentoId
          AND CodigoNombre = @CodigoNombre
          AND Ubicacion = @Ubicacion
          AND IsActive = 1
          AND Id <> @Id)
    BEGIN
        SET @Mensaje = N'Ya existe un baño activo con el mismo código en esa ubicación.';
        RETURN;
    END

    IF @Id > 0 AND @IsActive = 0
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.Habitaciones WITH (NOLOCK) WHERE BanoId = @Id AND IsActive = 1)
        BEGIN
            SET @Mensaje = N'RN-01: No puede inactivar el baño mientras tenga habitaciones activas vinculadas.';
            RETURN;
        END
    END

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.Banos (CampamentoId, CodigoNombre, Ubicacion, TipoSexo, Estado, IsActive, UsuarioCreacionId)
        VALUES (@CampamentoId, @CodigoNombre, @Ubicacion, @TipoSexo, @Estado, @IsActive, @UsuarioId);
        SET @IdResultado = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Banos WHERE Id = @Id AND CampamentoId = @CampamentoId)
        BEGIN
            SET @Mensaje = N'El baño no pertenece al campamento.';
            RETURN;
        END

        IF EXISTS (
            SELECT 1
            FROM dbo.Habitaciones AS h WITH (NOLOCK)
            WHERE h.BanoId = @Id AND h.IsActive = 1
              AND NOT (
                    @TipoSexo IN ('MIXTO', 'M', 'F')
                    OR (@TipoSexo = 'M' AND h.Sexo IN ('M', 'MIXTO', 'HOMBRE'))
                    OR (@TipoSexo = 'F' AND h.Sexo IN ('F', 'MIXTO', 'MUJER'))
                    OR (@TipoSexo = 'MUJER' AND h.Sexo IN ('F', 'MIXTO', 'MUJER'))
                    OR (@TipoSexo = 'HOMBRE' AND h.Sexo IN ('M', 'MIXTO', 'HOMBRE'))
                  ))
        BEGIN
            SET @Mensaje = N'RN-02: Hay habitaciones vinculadas con sexo incompatible con el baño.';
            RETURN;
        END

        UPDATE dbo.Banos
        SET CodigoNombre = @CodigoNombre,
            Ubicacion = @Ubicacion,
            TipoSexo = @TipoSexo,
            Estado = @Estado,
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
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Camas de una habitación con tipo e inventario (CU-HAB-02).
    --======================================================

SET NOCOUNT ON;

    SELECT
        c.Id,
        c.HabitacionId,
        c.CodigoCama,
        c.Habilitada,
        c.Orden,
        c.IsActive
    FROM dbo.Camas AS c WITH (NOLOCK)
    INNER JOIN dbo.Habitaciones AS h WITH (NOLOCK) ON h.Id = c.HabitacionId
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
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
    @CampamentoId int,
    @UsuarioId int,
    @IdResultado int OUTPUT,
    @Mensaje nvarchar(500) OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Alta y edición de cama; actualiza conteos en Habitaciones.
    --======================================================

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
        INSERT INTO dbo.Camas (HabitacionId, CodigoCama, Habilitada, Orden, IsActive, UsuarioCreacionId)
        VALUES (@HabitacionId, @CodigoCama, @Habilitada, @Orden, @IsActive, @UsuarioId);
        SET @IdResultado = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE c
        SET CodigoCama = @CodigoCama,
            Habilitada = @Habilitada,
            Orden = @Orden,
            IsActive = @IsActive,
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

CREATE OR ALTER PROCEDURE dbo.he_sp_habitacion_sincronizar_camas
    @HabitacionId int,
    @CantidadObjetivo int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Crea o ajusta filas en Camas según CantidadCamas de la habitación.
    --======================================================

SET NOCOUNT ON;

    IF @CantidadObjetivo < 1
        THROW 51080, 'La habitación debe tener al menos 1 cama.', 1;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Habitaciones AS h
        INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
        INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
        WHERE h.Id = @HabitacionId AND p.CampamentoId = @CampamentoId)
        THROW 51081, 'La habitación no pertenece al campamento.', 1;

    WHILE (SELECT COUNT(*) FROM dbo.Camas WHERE HabitacionId = @HabitacionId AND IsActive = 1) < @CantidadObjetivo
    BEGIN
        DECLARE @n int = (SELECT COUNT(*) FROM dbo.Camas WHERE HabitacionId = @HabitacionId AND IsActive = 1) + 1;
        INSERT INTO dbo.Camas (HabitacionId, CodigoCama, Habilitada, Orden, IsActive, UsuarioCreacionId)
        VALUES (@HabitacionId, CONCAT(N'Cama ', @n), 1, @n, 1, @UsuarioId);
    END

    UPDATE dbo.Camas
    SET IsActive = 0, FechaActualizacion = SYSUTCDATETIME(), UsuarioActualizacionId = @UsuarioId
    WHERE HabitacionId = @HabitacionId
      AND IsActive = 1
      AND Orden > @CantidadObjetivo;

    UPDATE dbo.Habitaciones SET CantidadCamas = @CantidadObjetivo WHERE Id = @HabitacionId;
END
GO

GO

CREATE OR ALTER PROCEDURE dbo.he_sp_tipo_cama_listar
    @CampamentoId int,
    @SoloActivos int = 1,
    @TipoCamaId int = 0
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
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
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Alta y edición de tipo de cama.
    --======================================================

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
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Camas de una habitación con tipo e inventario (CU-HAB-02).
    --======================================================

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
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Alta y edición de cama; actualiza conteos en Habitaciones.
    --======================================================

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

CREATE OR ALTER PROCEDURE dbo.he_sp_habitacion_catalogo
    @CampamentoId int,
    @MineraId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Result sets de catálogo para formularios (pabellones, listas, baños, tipos).
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

GO

CREATE OR ALTER PROCEDURE dbo.he_sp_habitacion_id_por_nombre
    @CampamentoId int,
    @Nombre       nvarchar(100)
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Obtiene Id de habitación por nombre en el campamento (carga masiva).
    --======================================================

SET NOCOUNT ON;

    SELECT TOP (1) h.Id
    FROM dbo.Habitaciones AS h WITH (NOLOCK)
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    WHERE p.CampamentoId = @CampamentoId
      AND LTRIM(RTRIM(h.Nombre)) = LTRIM(RTRIM(@Nombre));
END
GO

GO

CREATE OR ALTER PROCEDURE dbo.he_sp_habitacion_listar
    @EmpresaMandanteId int,
    @CampamentoId int,
    @PabellonId int = 0,
    @ModuloId int = 0,
    @TipoInstalacionId int = 0,
    @Estado varchar(10) = NULL,
    @SoloActivas int = 1,
    @SoloReservables bit = NULL,
    @HabitacionId int = 0
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Listado de habitaciones con baño y flags de uso CU-HAB.
    --======================================================

SET NOCOUNT ON;

    SELECT
        h.Id,
        h.Nombre,
        h.Sexo,
        h.Estado,
        ISNULL((SELECT COUNT(*) FROM dbo.Camas AS c WITH (NOLOCK) WHERE c.HabitacionId = h.Id AND c.IsActive = 1), h.CantidadCamas) AS CantidadCamas,
        ISNULL((SELECT COUNT(*) FROM dbo.Camas AS c WITH (NOLOCK) WHERE c.HabitacionId = h.Id AND c.IsActive = 1 AND c.Habilitada = 1),
               CASE WHEN h.IsActive = 1 THEN h.CantidadCamas ELSE 0 END) AS CamasHabilitadas,
        CAST(ISNULL(h.IsActive, 0) AS bit) AS IsActive,
        CAST(ISNULL(h.IsReservable, 0) AS bit) AS IsReservable,
        m.Id AS ModuloId,
        m.Nombre AS ModuloNombre,
        p.Id AS PabellonId,
        p.Nombre AS PabellonNombre,
        h.BanoId,
        b.CodigoNombre AS BanoNombre,
        CAST(ISNULL(h.PermiteUsoPorTurnos, CASE WHEN h.MinutosMin > 0 OR h.MinutosMax > 0 THEN 1 ELSE 0 END) AS bit) AS PermiteUsoPorTurnos,
        CAST(ISNULL(h.PermiteUsoExclusivo, CASE WHEN h.PersonasCantidadMax >= h.CantidadCamas AND h.CantidadCamas > 0 THEN 1 ELSE 0 END) AS bit) AS PermiteUsoExclusivo
    FROM dbo.Habitaciones AS h WITH (NOLOCK)
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    LEFT JOIN dbo.Banos AS b WITH (NOLOCK) ON b.Id = h.BanoId
    WHERE p.CampamentoId = @CampamentoId
      AND (@PabellonId = 0 OR p.Id = @PabellonId)
      AND (@ModuloId = 0 OR m.Id = @ModuloId)
      AND (@TipoInstalacionId = 0 OR h.TipoInstalacionId = @TipoInstalacionId)
      AND (@Estado IS NULL OR @Estado = '' OR h.Estado = @Estado)
      AND (@SoloActivas = 0 OR h.IsActive = 1)
      AND (@SoloReservables IS NULL OR h.IsReservable = @SoloReservables)
      AND (@HabitacionId = 0 OR h.Id = @HabitacionId)
    ORDER BY p.Nombre, m.Nombre, h.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_habitacion_obtener
    @Id int,
    @CampamentoId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Detalle de habitación para edición.
    --======================================================

SET NOCOUNT ON;

    SELECT
        h.Id,
        h.Nombre,
        h.ModuloId,
        h.Sexo,
        h.Estado,
        ISNULL((SELECT COUNT(*) FROM dbo.Camas AS c WHERE c.HabitacionId = h.Id AND c.IsActive = 1), h.CantidadCamas) AS CantidadCamas,
        CAST(ISNULL(h.IsActive, 0) AS bit) AS IsActive,
        CAST(ISNULL(h.IsReservable, 0) AS bit) AS IsReservable,
        h.TipoInstalacionId,
        h.StandardHabitacionId,
        h.ObservacionBloqueo,
        h.M2,
        h.BanoId,
        ISNULL(h.PersonasCantidadMin, 0) AS PersonasCantidadMin,
        ISNULL(h.PersonasCantidadMax, 0) AS PersonasCantidadMax,
        ISNULL(h.MinutosMin, 0) AS MinutosMin,
        ISNULL(h.MinutosMax, 0) AS MinutosMax,
        ISNULL(h.TiempoHigienizacionMin, h.MinutosMin) AS TiempoHigienizacionMin,
        CAST(ISNULL(h.TieneServiciosAlimentacion, 0) AS bit) AS TieneServiciosAlimentacion,
        CAST(ISNULL(h.PermiteUsoPorTurnos, CASE WHEN h.MinutosMin > 0 OR h.MinutosMax > 0 THEN 1 ELSE 0 END) AS bit) AS PermiteUsoPorTurnos,
        CAST(ISNULL(h.PermiteUsoExclusivo, CASE WHEN h.PersonasCantidadMax >= h.CantidadCamas AND h.CantidadCamas > 0 THEN 1 ELSE 0 END) AS bit) AS PermiteUsoExclusivo
    FROM dbo.Habitaciones AS h WITH (NOLOCK)
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    WHERE h.Id = @Id
      AND p.CampamentoId = @CampamentoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_habitacion_catalogo
    @CampamentoId int,
    @MineraId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Result sets de catálogo para formularios (pabellones, listas, baños, tipos).
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
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_habitacion_guardar
    @Id int,
    @ModuloId int,
    @Nombre varchar(100),
    @IsActive int,
    @CantidadCamas int,
    @Sexo varchar(20),
    @UsuarioId int,
    @TipoInstalacionId int,
    @StandardHabitacionId int,
    @Estado varchar(10),
    @ObservacionBloqueo varchar(500) = NULL,
    @M2 decimal(10, 2) = NULL,
    @IsReservable bit = 1,
    @PersonasCantidadMin int = 0,
    @PersonasCantidadMax int = 0,
    @MinutosMin int = 0,
    @MinutosMax int = 0,
    @TieneServiciosAlimentacion int = 0,
    @BanoId int = NULL,
    @PermiteUsoExclusivo bit = 0,
    @PermiteUsoPorTurnos bit = 0,
    @TiempoHigienizacionMin int = 0,
    @CampamentoId int,
    @IdResultado int OUTPUT,
    @Mensaje nvarchar(500) OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Alta y edición de habitación; legado HAB_* o guardado nativo.
    --======================================================

SET NOCOUNT ON;
    SET @Mensaje = NULL;
    SET @IdResultado = 0;

    IF @ModuloId > 0 AND NOT EXISTS (
        SELECT 1
        FROM dbo.Modulos AS m
        INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
        WHERE m.Id = @ModuloId AND p.CampamentoId = @CampamentoId)
    BEGIN
        SET @Mensaje = N'El módulo no pertenece al campamento de la sesión.';
        RETURN;
    END

    IF @BanoId IS NOT NULL AND @BanoId > 0
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM dbo.Banos
            WHERE Id = @BanoId AND CampamentoId = @CampamentoId
              AND (IsActive = 1 OR Estado = N'ACTIVO'))
        BEGIN
            SET @Mensaje = N'El baño seleccionado no es válido para el campamento.';
            RETURN;
        END
        IF EXISTS (
            SELECT 1 FROM dbo.Banos AS b
            WHERE b.Id = @BanoId
              AND NOT (
                    b.TipoSexo IN ('MIXTO', 'M', 'F')
                    OR (b.TipoSexo IN ('HOMBRE', 'M') AND @Sexo IN ('M', 'MIXTO', 'HOMBRE'))
                    OR (b.TipoSexo IN ('MUJER', 'F') AND @Sexo IN ('F', 'MIXTO', 'MUJER'))
                  ))
        BEGIN
            SET @Mensaje = N'RN-02: El sexo de la habitación no es compatible con el baño.';
            RETURN;
        END
    END

    IF @ModuloId > 0 AND @Id > 0 AND EXISTS (
        SELECT 1 FROM dbo.Habitaciones AS h
        INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
        INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
        WHERE h.Id = @Id AND p.CampamentoId <> @CampamentoId)
    BEGIN
        SET @Mensaje = N'La habitación no pertenece al campamento de la sesión.';
        RETURN;
    END

    IF @Id > 0 AND (@PersonasCantidadMax <= 0 OR @PersonasCantidadMin <= 0)
    BEGIN
        SELECT
            @PersonasCantidadMin = CASE
                WHEN @PersonasCantidadMin > 0 THEN @PersonasCantidadMin
                ELSE ISNULL(NULLIF(h.PersonasCantidadMin, 0), 1) END,
            @PersonasCantidadMax = CASE
                WHEN @PersonasCantidadMax > 0 THEN @PersonasCantidadMax
                ELSE ISNULL(NULLIF(h.PersonasCantidadMax, 0), @CantidadCamas) END
        FROM dbo.Habitaciones AS h WITH (NOLOCK)
        WHERE h.Id = @Id;
    END

    IF @PersonasCantidadMin <= 0 SET @PersonasCantidadMin = 1;
    IF @PersonasCantidadMax <= 0 SET @PersonasCantidadMax = @CantidadCamas;

    IF @CantidadCamas < 1
    BEGIN
        SET @Mensaje = N'La habitación debe tener al menos 1 cama.';
        RETURN;
    END

    IF @Id > 0
       AND @IsReservable = 0
       AND EXISTS (
           SELECT 1 FROM dbo.Habitaciones WITH (NOLOCK)
           WHERE Id = @Id AND IsActive = 1 AND IsReservable = 1)
       AND EXISTS (
           SELECT 1 FROM dbo.ReservasInstalacion WITH (NOLOCK)
           WHERE ubicacionId = @Id
             AND fecHorInicio >= DATEADD(hour, DATEDIFF(hour, 0, SYSDATETIME()), 0))
    BEGIN
        SET @Mensaje = N'Ubicacion se encuentra con reservas activas';
        RETURN;
    END

    DECLARE @UsarNativo bit = 1;
    DECLARE @GuardadoLegado bit = 0;
    DECLARE @ret int;
    DECLARE @ErrLegado nvarchar(500);

    IF OBJECT_ID(N'dbo.HAB_GuardaHabitacion', N'P') IS NULL
        SET @UsarNativo = 1;

    IF @UsarNativo = 0 AND EXISTS (
        SELECT 1
        FROM sys.synonyms AS s
        WHERE s.name = N'HabitacionFiles'
          AND SCHEMA_NAME(s.schema_id) = N'files'
          AND OBJECT_ID(s.base_object_name) IS NULL)
        SET @UsarNativo = 1;

    IF @UsarNativo = 0
    BEGIN
        BEGIN TRY
            EXEC dbo.HAB_ValidaHabitacion
                @id = @Id,
                @nombre = @Nombre,
                @isActive = @IsActive,
                @cantidadCamas = @CantidadCamas,
                @sexo = @Sexo,
                @usuario = @UsuarioId,
                @tipoInstalacion = @TipoInstalacionId,
                @standarHabitacion = @StandardHabitacionId,
                @estado = @Estado,
                @observacionBloqueo = @ObservacionBloqueo,
                @m2 = @M2,
                @isReservable = @IsReservable,
                @PersonasCantidadMin = @PersonasCantidadMin,
                @PersonasCantidadMax = @PersonasCantidadMax,
                @MinutosMin = @MinutosMin,
                @MinutosMax = @MinutosMax,
                @TieneServiciosAlimentacion = @TieneServiciosAlimentacion,
                @FechaReservaDesde = NULL,
                @FechaReservaHasta = NULL,
                @ReservaDiasAnticipacion = 0,
                @ReservaDiasCancelacion = 0,
                @ReservaTotalEnMes = 0,
                @FileId = 0,
                @FileName = NULL,
                @ContentType = NULL,
                @XMLSemana = NULL,
                @PlantillaUbicacionId = @ModuloId,
                @IsExistReservaConfirm = 0,
                @ImageId = 0,
                @ImageName = NULL,
                @ImageContentType = NULL;

            EXEC dbo.HAB_GuardaHabitacion
                @id = @Id,
                @nombre = @Nombre,
                @isActive = @IsActive,
                @cantidadCamas = @CantidadCamas,
                @sexo = @Sexo,
                @usuario = @UsuarioId,
                @tipoInstalacion = @TipoInstalacionId,
                @standarHabitacion = @StandardHabitacionId,
                @estado = @Estado,
                @observacionBloqueo = @ObservacionBloqueo,
                @m2 = @M2,
                @isReservable = @IsReservable,
                @PersonasCantidadMin = @PersonasCantidadMin,
                @PersonasCantidadMax = @PersonasCantidadMax,
                @MinutosMin = @MinutosMin,
                @MinutosMax = @MinutosMax,
                @TieneServiciosAlimentacion = @TieneServiciosAlimentacion,
                @FechaReservaDesde = NULL,
                @FechaReservaHasta = NULL,
                @ReservaDiasAnticipacion = 0,
                @ReservaDiasCancelacion = 0,
                @ReservaTotalEnMes = 0,
                @FileId = 0,
                @FileName = NULL,
                @ContentType = NULL,
                @Archivo = NULL,
                @ImageId = 0,
                @ImageName = NULL,
                @ImageContentType = NULL,
                @Image = NULL,
                @XMLSemana = NULL,
                @PlantillaUbicacionId = @ModuloId,
                @IsExistReservaConfirm = 0,
                @ret = @ret OUTPUT;

            IF ISNULL(@ret, 0) > 0
            BEGIN
                SET @IdResultado = @ret;
                SET @GuardadoLegado = 1;
            END
        END TRY
        BEGIN CATCH
            SET @ErrLegado = LEFT(ERROR_MESSAGE(), 500);
        END CATCH

        WHILE @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
    END

    IF @GuardadoLegado = 0
    BEGIN
        SET @UsarNativo = 1;

        DECLARE @EstadoTabla int = 8;
        DECLARE @SexoTabla int = 11;
        DECLARE @SexoLista varchar(1);

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Listas WITH (NOLOCK)
            WHERE IdTabla = @EstadoTabla AND Tipo = N'Detalle' AND Valor1 = @Estado)
        BEGIN
            SET @Mensaje = N'El estado no existe en dbo.Listas (IdTabla 8).';
            RETURN;
        END

        DECLARE @SexoNorm varchar(10) = UPPER(LTRIM(RTRIM(ISNULL(@Sexo, N''))));
        IF @SexoNorm IN (N'HOMBRE', N'MASCULINO')
            SET @SexoNorm = N'M';
        IF @SexoNorm IN (N'MUJER', N'FEMENINO')
            SET @SexoNorm = N'F';

        IF LEN(@SexoNorm) = 1 AND @SexoNorm IN (N'M', N'F', N'O')
            SET @SexoLista = @SexoNorm;
        ELSE IF @SexoNorm = N'MIXTO'
        BEGIN
            SELECT TOP 1 @SexoLista = CAST(l.Valor1 AS varchar(1))
            FROM dbo.Listas AS l WITH (NOLOCK)
            WHERE l.IdTabla = @SexoTabla AND l.Tipo = N'Detalle' AND l.Valor1 IN (N'O', N'M', N'F', N'MIXTO')
            ORDER BY CASE l.Valor1 WHEN N'O' THEN 0 WHEN N'MIXTO' THEN 1 ELSE 2 END;
        END
        ELSE
            SET @SexoLista = LEFT(@SexoNorm, 1);

        IF @SexoLista IS NULL OR NOT EXISTS (
            SELECT 1 FROM dbo.Listas WITH (NOLOCK)
            WHERE IdTabla = @SexoTabla AND Tipo = N'Detalle' AND Valor1 = @SexoLista)
        BEGIN
            SET @Mensaje = N'El sexo no existe en dbo.Listas (IdTabla 11).';
            RETURN;
        END

        SET @MinutosMin = CASE
            WHEN @PermiteUsoPorTurnos = 1 AND @TiempoHigienizacionMin > 0 THEN @TiempoHigienizacionMin
            ELSE @MinutosMin END;
        SET @PersonasCantidadMax = CASE
            WHEN @PermiteUsoExclusivo = 1 THEN @CantidadCamas
            ELSE @PersonasCantidadMax END;

        BEGIN TRY
            BEGIN TRANSACTION;

            IF @Id > 0
            BEGIN
                UPDATE dbo.Habitaciones
                SET ModuloId = CASE WHEN @ModuloId > 0 THEN @ModuloId ELSE ModuloId END,
                    Nombre = @Nombre,
                    IsActive = @IsActive,
                    CantidadCamas = @CantidadCamas,
                    SexoTabla = @SexoTabla,
                    Sexo = @SexoLista,
                    TipoInstalacionId = @TipoInstalacionId,
                    StandardHabitacionId = NULLIF(@StandardHabitacionId, 0),
                    Estado = @Estado,
                    ObservacionBloqueo = @ObservacionBloqueo,
                    M2 = @M2,
                    IsReservable = @IsReservable,
                    PersonasCantidadMin = @PersonasCantidadMin,
                    PersonasCantidadMax = @PersonasCantidadMax,
                    MinutosMin = @MinutosMin,
                    MinutosMax = @MinutosMax,
                    TieneServiciosAlimentacion = @TieneServiciosAlimentacion,
                    BanoId = @BanoId,
                    PermiteUsoExclusivo = @PermiteUsoExclusivo,
                    PermiteUsoPorTurnos = @PermiteUsoPorTurnos,
                    TiempoHigienizacionMin = NULLIF(@TiempoHigienizacionMin, 0),
                    FechaActualizacion = SYSUTCDATETIME(),
                    UsuarioActualizacionId = @UsuarioId
                WHERE Id = @Id;

                IF @@ROWCOUNT = 0
                BEGIN
                    ROLLBACK TRANSACTION;
                    SET @Mensaje = N'La habitación no existe.';
                    RETURN;
                END

                SET @IdResultado = @Id;
            END
            ELSE
            BEGIN
                IF @ModuloId <= 0
                BEGIN
                    ROLLBACK TRANSACTION;
                    SET @Mensaje = N'Seleccione un módulo.';
                    RETURN;
                END

                INSERT INTO dbo.Habitaciones (
                    ModuloId, Nombre, IsActive, CantidadCamas, SexoTabla, Sexo,
                    TipoInstalacionId, StandardHabitacionId, EstadoTabla, Estado,
                    ObservacionBloqueo, M2, IsReservable,
                    PersonasCantidadMin, PersonasCantidadMax,
                    MinutosMin, MinutosMax, TieneServiciosAlimentacion,
                    BanoId, PermiteUsoExclusivo, PermiteUsoPorTurnos, TiempoHigienizacionMin,
                    FechaCreacion, UsuarioCreacionId)
                VALUES (
                    @ModuloId, @Nombre, @IsActive, @CantidadCamas, @SexoTabla, @SexoLista,
                    @TipoInstalacionId, NULLIF(@StandardHabitacionId, 0), @EstadoTabla, @Estado,
                    @ObservacionBloqueo, @M2, @IsReservable,
                    @PersonasCantidadMin, @PersonasCantidadMax,
                    @MinutosMin, @MinutosMax, @TieneServiciosAlimentacion,
                    @BanoId, @PermiteUsoExclusivo, @PermiteUsoPorTurnos, NULLIF(@TiempoHigienizacionMin, 0),
                    SYSUTCDATETIME(), @UsuarioId);

                SET @IdResultado = SCOPE_IDENTITY();
            END

            COMMIT TRANSACTION;
        END TRY
        BEGIN CATCH
            WHILE @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            SET @Mensaje = LEFT(
                COALESCE(@ErrLegado + N' ', N'') + ERROR_MESSAGE(),
                500);
            RETURN;
        END CATCH
    END
    ELSE
    BEGIN
        UPDATE dbo.Habitaciones
        SET ModuloId = CASE WHEN @ModuloId > 0 THEN @ModuloId ELSE ModuloId END,
            BanoId = @BanoId,
            PermiteUsoExclusivo = @PermiteUsoExclusivo,
            PermiteUsoPorTurnos = @PermiteUsoPorTurnos,
            TiempoHigienizacionMin = NULLIF(@TiempoHigienizacionMin, 0),
            MinutosMin = CASE WHEN @PermiteUsoPorTurnos = 1 AND @TiempoHigienizacionMin > 0 THEN @TiempoHigienizacionMin ELSE MinutosMin END,
            PersonasCantidadMax = CASE WHEN @PermiteUsoExclusivo = 1 THEN @CantidadCamas ELSE PersonasCantidadMax END
        WHERE Id = @IdResultado;
    END

    IF @IdResultado <= 0
    BEGIN
        SET @Mensaje = COALESCE(@ErrLegado, N'No se pudo guardar la habitación.');
        RETURN;
    END

    BEGIN TRY
        EXEC dbo.he_sp_habitacion_sincronizar_camas
            @HabitacionId = @IdResultado,
            @CantidadObjetivo = @CantidadCamas,
            @CampamentoId = @CampamentoId,
            @UsuarioId = @UsuarioId;
    END TRY
    BEGIN CATCH
        SET @Mensaje = LEFT(ERROR_MESSAGE(), 500);
        RETURN;
    END CATCH
END
GO

GO
