CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_HabitacionesDisponibles
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Grupo int, @Empresa int, @Desde datetime, @Hasta datetime;
    SELECT @Grupo = s.AreaEmpresaSolicitanteId, @Empresa = a.EmpresaId,
           @Desde = s.FechaCheckIn, @Hasta = s.FechaCheckOut
    FROM dbo.SolicitudesAlojamiento s
    INNER JOIN dbo.AreasEmpresa a ON a.Id = s.AreaEmpresaSolicitanteId
    WHERE s.Id = @SolicitudId AND s.EmpresaMandanteId = @EmpresaMandanteId AND s.CampamentoId = @CampamentoId;
    IF @Grupo IS NULL
        THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;
    EXEC dbo.HAB_ObtieneHabitacionesDisponiblesV2
        @grupoEmpresa = @Grupo, @empresaContratista = @Empresa, @modulo = 0, @piso = 0,
        @habitacion = 0, @fechaLlegada = @Desde, @fechaSalida = @Hasta,
        @EmpresaMandante = @EmpresaMandanteId, @Campamento = @CampamentoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_AsignarHabitacion
    @SolicitudId int,
    @PasajeroId int,
    @HabitacionId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Estado nvarchar(450), @HabitacionActual int, @Bloqueo int;
        DECLARE @Recurso nvarchar(255) = CONCAT('SGH_Solicitud_Habitacion_', @CampamentoId);
        EXEC @Bloqueo = sys.sp_getapplock @Resource = @Recurso, @LockMode = 'Exclusive',
            @LockOwner = 'Transaction', @LockTimeout = 10000;
        IF @Bloqueo < 0
            THROW 51055, 'No se pudo bloquear la asignación de habitaciones. Intente nuevamente.', 1;
        SELECT @Estado = Estado
        FROM dbo.SolicitudesAlojamiento WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @SolicitudId AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId;
        IF @Estado IS NULL
            THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;
        IF @Estado IN ('R', 'A')
            THROW 51043, 'No se pueden modificar los pasajeros de una solicitud reservada o anulada.', 1;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.PasajerosSolicitudAlojamiento WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @PasajeroId AND SolicitudAlojamientoId = @SolicitudId)
            THROW 51045, 'El pasajero no pertenece a la solicitud.', 1;
        SELECT @HabitacionActual = HabitacionId FROM dbo.PasajerosSolicitudAlojamiento WHERE Id = @PasajeroId;

        IF @HabitacionActual IS NULL OR @HabitacionActual <> @HabitacionId
        BEGIN
            DECLARE @Disponibles TABLE (
                pabellonId int, pabellonName nvarchar(100), moduloId int, moduloName nvarchar(100),
                habitacionId int, habitacionName nvarchar(100), totalCamas int, sexo nvarchar(100),
                tipoInstalacion nvarchar(100), StandardHabitacion nvarchar(100),
                areaEmpresaContratista int, cantidadCamasAsignadas int, cantidadCamasDisponibles int);
            INSERT INTO @Disponibles
            EXEC dbo.sp_Solicitud_HabitacionesDisponibles @SolicitudId, @EmpresaMandanteId, @CampamentoId;
            IF NOT EXISTS (
                SELECT 1 FROM @Disponibles d
                WHERE d.habitacionId = @HabitacionId
                  AND d.cantidadCamasDisponibles > (
                      SELECT COUNT(*) FROM dbo.PasajerosSolicitudAlojamiento p
                      WHERE p.SolicitudAlojamientoId = @SolicitudId AND p.HabitacionId = @HabitacionId))
                THROW 51056, 'La habitación no está disponible para esta solicitud o no tiene camas disponibles.', 1;

            DECLARE @Ret int;
            EXEC dbo.SOL_AsignaHabitacionPasajeroSolicitudEmpresa
                @id = @PasajeroId, @habitacion = @HabitacionId, @usuario = @UsuarioId,
                @origen = 'solicitud', @ret = @Ret OUTPUT;
            IF ISNULL(@Ret, 0) <> @PasajeroId
                THROW 51057, 'El procedimiento legado no pudo asignar la habitación.', 1;
        END
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_DesasignarHabitacion
    @SolicitudId int,
    @PasajeroId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Estado nvarchar(450), @Bloqueo int;
        DECLARE @Recurso nvarchar(255) = CONCAT('SGH_Solicitud_Habitacion_', @CampamentoId);
        EXEC @Bloqueo = sys.sp_getapplock @Resource = @Recurso, @LockMode = 'Exclusive',
            @LockOwner = 'Transaction', @LockTimeout = 10000;
        IF @Bloqueo < 0
            THROW 51055, 'No se pudo bloquear la asignación de habitaciones. Intente nuevamente.', 1;
        SELECT @Estado = Estado
        FROM dbo.SolicitudesAlojamiento WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @SolicitudId AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId;
        IF @Estado IS NULL
            THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;
        IF @Estado IN ('R', 'A')
            THROW 51043, 'No se pueden modificar los pasajeros de una solicitud reservada o anulada.', 1;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.PasajerosSolicitudAlojamiento WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @PasajeroId AND SolicitudAlojamientoId = @SolicitudId)
            THROW 51045, 'El pasajero no pertenece a la solicitud.', 1;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.PasajerosSolicitudAlojamiento
            WHERE Id = @PasajeroId AND SolicitudAlojamientoId = @SolicitudId AND HabitacionId IS NOT NULL)
            THROW 51058, 'El pasajero no tiene habitación asignada.', 1;

        UPDATE dbo.PasajerosSolicitudAlojamiento
        SET HabitacionId = NULL
        WHERE Id = @PasajeroId AND SolicitudAlojamientoId = @SolicitudId;
        IF @@ROWCOUNT = 0
            THROW 51058, 'No se pudo quitar la habitación del pasajero.', 1;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
