CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_AsegurarPeriodo
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FechaCheckIn datetime, @FechaCheckOut datetime, @ConTurno bit, @TurnoId int;

    SELECT @FechaCheckIn = FechaCheckIn, @FechaCheckOut = FechaCheckOut,
           @ConTurno = ConSistemaTurno, @TurnoId = TurnoId
    FROM dbo.SolicitudesAlojamiento
    WHERE Id = @SolicitudId
      AND EmpresaMandanteId = @EmpresaMandanteId
      AND CampamentoId = @CampamentoId;

    IF @FechaCheckIn IS NULL OR @FechaCheckOut IS NOT NULL
        RETURN;

    DECLARE @Dias int = NULL;
    IF @ConTurno = 1 AND @TurnoId IS NOT NULL
        SELECT @Dias = DiasTrabajados FROM dbo.Turnos WHERE Id = @TurnoId;

    SET @FechaCheckOut = DATEADD(
        day,
        CASE WHEN @Dias IS NOT NULL AND @Dias > 0 THEN @Dias - 1 ELSE 0 END,
        CAST(CAST(@FechaCheckIn AS date) AS datetime));

    UPDATE dbo.SolicitudesAlojamiento
    SET FechaCheckOut = @FechaCheckOut,
        FechaActualizacion = GETDATE(),
        UsuarioActualizacionId = COALESCE(@UsuarioId, UsuarioActualizacionId)
    WHERE Id = @SolicitudId
      AND EmpresaMandanteId = @EmpresaMandanteId
      AND CampamentoId = @CampamentoId
      AND FechaCheckOut IS NULL;
END
GO