-- ============================================================================
-- Reserva: 003_sp_reserva_anular.sql
-- ============================================================================

CREATE OR ALTER PROCEDURE dbo.he_sp_reserva_anular
    @ReservaId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Anula una reserva.
    --======================================================
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Reservas WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @ReservaId AND CampamentoId = @CampamentoId)
        THROW 51020, 'La reserva no corresponde al campamento.', 1;

    IF EXISTS (
        SELECT 1 FROM dbo.Reservas
        WHERE Id = @ReservaId AND Estado IN ('N', 'F'))
        THROW 51025, 'La reserva ya está anulada o finalizada.', 1;

    UPDATE dbo.Reservas
    SET Estado = 'N',
        UsuarioActualizacionId = @UsuarioId,
        FechaActualizacion = GETDATE()
    WHERE Id = @ReservaId AND CampamentoId = @CampamentoId;
END
GO

-- ============================================================================
-- Reserva: 004_sp_reserva_reversar_movimiento.sql
-- ============================================================================

CREATE OR ALTER PROCEDURE dbo.he_sp_reserva_reversar_movimiento
    @ReservaId int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Reversa el último movimiento operativo de una reserva.
    --======================================================
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Reservas WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @ReservaId AND CampamentoId = @CampamentoId)
    BEGIN
        RAISERROR('La reserva no corresponde al campamento.', 16, 1);
        RETURN;
    END;

    DECLARE @Estado char(1);
    DECLARE @CheckIn datetime;
    DECLARE @CheckOut datetime;

    SELECT @Estado = Estado, @CheckIn = CheckIn, @CheckOut = CheckOut
    FROM dbo.Reservas
    WHERE Id = @ReservaId;

    IF @Estado <> 'A'
    BEGIN
        RAISERROR('Solo se puede anular movimientos en reservas asignadas.', 16, 1);
        RETURN;
    END;

    IF @CheckOut IS NOT NULL
    BEGIN
        UPDATE dbo.Reservas
        SET CheckOut = NULL,
            UsuarioActualizacionId = @UsuarioId,
            FechaActualizacion = GETDATE()
        WHERE Id = @ReservaId AND CampamentoId = @CampamentoId;
        RETURN;
    END;

    IF @CheckIn IS NOT NULL
    BEGIN
        UPDATE dbo.Reservas
        SET CheckIn = NULL,
            UsuarioActualizacionId = @UsuarioId,
            FechaActualizacion = GETDATE()
        WHERE Id = @ReservaId AND CampamentoId = @CampamentoId;
        RETURN;
    END;

    RAISERROR('La reserva no tiene check-in ni check-out para anular.', 16, 1);
END
GO

-- ============================================================================
-- Solicitud: 001_sp_solicitud.sql (he_sp_solicitud_contacto_correo, he_sp_solicitud_guardar)
-- ============================================================================

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_contacto_correo
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Obtiene los correos de contacto de la empresa y el campamento.
    --======================================================
    SET NOCOUNT ON;
    SELECT NULLIF(LTRIM(RTRIM(e.Email)), '') AS EmailEmpresa
    FROM dbo.Empresas AS e
    WHERE e.Id = @EmpresaMandanteId;
    SELECT NULLIF(LTRIM(RTRIM(c.Email)), '') AS EmailCampamento
    FROM dbo.Campamentos AS c
    WHERE c.Id = @CampamentoId;
