
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
