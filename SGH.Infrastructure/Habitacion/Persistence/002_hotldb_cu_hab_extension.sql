
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
