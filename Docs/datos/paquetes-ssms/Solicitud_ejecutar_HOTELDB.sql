SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_buscar
    @EmpresaMandanteId int,
    @CampamentoId int,
    @Desde date = NULL,
    @Hasta date = NULL,
    @GrupoId int,
    @SolicitanteId int,
    @Estado varchar(1),
    @Numero int,
    @MaxFilas int = 5000
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Listado de solicitudes con rango 30 días y TOP @MaxFilas por defecto.
    --======================================================

    SET NOCOUNT ON;

IF @Desde IS NULL AND @Hasta IS NULL
    BEGIN
        SET @Hasta = CAST(GETDATE() AS date);
        SET @Desde = DATEADD(day, -30, @Hasta);
    END
    ELSE IF @Desde IS NULL
        SET @Desde = DATEADD(day, -30, @Hasta);
    ELSE IF @Hasta IS NULL
        SET @Hasta = CAST(GETDATE() AS date);

    SELECT TOP (@MaxFilas) s.Id AS Numero, s.FechaSolicitud AS Fecha,
           a.Nombre + ' - ' + e.Nombre AS Empresa,
           u.Nombre + ' ' + u.Apellidos AS Solicitante,
           l.Descripcion AS Estado,
           s.Id AS SolicitudId,
           s.Estado AS EstadoCodigo,
           s.EstadoTabla,
           s.CampamentoId,
           s.EmpresaMandanteId,
           s.AreaEmpresaSolicitanteId,
           s.UsuarioSolicitanteId,
           a.EmpresaId AS EmpresaContratistaId,
           s.FechaCheckIn,
           s.FechaCheckOut,
           s.TurnoId,
           s.ConSistemaTurno,
           (SELECT TOP (1) r.Id FROM dbo.Reservas AS r WHERE r.SolicitudAlojamientoId = s.Id ORDER BY r.Id) AS PrimeraReservaGeneradaId
    FROM dbo.SolicitudesAlojamiento AS s WITH (NOLOCK)
    INNER JOIN dbo.AreasEmpresa AS a WITH (NOLOCK) ON a.Id = s.AreaEmpresaSolicitanteId
    INNER JOIN dbo.Empresas AS e WITH (NOLOCK) ON e.Id = a.EmpresaId
    LEFT JOIN dbo.Usuarios AS u WITH (NOLOCK) ON u.Id = s.UsuarioSolicitanteId
    LEFT JOIN dbo.Listas AS l WITH (NOLOCK) ON l.IdTabla = s.EstadoTabla AND l.Valor1 = s.Estado AND l.Tipo = 'Detalle'
    WHERE s.EmpresaMandanteId = @EmpresaMandanteId
      AND s.CampamentoId = @CampamentoId
      AND s.FechaSolicitud >= @Desde AND s.FechaSolicitud < DATEADD(day, 1, @Hasta)
      AND (@GrupoId = 0 OR s.AreaEmpresaSolicitanteId = @GrupoId)
      AND (@SolicitanteId = 0 OR s.UsuarioSolicitanteId = @SolicitanteId)
      AND (@Estado = '0' OR s.Estado = @Estado)
      AND (@Numero = 0 OR s.Id = @Numero)
    ORDER BY s.FechaSolicitud, s.Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_obtener_primera_reserva
    @Id int,
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Id de la primera reserva generada para una solicitud.
    --======================================================

    SET NOCOUNT ON;

SELECT TOP (1) r.Id
    FROM dbo.Reservas r
    INNER JOIN dbo.SolicitudesAlojamiento s ON s.Id = r.SolicitudAlojamientoId
    WHERE s.Id = @Id AND s.EmpresaMandanteId = @EmpresaMandanteId AND s.CampamentoId = @CampamentoId
    ORDER BY r.Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_listar_catalogo
    @EmpresaMandanteId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Catálogos para filtros y formulario de solicitud.
    --======================================================

    SET NOCOUNT ON;

SELECT a.Id, e.Nombre + ' - ' + a.Nombre AS Nombre, a.TurnoId, e.UnidadNegocioId, e.Contrato
    FROM dbo.EmpresasMandanteAreasEmpresaContratista AS rel
    INNER JOIN dbo.AreasEmpresa AS a ON a.Id = rel.AreaEmpresaContratistaId
    INNER JOIN dbo.Empresas AS e ON e.Id = a.EmpresaId
    WHERE rel.EmpresaMandanteId = @EmpresaMandanteId
    ORDER BY Nombre;

    SELECT Valor1 AS Id, Descripcion AS Nombre
    FROM dbo.Listas WHERE IdTabla = 1 AND Tipo = 'Detalle'
    ORDER BY Orden, Descripcion;

    SELECT Id, Nombre, DiasTrabajados, DiasLibres, TipoTurno
    FROM dbo.Turnos
    WHERE EmpresaMandanteId = @EmpresaMandanteId
    ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_listar_organizacion
    @EmpresaMandanteId int,
    @UnidadId int,
    @DivisionId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Áreas y solicitantes para el campamento.
    --======================================================

    SET NOCOUNT ON;

