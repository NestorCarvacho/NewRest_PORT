CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_GuardarPasajero
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