END
GO
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_guardar
    @Id int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int,
    @FechaSolicitud datetime,
    @FechaCheckIn datetime = NULL,
    @FechaCheckOut datetime = NULL,
    @GrupoId int,
    @SolicitanteId int = NULL,
    @ConSistemaTurno int,
    @TurnoId int = NULL,
    @TurnoRotacionId int = NULL,
    @Observacion varchar(200) = NULL,
    @Adc varchar(100) = NULL,
    @Email varchar(100) = NULL,
    @Telefono varchar(20) = NULL,
    @UnidadNegocioId int = NULL,
    @DivisionId int = NULL,
    @DepartamentoId int = NULL,
    @NumeroContrato varchar(100) = NULL,
    @ReservaVisita int,
    @Ret int OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Guarda o actualiza una solicitud de alojamiento.
    --======================================================
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
        IF @Id <> 0 AND NOT EXISTS (
            SELECT 1 FROM dbo.SolicitudesAlojamiento WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @Id AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId)
            THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;

        IF @Id = 0 AND NOT EXISTS (
            SELECT 1 FROM dbo.EmpresasMandanteAreasEmpresaContratista
            WHERE EmpresaMandanteId = @EmpresaMandanteId AND AreaEmpresaContratistaId = @GrupoId)
            THROW 51031, 'El grupo no corresponde a la empresa de la sesión.', 1;

        IF @Id = 0 AND NOT EXISTS (
            SELECT 1 FROM dbo.AreasEmpresa WHERE Id = @GrupoId AND TurnoId IS NOT NULL)
            THROW 51033, 'El grupo seleccionado no tiene un turno por defecto configurado.', 1;

        IF @Id = 0
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.Turnos WHERE Id = @TurnoId AND EmpresaMandanteId = @EmpresaMandanteId)
                THROW 51040, 'El turno principal no corresponde a la empresa de la sesión.', 1;
            IF @TurnoRotacionId IS NOT NULL AND NOT EXISTS (
                SELECT 1 FROM dbo.Turnos WHERE Id = @TurnoRotacionId AND EmpresaMandanteId = @EmpresaMandanteId)
                THROW 51040, 'El turno de rotación no corresponde a la empresa de la sesión.', 1;

            DECLARE @UnidadGrupo int;
            SELECT @UnidadGrupo = e.UnidadNegocioId
            FROM dbo.AreasEmpresa AS a
            INNER JOIN dbo.Empresas AS e ON e.Id = a.EmpresaId
            WHERE a.Id = @GrupoId;

            IF @UnidadGrupo > 0 AND ISNULL(@UnidadNegocioId, 0) <> @UnidadGrupo
                THROW 51034, 'La unidad de negocio debe corresponder al grupo seleccionado.', 1;
            IF @UnidadGrupo > 0 AND ISNULL(@DivisionId, 0) = 0
               AND EXISTS (SELECT 1 FROM dbo.Divisiones WHERE UnidadNegocioId = @UnidadGrupo)
                THROW 51035, 'Seleccione una división para la unidad de negocio del grupo.', 1;
            IF @UnidadGrupo > 0 AND ISNULL(@DivisionId, 0) > 0 AND ISNULL(@DepartamentoId, 0) = 0
               AND EXISTS (SELECT 1 FROM dbo.Departamentos WHERE DivisionId = @DivisionId)
                THROW 51036, 'Seleccione un departamento para la división.', 1;
        END

        IF @UnidadNegocioId IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.UnidadesNegocio u INNER JOIN dbo.Empresas e ON e.MineraId = u.MineraId
            WHERE u.Id = @UnidadNegocioId AND e.Id = @EmpresaMandanteId)
            THROW 51038, 'La unidad de negocio no corresponde a la empresa de la sesión.', 1;
        IF @DivisionId IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.Divisiones WHERE Id = @DivisionId AND UnidadNegocioId = @UnidadNegocioId)
            THROW 51039, 'La división no corresponde a la unidad de negocio seleccionada.', 1;
        IF @DepartamentoId IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.Departamentos WHERE Id = @DepartamentoId AND DivisionId = @DivisionId)
            THROW 51041, 'El departamento no corresponde a la división seleccionada.', 1;

        EXEC dbo.SOL_GuardaSolicitudAlojamiento
            @id = @Id, @fecha = @FechaSolicitud, @fechaCheckIn = @FechaCheckIn,
            @fechaCheckOut = @FechaCheckOut, @observacion = @Observacion,
            @grupo = @GrupoId, @usuarioSolicitante = @SolicitanteId,
            @empresaMandante = @EmpresaMandanteId, @campamento = @CampamentoId,
            @estado = '', @turno = @TurnoId, @turnoRotacion = @TurnoRotacionId,
            @conSistemaTurno = @ConSistemaTurno, @usuario = @UsuarioId, @adc = @Adc,
            @emailContactoCampamento = @Email, @numeroContactoCampamento = @Telefono,
            @unidadNegocio = @UnidadNegocioId, @division = @DivisionId,
            @departamento = @DepartamentoId, @numeroContrato = @NumeroContrato,
            @reservaVisita = @ReservaVisita, @ret = @Ret OUTPUT;

        IF @Ret <= 0
            THROW 51032, 'No se pudo guardar la solicitud.', 1;

        DECLARE @SolicitudId int = CASE WHEN @Id = 0 THEN @Ret ELSE @Id END;
        EXEC dbo.he_sp_solicitud_asegurar_periodo
            @SolicitudId = @SolicitudId,
            @EmpresaMandanteId = @EmpresaMandanteId,
            @CampamentoId = @CampamentoId,
            @UsuarioId = @UsuarioId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
GO


-- ============================================================================
-- Solicitud: 002_sp_pasajeros_solicitud.sql (he_sp_solicitud_reservas_por_pasajero)
-- ============================================================================

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_reservas_por_pasajero
    @SolicitudId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Obtiene las reservas asociadas a un pasajero en una solicitud.
    --======================================================
    SET NOCOUNT ON;
    SELECT p.Id AS PasajeroSolicitudId, MIN(r.Id) AS NumeroReserva
    FROM dbo.PasajerosSolicitudAlojamiento AS p
    LEFT JOIN dbo.Reservas AS r
        ON r.SolicitudAlojamientoId = p.SolicitudAlojamientoId AND r.PasajeroId = p.UsuarioPasajeroId
    WHERE p.SolicitudAlojamientoId = @SolicitudId
    GROUP BY p.Id;
END
GO
GO


