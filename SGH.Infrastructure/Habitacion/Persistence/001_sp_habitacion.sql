/* Habitaciones — CRUD alineado a HOTELDB (envoltorio + HAB_GuardaHabitacion / HAB_ValidaHabitacion). */

CREATE OR ALTER PROCEDURE dbo.sp_Habitacion_Listar
    @EmpresaMandanteId int,
    @CampamentoId int,
    @PabellonId int = 0,
    @ModuloId int = 0,
    @TipoInstalacionId int = 0,
    @Estado varchar(10) = NULL,
    @SoloActivas int = 1,
    @SoloReservables bit = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        h.Id,
        h.Nombre,
        h.Sexo,
        h.Estado,
        ISNULL((SELECT COUNT(*) FROM dbo.Camas AS c WITH (NOLOCK) WHERE c.HabitacionId = h.Id AND c.IsActive = 1), h.CantidadCamas) AS CantidadCamas,
        ISNULL((SELECT COUNT(*) FROM dbo.Camas AS c WITH (NOLOCK) WHERE c.HabitacionId = h.Id AND c.IsActive = 1 AND c.Habilitada = 1),
               CASE WHEN h.IsActive = 1 THEN h.CantidadCamas ELSE 0 END) AS CamasHabilitadas,
        h.IsActive,
        h.IsReservable,
        m.Id AS ModuloId,
        m.Nombre AS ModuloNombre,
        p.Id AS PabellonId,
        p.Nombre AS PabellonNombre,
        h.BanoId,
        b.CodigoNombre AS BanoNombre,
        ISNULL(h.PermiteUsoPorTurnos, CASE WHEN h.MinutosMin > 0 OR h.MinutosMax > 0 THEN 1 ELSE 0 END) AS PermiteUsoPorTurnos,
        ISNULL(h.PermiteUsoExclusivo, CASE WHEN h.PersonasCantidadMax >= h.CantidadCamas AND h.CantidadCamas > 0 THEN 1 ELSE 0 END) AS PermiteUsoExclusivo
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
    ORDER BY p.Nombre, m.Nombre, h.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Habitacion_Obtener
    @Id int,
    @CampamentoId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        h.Id,
        h.Nombre,
        h.ModuloId,
        h.Sexo,
        h.Estado,
        ISNULL((SELECT COUNT(*) FROM dbo.Camas AS c WHERE c.HabitacionId = h.Id AND c.IsActive = 1), h.CantidadCamas) AS CantidadCamas,
        h.IsActive,
        h.IsReservable,
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
        ISNULL(h.TieneServiciosAlimentacion, 0) AS TieneServiciosAlimentacion,
        ISNULL(h.PermiteUsoPorTurnos, CAST(CASE WHEN h.MinutosMin > 0 OR h.MinutosMax > 0 THEN 1 ELSE 0 END AS bit)) AS PermiteUsoPorTurnos,
        ISNULL(h.PermiteUsoExclusivo, CAST(CASE WHEN h.PersonasCantidadMax >= h.CantidadCamas AND h.CantidadCamas > 0 THEN 1 ELSE 0 END AS bit)) AS PermiteUsoExclusivo
    FROM dbo.Habitaciones AS h WITH (NOLOCK)
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    WHERE h.Id = @Id
      AND p.CampamentoId = @CampamentoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Habitacion_Catalogo
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
    WHERE b.CampamentoId = @CampamentoId AND b.IsActive = 1 AND b.Estado = 'ACTIVO'
    ORDER BY b.Ubicacion, b.CodigoNombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Habitacion_Guardar
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
        IF NOT EXISTS (SELECT 1 FROM dbo.Banos WHERE Id = @BanoId AND CampamentoId = @CampamentoId AND IsActive = 1)
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

  DECLARE @ret int;
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

    IF ISNULL(@ret, 0) <= 0
    BEGIN
        SET @Mensaje = N'No se pudo guardar la habitación (HAB_GuardaHabitacion).';
        RETURN;
    END

    SET @IdResultado = @ret;

    UPDATE dbo.Habitaciones
    SET ModuloId = CASE WHEN @ModuloId > 0 THEN @ModuloId ELSE ModuloId END,
        BanoId = @BanoId,
        PermiteUsoExclusivo = @PermiteUsoExclusivo,
        PermiteUsoPorTurnos = @PermiteUsoPorTurnos,
        TiempoHigienizacionMin = NULLIF(@TiempoHigienizacionMin, 0),
        MinutosMin = CASE WHEN @PermiteUsoPorTurnos = 1 AND @TiempoHigienizacionMin > 0 THEN @TiempoHigienizacionMin ELSE MinutosMin END,
        PersonasCantidadMax = CASE WHEN @PermiteUsoExclusivo = 1 THEN @CantidadCamas ELSE PersonasCantidadMax END
    WHERE Id = @IdResultado;

    EXEC dbo.sp_Habitacion_SincronizarCamas
        @HabitacionId = @IdResultado,
        @CantidadObjetivo = @CantidadCamas,
        @CampamentoId = @CampamentoId,
        @UsuarioId = @UsuarioId;
END
GO