DECLARE @MineraId int;
    SELECT @MineraId = MineraId FROM dbo.Empresas WHERE Id = @EmpresaMandanteId;
    IF @MineraId IS NULL
        THROW 51037, 'La empresa mandante no tiene una minera configurada.', 1;
    IF @UnidadId <> 0 AND NOT EXISTS (
        SELECT 1 FROM dbo.UnidadesNegocio WHERE Id = @UnidadId AND MineraId = @MineraId)
        THROW 51038, 'La unidad de negocio no corresponde a la empresa de la sesión.', 1;
    IF @DivisionId <> 0 AND NOT EXISTS (
        SELECT 1 FROM dbo.Divisiones WHERE Id = @DivisionId AND UnidadNegocioId = @UnidadId)
        THROW 51039, 'La división no corresponde a la unidad de negocio seleccionada.', 1;

    EXEC dbo.UNG_ObtieneUnidadesNegocio @Minera = @MineraId, @IsActive = NULL, @Nombre = NULL;
    EXEC dbo.DIV_ObtieneDivisiones @UnidadNegocio = @UnidadId;
    EXEC dbo.DEP_ObtieneDepartamentos @Division = @DivisionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_guardar
    @Id int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int,
    @FechaSolicitud datetime,
    @FechaCheckIn datetime,
    @FechaCheckOut datetime = NULL,
    @GrupoId int,
    @SolicitanteId int = NULL,
    @ConSistemaTurno int,
    @TurnoId int = NULL,
    @TurnoRotacionId int = NULL,
    @Observacion varchar(200) = NULL,
    @Adc varchar(100) = NULL,
    @Email varchar(100),
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
    --  date: 2026-11-20
    --  description: Alta y edición de solicitud de alojamiento.
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

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_anular
    @Id int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Anula solicitud en estado permitido.
    --======================================================

    SET NOCOUNT ON;

UPDATE dbo.SolicitudesAlojamiento
    SET Estado = 'A', UsuarioActualizacionId = @UsuarioId, FechaActualizacion = GETDATE()
    WHERE Id = @Id AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId;

    IF @@ROWCOUNT = 0
        THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_listar_pasajeros
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Pasajeros vinculados a la solicitud.
    --======================================================

    SET NOCOUNT ON;

IF NOT EXISTS (
        SELECT 1 FROM dbo.SolicitudesAlojamiento
        WHERE Id = @SolicitudId AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId)
        THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;

    EXEC dbo.SOL_ObtienePasajerosSolicitudAlojamiento @solicitud = @SolicitudId;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_agregar_cupos
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int,
    @Cantidad int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Aumenta cupos de pasajeros en la solicitud.
    --======================================================

    SET NOCOUNT ON;

SET XACT_ABORT ON;

    IF @Cantidad NOT BETWEEN 1 AND 999
        THROW 51042, 'La cantidad de cupos debe estar entre 1 y 999.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Estado nvarchar(450), @Desde datetime, @Hasta datetime, @Ret int;
        SELECT @Estado = Estado, @Desde = FechaCheckIn, @Hasta = FechaCheckOut
        FROM dbo.SolicitudesAlojamiento WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @SolicitudId AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId;

        IF @Estado IS NULL
            THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;
        IF @Estado IN ('R', 'A')
            THROW 51043, 'No se pueden modificar los pasajeros de una solicitud reservada o anulada.', 1;

        EXEC dbo.SOL_GuardaPasajeroSolicitudAlojamiento
            @id = 0, @solicitudId = @SolicitudId, @pasajero = 0,
            @fechaCheckin = @Desde, @fechaCheckout = @Hasta, @usuario = @UsuarioId,
            @porCantidad = 1, @cantidad = @Cantidad, @ret = @Ret OUTPUT;
        IF ISNULL(@Ret, 0) <= 0
            THROW 51044, 'No se pudieron agregar los cupos.', 1;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_eliminar_pasajero
    @SolicitudId int,
    @PasajeroId int,
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Quita pasajero de la solicitud.
    --======================================================

    SET NOCOUNT ON;

SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Estado nvarchar(450);
        SELECT @Estado = Estado
        FROM dbo.SolicitudesAlojamiento WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @SolicitudId AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId;
        IF @Estado IS NULL
            THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;
        IF @Estado IN ('R', 'A')
            THROW 51043, 'No se pueden modificar los pasajeros de una solicitud reservada o anulada.', 1;

        DELETE FROM dbo.PasajerosSolicitudAlojamiento
        WHERE Id = @PasajeroId AND SolicitudAlojamientoId = @SolicitudId;
        IF @@ROWCOUNT <> 1
            THROW 51045, 'El pasajero no pertenece a la solicitud.', 1;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_guardar_pasajero
    @SolicitudId int,
    @PasajeroId int,
    @UsuarioPasajeroId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int,
    @FechaCheckIn datetime,
    @FechaCheckOut datetime = NULL,
    @Ret int OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Alta o edición de pasajero en detalle.
    --======================================================

    SET NOCOUNT ON;

SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Estado nvarchar(450), @GrupoId int;
        SELECT @Estado = Estado, @GrupoId = AreaEmpresaSolicitanteId
        FROM dbo.SolicitudesAlojamiento WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @SolicitudId AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId;

        IF @Estado IS NULL
            THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;
        IF @Estado IN ('R', 'A')
            THROW 51043, 'No se pueden modificar los pasajeros de una solicitud reservada o anulada.', 1;
        IF @PasajeroId <> 0 AND NOT EXISTS (
            SELECT 1 FROM dbo.PasajerosSolicitudAlojamiento WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @PasajeroId AND SolicitudAlojamientoId = @SolicitudId)
            THROW 51045, 'El pasajero no pertenece a la solicitud.', 1;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.Usuarios u
            WHERE u.Id = @UsuarioPasajeroId AND u.EmpresaMandanteId = @EmpresaMandanteId
              AND u.AreaEmpresaContratistaId = @GrupoId AND u.IsActive = 1
              AND (u.PerfilId = 2 OR EXISTS (
                  SELECT 1 FROM dbo.PerfilesUsuario pu
                  WHERE pu.UsuarioId = u.Id AND pu.EmpresaMandanteId = u.EmpresaMandanteId AND pu.PerfilId = 2))
              AND TRIM(u.Rut) <> '')
            THROW 51046, 'El pasajero no pertenece al catálogo activo del grupo.', 1;
        IF EXISTS (
            SELECT 1 FROM dbo.PasajerosSolicitudAlojamiento
            WHERE SolicitudAlojamientoId = @SolicitudId AND UsuarioPasajeroId = @UsuarioPasajeroId AND Id <> @PasajeroId)
            THROW 51047, 'El pasajero ya se encuentra en la solicitud.', 1;

        DECLARE @Resultado TABLE (resultado int, mensaje varchar(1000));
        INSERT INTO @Resultado
        EXEC dbo.SOL_GuardaPasajeroSolicitudAlojamiento
            @id = @PasajeroId, @solicitudId = @SolicitudId, @pasajero = @UsuarioPasajeroId,
            @fechaCheckin = @FechaCheckIn, @fechaCheckout = @FechaCheckOut,
            @usuario = @UsuarioId, @ret = @Ret OUTPUT;
        IF ISNULL(@Ret, 0) <= 0
        BEGIN
            DECLARE @Mensaje nvarchar(2048) = (SELECT TOP (1) mensaje FROM @Resultado);
            IF @Mensaje IS NULL OR @Mensaje = ''
                SET @Mensaje = 'El procedimiento legado no pudo guardar el pasajero.';
            THROW 51048, @Mensaje, 1;
        END
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_habitaciones_disponibles
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Habitaciones disponibles para asignar a solicitud.
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
    --  date: 2026-11-20
    --  description: Asigna habitación a solicitud/reserva en preparación.
    --======================================================

    SET NOCOUNT ON;

SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Estado nvarchar(450), @HabitacionActual int, @Bloqueo int;
        DECLARE @Recurso nvarchar(255) = CONCAT('SGH_Solicitud_Habitacion_', @CampamentoId);
        EXEC @Bloqueo = sys.he_sp_getapplock @Resource = @Recurso, @LockMode = 'Exclusive',
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
    --  date: 2026-11-20
    --  description: Quita asignación de habitación.
    --======================================================

    SET NOCOUNT ON;

SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Estado nvarchar(450), @Bloqueo int;
        DECLARE @Recurso nvarchar(255) = CONCAT('SGH_Solicitud_Habitacion_', @CampamentoId);
        EXEC @Bloqueo = sys.he_sp_getapplock @Resource = @Recurso, @LockMode = 'Exclusive',
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
    --  date: 2026-11-20
    --  description: Genera reservas desde solicitud (legado SOL/RES).
    --======================================================

    SET NOCOUNT ON;

SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Bloqueo int;
        DECLARE @Recurso nvarchar(255) = CONCAT('SGH_Solicitud_Habitacion_', @CampamentoId);
        EXEC @Bloqueo = sys.he_sp_getapplock @Resource = @Recurso, @LockMode = 'Exclusive',
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
CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_asegurar_periodo
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int = NULL
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Alinea fechas de solicitud para importación masiva.
    --======================================================

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
CREATE OR ALTER PROCEDURE dbo.he_sp_reserva_finalizar_checkout
    @ReservaId int = NULL,
    @EmpresaMandanteId int = NULL,
    @IdentificadorRut varchar(25) = NULL,
    @UsuarioId int = NULL
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Checkout y cierre de reserva tras importación u operación O.
    --======================================================

    SET NOCOUNT ON;

DECLARE @RutNorm varchar(25) = NULL;
    IF @IdentificadorRut IS NOT NULL
        SET @RutNorm = REPLACE(REPLACE(REPLACE(UPPER(RTRIM(@IdentificadorRut)), '.', ''), '-', ''), ' ', '');

    UPDATE r
    SET r.FechaSalida = COALESCE(r.FechaSalida, r.CheckOut),
        r.Estado = 'F',
        r.FechaActualizacion = GETDATE(),
        r.UsuarioActualizacionId = COALESCE(@UsuarioId, r.UsuarioActualizacionId)
    FROM dbo.Reservas AS r
    INNER JOIN dbo.Campamentos AS camp ON camp.Id = r.CampamentoId
    INNER JOIN dbo.Empresas AS emprCamp ON emprCamp.Id = camp.EmpresaMandanteId
    INNER JOIN dbo.Empresas AS emprSesion ON emprSesion.Id = @EmpresaMandanteId
    WHERE r.CheckOut IS NOT NULL
      AND r.Estado = 'A'
      AND (@ReservaId IS NULL OR r.Id = @ReservaId)
      AND emprCamp.MineraId = emprSesion.MineraId
      AND (@RutNorm IS NULL OR EXISTS (
            SELECT 1
            FROM dbo.Usuarios AS u
            WHERE u.Id = r.PasajeroId
              AND REPLACE(REPLACE(REPLACE(UPPER(RTRIM(ISNULL(u.Rut, ''))), '.', ''), '-', ''), ' ', '') = @RutNorm
      ));
END
GO
CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_cargar_pasajero
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int,
    @Identificador varchar(25),
    @TipoIdentificador varchar(10),
    @Nombres varchar(100),
    @Apellidos varchar(100),
    @Genero varchar(10),
    @Ret int OUTPUT,
    @Mensaje varchar(1000) OUTPUT
AS
BEGIN
    --======================================================
    --  author: Nestor Carvacho
    --  date: 2026-11-20
    --  description: Carga masiva de pasajeros (SOL_GuardaPasajero masivo).
    --======================================================

    SET NOCOUNT ON;

SET XACT_ABORT ON;

    DECLARE @Estado nvarchar(450);
    SELECT @Estado = Estado
    FROM dbo.SolicitudesAlojamiento
    WHERE Id = @SolicitudId AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId;
    IF @Estado IS NULL
        THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;
    IF @Estado IN ('R', 'A')
        THROW 51043, 'No se pueden modificar los pasajeros de una solicitud reservada o anulada.', 1;

    EXEC dbo.he_sp_solicitud_asegurar_periodo
        @SolicitudId = @SolicitudId,
        @EmpresaMandanteId = @EmpresaMandanteId,
        @CampamentoId = @CampamentoId,
        @UsuarioId = @UsuarioId;

    EXEC dbo.he_sp_reserva_finalizar_checkout
        @EmpresaMandanteId = @EmpresaMandanteId,
        @IdentificadorRut = @Identificador,
        @UsuarioId = @UsuarioId;

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Resultado TABLE (resultado int, mensaje varchar(1000));
        INSERT INTO @Resultado
        EXEC dbo.SOL_GuardaPasajeroSolicitudAlojamientoMasivo
            @rut = @Identificador, @tipoIdentificador = @TipoIdentificador,
            @nombres = @Nombres, @apellidos = @Apellidos, @solicitud = @SolicitudId,
            @usuario = @UsuarioId, @genero = @Genero, @ret = @Ret OUTPUT;
        SELECT TOP (1) @Mensaje = mensaje FROM @Resultado;
        IF @Ret IS NULL
            THROW 51048, 'El procedimiento legado no devolvió un resultado válido.', 1;
        IF @Ret <= 0
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
