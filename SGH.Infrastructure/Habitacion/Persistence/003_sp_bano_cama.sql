/* Baños y camas — CRUD CU-HAB-01 */

CREATE OR ALTER PROCEDURE dbo.sp_Bano_Listar
    @CampamentoId int,
    @SoloActivos int = 1
AS
BEGIN
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
    ORDER BY b.Ubicacion, b.CodigoNombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bano_Guardar
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

        /* RN-02 al cambiar sexo: habitaciones vinculadas deben ser compatibles */
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

CREATE OR ALTER PROCEDURE dbo.sp_Cama_ListarPorHabitacion
    @HabitacionId int,
    @CampamentoId int
AS
BEGIN
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
      AND c.IsActive = 1
    ORDER BY c.Orden, c.CodigoCama;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cama_Guardar
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

CREATE OR ALTER PROCEDURE dbo.sp_Habitacion_SincronizarCamas
    @HabitacionId int,
    @CantidadObjetivo int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
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