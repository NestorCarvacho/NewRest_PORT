CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_GenerarReservas
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int,
    @PrimeraReservaId int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Bloqueo int;
        DECLARE @Recurso nvarchar(255) = CONCAT('SGH_Solicitud_Habitacion_', @CampamentoId);
        EXEC @Bloqueo = sys.sp_getapplock @Resource = @Recurso, @LockMode = 'Exclusive',
            @LockOwner = 'Transaction', @LockTimeout = 10000;
        IF @Bloqueo < 0
            THROW 51055, 'No se pudo bloquear la generación de reservas. Intente nuevamente.', 1;
        DECLARE @Estado nvarchar(450), @ConTurno bit, @TurnoId int;
        SELECT @Estado = Estado, @ConTurno = ConSistemaTurno, @TurnoId = TurnoId
        FROM dbo.SolicitudesAlojamiento WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @SolicitudId AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId;
        IF @Estado IS NULL
            THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;
        IF @Estado <> 'P'
            THROW 51049, 'Solo se pueden generar reservas de una solicitud pendiente.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.PasajerosSolicitudAlojamiento WHERE SolicitudAlojamientoId = @SolicitudId)
            THROW 51050, 'Agregue al menos un pasajero o cupo antes de generar reservas.', 1;
        IF EXISTS (SELECT 1 FROM dbo.PasajerosSolicitudAlojamiento WHERE SolicitudAlojamientoId = @SolicitudId AND HabitacionId IS NULL)
            THROW 51051, 'Asigne una habitación a todos los pasajeros antes de generar reservas.', 1;
        IF @ConTurno = 1 AND NOT EXISTS (
            SELECT 1 FROM dbo.Turnos WHERE Id = @TurnoId AND DiasTrabajados > 0 AND DiasLibres >= 0)
            THROW 51052, 'El turno de la solicitud no tiene una duración válida.', 1;

        EXEC dbo.sp_Solicitud_AsegurarPeriodo
            @SolicitudId = @SolicitudId,
            @EmpresaMandanteId = @EmpresaMandanteId,
            @CampamentoId = @CampamentoId,
            @UsuarioId = @UsuarioId;

        DECLARE @Disponibles TABLE (
            pabellonId int, pabellonName nvarchar(100), moduloId int, moduloName nvarchar(100),
            habitacionId int, habitacionName nvarchar(100), totalCamas int, sexo nvarchar(100),
            tipoInstalacion nvarchar(100), StandardHabitacion nvarchar(100),
            areaEmpresaContratista int, cantidadCamasAsignadas int, cantidadCamasDisponibles int);
        INSERT INTO @Disponibles
        EXEC dbo.sp_Solicitud_HabitacionesDisponibles @SolicitudId, @EmpresaMandanteId, @CampamentoId;
        IF EXISTS (
            SELECT 1
            FROM dbo.PasajerosSolicitudAlojamiento p
            LEFT JOIN @Disponibles d ON d.habitacionId = p.HabitacionId
            WHERE p.SolicitudAlojamientoId = @SolicitudId
            GROUP BY p.HabitacionId, d.habitacionId, d.cantidadCamasDisponibles
            HAVING d.habitacionId IS NULL OR COUNT(*) > d.cantidadCamasDisponibles)
            THROW 51056, 'La disponibilidad de las habitaciones cambió. Revise las asignaciones antes de generar reservas.', 1;

        UPDATE r
        SET r.FechaSalida = COALESCE(r.FechaSalida, r.CheckOut),
            r.Estado = 'F',
            r.FechaActualizacion = GETDATE(),
            r.UsuarioActualizacionId = @UsuarioId
        FROM dbo.Reservas AS r
        INNER JOIN dbo.PasajerosSolicitudAlojamiento AS p ON p.UsuarioPasajeroId = r.PasajeroId
        INNER JOIN dbo.Campamentos AS camp ON camp.Id = r.CampamentoId
        INNER JOIN dbo.Empresas AS emprCamp ON emprCamp.Id = camp.EmpresaMandanteId
        INNER JOIN dbo.Empresas AS e ON e.Id = @EmpresaMandanteId AND emprCamp.MineraId = e.MineraId
        WHERE p.SolicitudAlojamientoId = @SolicitudId
          AND r.CheckOut IS NOT NULL
          AND r.Estado = 'A';

        DECLARE @Conflictos TABLE (PasajeroId int, PasajeroNombre varchar(1000), ReservaId int, Tipo int);
        INSERT INTO @Conflictos
        EXEC dbo.HAB_ValidaCreacionReserva_Solicitud @SolicitudId = @SolicitudId;
        IF EXISTS (SELECT 1 FROM @Conflictos)
        BEGIN
            DECLARE @Mensaje nvarchar(2048);
            SELECT TOP (1) @Mensaje = CASE WHEN Tipo = 1
                THEN CONCAT(N'El pasajero ', PasajeroNombre, N' ya tiene una reserva para el periodo solicitado. Reserva N', NCHAR(176), N' ', ReservaId, N'.')
                ELSE PasajeroNombre END
            FROM @Conflictos;
            IF @Mensaje IS NULL OR @Mensaje = ''
                SET @Mensaje = 'La validación legada detectó un conflicto de reserva.';
            THROW 51053, @Mensaje, 1;
        END

        EXEC dbo.RES_CreaReserva @solicitudId = @SolicitudId, @usuarioCreacionId = @UsuarioId,
            @ret = @PrimeraReservaId OUTPUT;
        IF ISNULL(@PrimeraReservaId, 0) <= 0 OR NOT EXISTS (
            SELECT 1 FROM dbo.Reservas WHERE Id = @PrimeraReservaId AND SolicitudAlojamientoId = @SolicitudId)
            THROW 51054, 'El procedimiento legado no devolvió una reserva válida.', 1;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