-- ============================================================================
-- Solicitud: 004_sp_generar_reservas.sql
-- ============================================================================

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_generar_reservas
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int,
    @PrimeraReservaId int OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Genera las reservas para una solicitud de alojamiento.
    --======================================================
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

        EXEC dbo.he_sp_solicitud_asegurar_periodo
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
        EXEC dbo.he_sp_solicitud_habitaciones_disponibles @SolicitudId, @EmpresaMandanteId, @CampamentoId;
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

-- ============================================================================
-- Solicitud: 005_sp_habitaciones_solicitud.sql
-- ============================================================================

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_habitaciones_disponibles
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Lista habitaciones disponibles para una solicitud de alojamiento.
    --======================================================
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

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_asignar_habitacion
    @SolicitudId int,
    @PasajeroId int,
    @HabitacionId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Asigna una habitación a un pasajero en una solicitud de alojamiento.
    --======================================================
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
            EXEC dbo.he_sp_solicitud_habitaciones_disponibles @SolicitudId, @EmpresaMandanteId, @CampamentoId;
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

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_desasignar_habitacion
    @SolicitudId int,
    @PasajeroId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Desasigna la habitación de un pasajero en una solicitud de alojamiento.
    --======================================================
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

        DECLARE @Ret int;
        EXEC dbo.SOL_AsignaHabitacionPasajeroSolicitudEmpresa
            @id = @PasajeroId, @habitacion = NULL, @usuario = @UsuarioId,
            @origen = 'solicitud', @ret = @Ret OUTPUT;
        IF ISNULL(@Ret, 0) <> @PasajeroId
            THROW 51057, 'El procedimiento legado no pudo quitar la habitación.', 1;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- Censo: 005_sp_censo_qr.sql
-- 20261123 ncarvacho: Censo QR — catálogo con tipos de instalación, habitaciones con capacidad y detalle he_sp_censo_obtener_habitacion_qr.
-- ============================================================================

CREATE OR ALTER PROCEDURE dbo.he_sp_censo_listar_catalogo
    @CampamentoId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Catálogo censo con tipos de instalación y habitaciones QR (capacidad).
    --======================================================
    SET NOCOUNT ON;

    SELECT p.Id, p.Nombre AS Etiqueta, 0 AS PadreId
    FROM dbo.Pabellones AS p
    WHERE p.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY p.Nombre;

    SELECT m.Id, m.Nombre AS Etiqueta, m.PabellonId AS PadreId
    FROM dbo.Modulos AS m
    INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
    WHERE m.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY m.Nombre;

    SELECT h.Id, h.Nombre AS Etiqueta, h.ModuloId AS PadreId
    FROM dbo.Habitaciones AS h
    INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
    WHERE h.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY h.Nombre;

    SELECT t.Id, t.Nombre AS Etiqueta, 0 AS PadreId
    FROM dbo.TiposInstalacion AS t WITH (NOLOCK)
    WHERE t.IsActive = 1
      AND (t.EmpresaMandanteId IS NULL OR EXISTS (
          SELECT 1 FROM dbo.Campamentos AS c
          WHERE c.Id = @CampamentoId AND c.EmpresaMandanteId = t.EmpresaMandanteId))
    ORDER BY t.Nombre;

    SELECT
        h.Id,
        h.Nombre AS Etiqueta,
        h.ModuloId AS PadreId,
        ISNULL(h.TipoInstalacionId, 0) AS TipoInstalacionId,
        ISNULL((
            SELECT COUNT(*)
            FROM dbo.Camas AS c WITH (NOLOCK)
            WHERE c.HabitacionId = h.Id AND c.IsActive = 1 AND c.Habilitada = 1
        ), CASE WHEN ISNULL(h.CantidadCamas, 0) > 0 THEN h.CantidadCamas ELSE 0 END) AS Capacidad
    FROM dbo.Habitaciones AS h
    INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
    WHERE h.IsActive = 1
      AND p.CampamentoId = @CampamentoId
    ORDER BY h.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_censo_obtener_habitacion_qr
    @CampamentoId int,
    @HabitacionId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-23
    --  description: Detalle de habitación para censo QR (módulo, piso, capacidad).
    --======================================================
    SET NOCOUNT ON;

    SELECT
        h.Id AS HabitacionId,
        h.Nombre AS Habitacion,
        p.Nombre AS Modulo,
        m.Nombre AS Piso,
        ISNULL((
            SELECT COUNT(*)
            FROM dbo.Camas AS c WITH (NOLOCK)
            WHERE c.HabitacionId = h.Id AND c.IsActive = 1 AND c.Habilitada = 1
        ), CASE WHEN ISNULL(h.CantidadCamas, 0) > 0 THEN h.CantidadCamas ELSE 0 END) AS Capacidad
    FROM dbo.Habitaciones AS h
    INNER JOIN dbo.Modulos AS m ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p ON p.Id = m.PabellonId
    WHERE h.Id = @HabitacionId
      AND h.IsActive = 1
      AND p.CampamentoId = @CampamentoId;
END
GO
