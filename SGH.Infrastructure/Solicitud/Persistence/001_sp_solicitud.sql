CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_Buscar
    @EmpresaMandanteId int,
    @CampamentoId int,
    @Desde date,
    @Hasta date,
    @GrupoId int,
    @SolicitanteId int,
    @Estado varchar(1),
    @Numero int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.Id AS Numero, s.FechaSolicitud AS Fecha,
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
    FROM dbo.SolicitudesAlojamiento AS s
    INNER JOIN dbo.AreasEmpresa AS a ON a.Id = s.AreaEmpresaSolicitanteId
    INNER JOIN dbo.Empresas AS e ON e.Id = a.EmpresaId
    LEFT JOIN dbo.Usuarios AS u ON u.Id = s.UsuarioSolicitanteId
    LEFT JOIN dbo.Listas AS l ON l.IdTabla = s.EstadoTabla AND l.Valor1 = s.Estado AND l.Tipo = 'Detalle'
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

CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_ObtenerPrimeraReserva
    @Id int,
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1) r.Id
    FROM dbo.Reservas r
    INNER JOIN dbo.SolicitudesAlojamiento s ON s.Id = r.SolicitudAlojamientoId
    WHERE s.Id = @Id AND s.EmpresaMandanteId = @EmpresaMandanteId AND s.CampamentoId = @CampamentoId
    ORDER BY r.Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_ListarCatalogo
    @EmpresaMandanteId int
AS
BEGIN
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

CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_ListarOrganizacion
    @EmpresaMandanteId int,
    @UnidadId int,
    @DivisionId int
AS
BEGIN
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

CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_Guardar
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
        EXEC dbo.sp_Solicitud_AsegurarPeriodo
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

CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_Anular
    @Id int,
    @EmpresaMandanteId int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SolicitudesAlojamiento
    SET Estado = 'A', UsuarioActualizacionId = @UsuarioId, FechaActualizacion = GETDATE()
    WHERE Id = @Id AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId;

    IF @@ROWCOUNT = 0
        THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;
END
GO
