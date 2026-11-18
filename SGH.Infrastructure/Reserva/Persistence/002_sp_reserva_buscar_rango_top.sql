/*
    Incremental QA — ejecutar después de 001_sp_reserva.sql si ese archivo ya se aplicó
    sin @MaxFilas, rango por defecto de 30 días ni TOP en sp_Reserva_Buscar.

    Instalación en frío: basta con 001_sp_reserva.sql (ya incluye este contenido).
*/
CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Buscar
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