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
        h.CantidadCamas,
        /* HOTELDB no tiene tabla Camas: capacidad operativa = CantidadCamas si activa */
        CASE WHEN h.IsActive = 1 THEN h.CantidadCamas ELSE 0 END AS CamasHabilitadas,
        h.IsActive,
        h.IsReservable,
        m.Id AS ModuloId,
        m.Nombre AS ModuloNombre,
        p.Id AS PabellonId,
        p.Nombre AS PabellonNombre,
        CAST(CASE WHEN h.MinutosMin > 0 OR h.MinutosMax > 0 THEN 1 ELSE 0 END AS bit) AS PermiteUsoPorTurnos,
        CAST(CASE WHEN h.PersonasCantidadMax >= h.CantidadCamas AND h.CantidadCamas > 0 THEN 1 ELSE 0 END AS bit) AS PermiteUsoExclusivo
    FROM dbo.Habitaciones AS h WITH (NOLOCK)
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
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
        h.CantidadCamas,
        h.IsActive,
        h.IsReservable,
        h.TipoInstalacionId,
        h.StandardHabitacionId,
        h.ObservacionBloqueo,
        h.M2,
        ISNULL(h.PersonasCantidadMin, 0) AS PersonasCantidadMin,
        ISNULL(h.PersonasCantidadMax, 0) AS PersonasCantidadMax,
        ISNULL(h.MinutosMin, 0) AS MinutosMin,
        ISNULL(h.MinutosMax, 0) AS MinutosMax,
        ISNULL(h.TieneServiciosAlimentacion, 0) AS TieneServiciosAlimentacion,
        CAST(CASE WHEN h.MinutosMin > 0 OR h.MinutosMax > 0 THEN 1 ELSE 0 END AS bit) AS PermiteUsoPorTurnos,
        CAST(CASE WHEN h.PersonasCantidadMax >= h.CantidadCamas AND h.CantidadCamas > 0 THEN 1 ELSE 0 END AS bit) AS PermiteUsoExclusivo
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

    IF @ModuloId > 0
        UPDATE dbo.Habitaciones SET ModuloId = @ModuloId WHERE Id = @IdResultado;
END
GO