/*
    Reserva. La búsqueda y el alta reproducen el repositorio legado:
    el listado era LINQ y el alta grababa estado P, origen R, tablas 6, 5 y 10.
    Check-in y check-out envuelven RES_RealizaCheckX (operación I u O).
    Los nombres de columna salen del modelo EF del legado. Si HOTELDB difiere, se ajusta este script.
*/

CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Buscar
    @CampamentoId int,
    @Id int = 0,
    @AreaId int = 0,
    @Origen varchar(20) = '0',
    @SolicitudId int = 0,
    @Estado varchar(2) = '0',
    @FechaReserva date = NULL,
    @FechaLlegada date = NULL,
    @FechaSalida date = NULL,
    @CheckIn date = NULL,
    @CheckOut date = NULL,
    @PasajeroId int = 0,
    @HabitacionId int = 0,
    @TieneAgua varchar(2) = '0'
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.Id,
        ISNULL(e.Nombre, '') + ' - ' + ISNULL(a.Nombre, '') AS GrupoEmpresa,
        LTRIM(RTRIM(ISNULL(u.Nombre, '') + ' ' + ISNULL(u.Apellidos, ''))) AS Pasajero,
        ISNULL(h.Nombre, '') AS Habitacion,
        ISNULL(le.Descripcion, r.Estado) AS Estado,
        r.Estado AS EstadoNumero,
        CONVERT(varchar(10), r.FechaLlegada, 103) AS Llegada,
        CASE
            WHEN r.FechaSalida IS NULL THEN '<Sin Asignar>'
            ELSE CONVERT(varchar(10), r.FechaSalida, 103)
        END AS Salida,
        CASE WHEN r.CheckIn IS NULL THEN NULL ELSE CONVERT(varchar(10), r.CheckIn, 103) END AS CheckIn,
        CASE WHEN r.CheckOut IS NULL THEN NULL ELSE CONVERT(varchar(10), r.CheckOut, 103) END AS CheckOut
    FROM dbo.Reservas AS r
    LEFT JOIN dbo.AreasEmpresa AS a ON a.Id = r.AreaEmpresaContratistaId
    LEFT JOIN dbo.Empresas AS e ON e.Id = a.EmpresaId
    LEFT JOIN dbo.Usuarios AS u ON u.Id = r.PasajeroId
    LEFT JOIN dbo.Habitaciones AS h ON h.Id = r.HabitacionId
    LEFT JOIN dbo.Listas AS le ON le.IdTabla = r.EstadoTabla AND le.Valor1 = r.Estado
    WHERE (@CampamentoId = 0 OR r.CampamentoId = @CampamentoId)
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
      AND (@FechaReserva IS NULL OR CONVERT(date, r.FechaReserva) = @FechaReserva)
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
    ORDER BY r.FechaLlegada, h.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserva_ListarCatalogo
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.Id, e.Nombre + ' - ' + a.Nombre AS Etiqueta
    FROM dbo.EmpresasMandanteAreasEmpresaContratista AS rel
    INNER JOIN dbo.AreasEmpresa AS a ON a.Id = rel.AreaEmpresaContratistaId
    INNER JOIN dbo.Empresas AS e ON e.Id = a.EmpresaId
    WHERE rel.EmpresaMandanteId = @EmpresaMandanteId
      AND a.IsActive = 1
    ORDER BY Etiqueta;

    SELECT Valor1 AS Id, Descripcion AS Etiqueta
    FROM dbo.Listas
    WHERE IdTabla = 5
    ORDER BY Orden, Descripcion;

    SELECT DISTINCT u.Id, LTRIM(RTRIM(u.Nombre + ' ' + u.Apellidos)) AS Etiqueta
    FROM dbo.Reservas AS r
    INNER JOIN dbo.Usuarios AS u ON u.Id = r.PasajeroId
    WHERE @CampamentoId = 0 OR r.CampamentoId = @CampamentoId
    ORDER BY Etiqueta;

    SELECT h.Id, h.Nombre AS Etiqueta
    FROM dbo.Habitaciones AS h
    INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
    WHERE h.IsActive = 1
      AND (@CampamentoId = 0 OR p.CampamentoId = @CampamentoId)
    ORDER BY h.Nombre;

    SELECT Id, Nombre, TipoTurno
    FROM dbo.Turnos
    WHERE EmpresaMandanteId = @EmpresaMandanteId
      AND IsActive = 1
    ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Crear
    @EmpresaMandanteId int,
    @CampamentoId int,
    @AreaId int,
    @FechaReserva date,
    @FechaLlegada date,
    @FechaSalida date = NULL,
    @Observacion nvarchar(200) = NULL,
    @TurnoId int,
    @UsuarioId int,
    @Id int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TipoTurno varchar(20);
    SELECT @TipoTurno = TipoTurno
    FROM dbo.Turnos
    WHERE Id = @TurnoId
      AND EmpresaMandanteId = @EmpresaMandanteId
      AND IsActive = 1;

    IF @TipoTurno IS NULL
    BEGIN
        RAISERROR('El turno no corresponde a la empresa de la sesión.', 16, 1);
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
        NULL,
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

CREATE OR ALTER PROCEDURE dbo.sp_Reserva_RegistrarMovimiento
    @ReservaId int,
    @Fecha datetime2,
    @UsuarioId int,
    @Operacion char(1),
    @Ret int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.RES_RealizaCheckX
        @reservaId = @ReservaId,
        @fecha = @Fecha,
        @usuario = @UsuarioId,
        @operacion = @Operacion,
        @ret = @Ret OUTPUT;
END
GO
