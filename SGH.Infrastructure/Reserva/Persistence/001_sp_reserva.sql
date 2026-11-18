/*
    Reserva. La búsqueda y el alta reproducen el repositorio legado:
    el listado era LINQ y el alta grababa estado P, origen R, tablas 6, 5 y 10.
    Check-in y check-out envuelven RES_RealizaCheckX (operación I u O).
    Los nombres de columna salen del modelo EF del legado. Si HOTELDB difiere, se ajusta este script.
*/

CREATE OR ALTER PROCEDURE dbo.he_sp_reserva_buscar
    @CampamentoId int,
    @Id int = 0,
    @AreaId int = 0,
    @Origen varchar(20) = '0',
    @SolicitudId int = 0,
    @Estado varchar(2) = '0',
    @FechaReserva date = NULL,
    @FechaReservaDesde date = NULL,
    @FechaReservaHasta date = NULL,
    @FechaLlegada date = NULL,
    @FechaSalida date = NULL,
    @CheckIn date = NULL,
    @CheckOut date = NULL,
    @PasajeroId int = 0,
    @HabitacionId int = 0,
    @TieneAgua varchar(2) = '0',
    @MaxFilas int = 5000
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DesdeEfectivo date = @FechaReservaDesde;
    DECLARE @HastaEfectivo date = @FechaReservaHasta;

    IF @DesdeEfectivo IS NULL AND @HastaEfectivo IS NULL AND @FechaReserva IS NULL
       AND @FechaLlegada IS NULL AND @FechaSalida IS NULL AND @CheckIn IS NULL AND @CheckOut IS NULL
    BEGIN
        SET @HastaEfectivo = CAST(GETDATE() AS date);
        SET @DesdeEfectivo = DATEADD(day, -30, @HastaEfectivo);
    END

    SELECT TOP (@MaxFilas)
        r.Id,
        r.AreaEmpresaContratistaId AS AreaId,
        r.PasajeroId,
        r.HabitacionId,
        r.TurnoId,
        RTRIM(ISNULL(e.Nombre, '')) + ' - ' + RTRIM(ISNULL(a.Nombre, '')) AS GrupoEmpresa,
        LTRIM(RTRIM(ISNULL(u.Nombre, '') + ' ' + ISNULL(u.Apellidos, ''))) AS Pasajero,
        RTRIM(ISNULL(h.Nombre, '')) AS Habitacion,
        RTRIM(ISNULL(le.Descripcion, r.Estado)) AS Estado,
        RTRIM(r.Estado) AS EstadoNumero,
        CONVERT(varchar(10), r.FechaReserva, 103) AS FechaReserva,
        CONVERT(varchar(10), r.FechaLlegada, 103) + ' ' + CONVERT(varchar(5), r.FechaLlegada, 108) AS Llegada,
        CASE
            WHEN r.FechaSalida IS NULL THEN '<Sin Asignar>'
            ELSE CONVERT(varchar(10), r.FechaSalida, 103) + ' ' + CONVERT(varchar(5), r.FechaSalida, 108)
        END AS Salida,
        CASE WHEN r.CheckIn IS NULL THEN NULL ELSE CONVERT(varchar(10), r.CheckIn, 103) + ' ' + CONVERT(varchar(5), r.CheckIn, 108) END AS CheckIn,
        CASE WHEN r.CheckOut IS NULL THEN NULL ELSE CONVERT(varchar(10), r.CheckOut, 103) + ' ' + CONVERT(varchar(5), r.CheckOut, 108) END AS CheckOut
    FROM dbo.Reservas AS r WITH (NOLOCK)
    LEFT JOIN dbo.AreasEmpresa AS a WITH (NOLOCK) ON a.Id = r.AreaEmpresaContratistaId
    LEFT JOIN dbo.Empresas AS e WITH (NOLOCK) ON e.Id = a.EmpresaId
    LEFT JOIN dbo.Usuarios AS u WITH (NOLOCK) ON u.Id = r.PasajeroId
    LEFT JOIN dbo.Habitaciones AS h WITH (NOLOCK) ON h.Id = r.HabitacionId
    LEFT JOIN dbo.Listas AS le WITH (NOLOCK) ON le.IdTabla = r.EstadoTabla AND le.Valor1 = r.Estado
    WHERE r.CampamentoId = @CampamentoId
      AND (@Id = 0 OR r.Id = @Id)
      AND (@AreaId = 0 OR r.AreaEmpresaContratistaId = @AreaId)
      AND (@Origen = '0' OR r.OrigenReserva = @Origen)
      AND (@SolicitudId = 0 OR r.SolicitudAlojamientoId = @SolicitudId)
      AND (
            @Estado = '0'
            OR (@Estado = 'I' AND r.Estado = 'A' AND r.CheckIn IS NOT NULL AND r.CheckOut IS NULL)
            OR (@Estado = 'O' AND r.Estado = 'A' AND r.CheckIn IS NOT NULL AND r.CheckOut IS NOT NULL)
            OR (@Estado = 'A' AND r.Estado = 'A' AND r.CheckIn IS NULL AND r.CheckOut IS NULL)
            OR (@Estado = 'N' AND r.Estado = 'N')
            OR (@Estado = 'F' AND r.Estado = 'F')
          )
      AND (
            (@DesdeEfectivo IS NOT NULL AND r.FechaReserva >= @DesdeEfectivo
                AND (@HastaEfectivo IS NULL OR r.FechaReserva < DATEADD(day, 1, @HastaEfectivo)))
            OR (@DesdeEfectivo IS NULL AND @FechaReserva IS NOT NULL
                AND r.FechaReserva >= @FechaReserva
                AND r.FechaReserva < DATEADD(day, 1, @FechaReserva))
          )
      AND (@FechaLlegada IS NULL OR CONVERT(date, r.FechaLlegada) = @FechaLlegada)
      AND (@FechaSalida IS NULL OR CONVERT(date, r.FechaSalida) = @FechaSalida)
      AND (@CheckIn IS NULL OR CONVERT(date, r.CheckIn) = @CheckIn)
      AND (@CheckOut IS NULL OR CONVERT(date, r.CheckOut) = @CheckOut)
      AND (@PasajeroId = 0 OR r.PasajeroId = @PasajeroId)
      AND (@HabitacionId = 0 OR r.HabitacionId = @HabitacionId)
      AND (
            @TieneAgua = '0'
            OR (@TieneAgua = '1' AND r.TieneAgua = 1)
            OR (@TieneAgua <> '0' AND @TieneAgua <> '1' AND r.TieneAgua = 0)
          )
    ORDER BY r.Id DESC
    OPTION (RECOMPILE);
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_reserva_listar_catalogo
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.Id, e.Nombre + ' - ' + a.Nombre AS Etiqueta
    FROM dbo.EmpresasMandanteAreasEmpresaContratista AS rel WITH (NOLOCK)
    INNER JOIN dbo.AreasEmpresa AS a WITH (NOLOCK) ON a.Id = rel.AreaEmpresaContratistaId
    INNER JOIN dbo.Empresas AS e WITH (NOLOCK) ON e.Id = a.EmpresaId
    WHERE rel.EmpresaMandanteId = @EmpresaMandanteId
      AND a.IsActive = 1
    ORDER BY Etiqueta;

    SELECT Valor1 AS Id, Descripcion AS Etiqueta
    FROM dbo.Listas WITH (NOLOCK)
    WHERE IdTabla = 5
    ORDER BY Orden, Descripcion;

    SELECT u.Id, LTRIM(RTRIM(ISNULL(u.Rut, '') + ' - ' + u.Nombre + ' ' + u.Apellidos)) AS Etiqueta
    FROM dbo.Usuarios AS u WITH (NOLOCK)
    WHERE u.EmpresaMandanteId = @EmpresaMandanteId
      AND u.IsActive = 1
    ORDER BY Etiqueta;

    SELECT h.Id, h.Nombre AS Etiqueta
    FROM dbo.Habitaciones AS h WITH (NOLOCK)
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    WHERE h.IsActive = 1
      AND h.IsReservable = 1
      AND (@CampamentoId = 0 OR p.CampamentoId = @CampamentoId)
    ORDER BY h.Nombre;

    SELECT Id, Nombre, TipoTurno
    FROM dbo.Turnos WITH (NOLOCK)
    WHERE EmpresaMandanteId = @EmpresaMandanteId
      AND IsActive = 1
    ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_reserva_crear
    @EmpresaMandanteId int,
    @CampamentoId int,
    @AreaId int,
    @FechaReserva datetime2,
    @FechaLlegada datetime2,
    @FechaSalida datetime2 = NULL,
    @Observacion nvarchar(200) = NULL,
    @TurnoId int,
    @UsuarioId int,
    @Id int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TipoTurno varchar(20);
    SELECT @TipoTurno = TipoTurno
    FROM dbo.Turnos WITH (NOLOCK)
    WHERE Id = @TurnoId
      AND EmpresaMandanteId = @EmpresaMandanteId
      AND IsActive = 1;

    IF @TipoTurno IS NULL
    BEGIN
        RAISERROR('El turno no corresponde a la empresa de la sesión.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.EmpresasMandanteAreasEmpresaContratista WITH (NOLOCK)
        WHERE EmpresaMandanteId = @EmpresaMandanteId
          AND AreaEmpresaContratistaId = @AreaId)
    BEGIN
        RAISERROR('El área no corresponde a la empresa de la sesión.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Reservas (
        EmpresaMandanteId,
        CampamentoId,
        AreaEmpresaContratistaId,
        OrigenReservaTabla,
        OrigenReserva,
        SolicitudAlojamientoId,
        FechaReserva,
        FechaLlegada,
        FechaSalida,
        Observacion,
        EstadoTabla,
        Estado,
        PasajeroId,
        TurnoId,
        TipoTurnoTabla,
        TipoTurno,
        TieneAgua,
        FechaCreacion,
        UsuarioCreacionId,
        FechaActualizacion)
    VALUES (
        @EmpresaMandanteId,
        @CampamentoId,
        @AreaId,
        5,
        'R',
        NULL,
        @FechaReserva,
        @FechaLlegada,
        @FechaSalida,
        @Observacion,
        6,
        'P',
        @UsuarioId,
        @TurnoId,
        10,
        @TipoTurno,
        0,
        GETDATE(),
        @UsuarioId,
        GETDATE());

    SET @Id = CAST(SCOPE_IDENTITY() AS int);
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_reserva_esta_en_campamento
    @ReservaId int,
    @CampamentoId int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM dbo.Reservas WITH (NOLOCK)
        WHERE Id = @ReservaId
          AND CampamentoId = @CampamentoId
    ) THEN 1 ELSE 0 END AS bit) AS Pertenece;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_reserva_actualizar
    @ReservaId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @AreaId int,
    @FechaReserva datetime2,
    @FechaLlegada datetime2,
    @FechaSalida datetime2 = NULL,
    @Observacion nvarchar(200) = NULL,
    @TurnoId int,
    @PasajeroId int = NULL,
    @HabitacionId int = NULL,
    @Estado char(1),
    @TieneAgua bit,
    @UsuarioId int
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Reservas WITH (NOLOCK) WHERE Id = @ReservaId AND CampamentoId = @CampamentoId)
        THROW 51020, 'La reserva no corresponde al campamento.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Turnos WITH (NOLOCK) WHERE Id = @TurnoId AND EmpresaMandanteId = @EmpresaMandanteId AND IsActive = 1)
        THROW 51021, 'El turno no corresponde a la empresa.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.EmpresasMandanteAreasEmpresaContratista WITH (NOLOCK)
                   WHERE EmpresaMandanteId = @EmpresaMandanteId AND AreaEmpresaContratistaId = @AreaId)
        THROW 51022, 'El área no corresponde a la empresa.', 1;
    IF @FechaSalida IS NOT NULL AND @FechaSalida < @FechaLlegada
        THROW 51023, 'La fecha de salida no puede ser anterior a la llegada.', 1;
    IF @HabitacionId IS NOT NULL
       AND EXISTS (
           SELECT 1
           FROM dbo.Habitaciones AS h WITH (NOLOCK)
           WHERE h.Id = @HabitacionId
             AND h.CantidadCamas IS NOT NULL
             AND (
                 SELECT COUNT_BIG(1)
                 FROM dbo.Reservas AS r2 WITH (NOLOCK)
                 WHERE r2.HabitacionId = @HabitacionId
                   AND r2.Id <> @ReservaId
                   AND r2.Estado NOT IN ('N', 'F')
                   AND r2.FechaLlegada <= ISNULL(@FechaSalida, @FechaLlegada)
                   AND ISNULL(r2.FechaSalida, r2.FechaLlegada) >= @FechaLlegada
             ) >= h.CantidadCamas
       )
        THROW 51024, 'La habitacion no tiene camas disponibles para el periodo indicado.', 1;

    UPDATE dbo.Reservas
    SET EmpresaMandanteId = @EmpresaMandanteId,
        AreaEmpresaContratistaId = @AreaId,
        FechaReserva = @FechaReserva,
        FechaLlegada = @FechaLlegada,
        FechaSalida = @FechaSalida,
        Observacion = @Observacion,
        TurnoId = @TurnoId,
        PasajeroId = @PasajeroId,
        HabitacionId = @HabitacionId,
        Estado = @Estado,
        TieneAgua = @TieneAgua,
        UsuarioActualizacionId = @UsuarioId,
        FechaActualizacion = GETDATE()
    WHERE Id = @ReservaId AND CampamentoId = @CampamentoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_reserva_registrar_movimiento
    @ReservaId int,
    @Fecha datetime2,
    @UsuarioId int,
    @Operacion char(1),
    @CampamentoId int,
    @Ret int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Reservas WITH (NOLOCK)
        WHERE Id = @ReservaId
          AND CampamentoId = @CampamentoId)
    BEGIN
        RAISERROR('La reserva no corresponde al campamento de la sesión.', 16, 1);
        RETURN;
    END

    EXEC dbo.RES_RealizaCheckX
        @reservaId = @ReservaId,
        @fecha = @Fecha,
        @usuario = @UsuarioId,
        @operacion = @Operacion,
        @ret = @Ret OUTPUT;

    IF @Operacion = 'O' AND ISNULL(@Ret, 0) > 0
        EXEC dbo.he_sp_reserva_finalizar_checkout @ReservaId = @ReservaId, @UsuarioId = @UsuarioId;
END
GO
