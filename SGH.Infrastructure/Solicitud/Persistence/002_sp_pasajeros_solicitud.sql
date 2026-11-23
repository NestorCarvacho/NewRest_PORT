CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_listar_pasajeros
    @SolicitudId int,
    @EmpresaMandanteId int,
    @CampamentoId int
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.SolicitudesAlojamiento
        WHERE Id = @SolicitudId AND EmpresaMandanteId = @EmpresaMandanteId AND CampamentoId = @CampamentoId)
        THROW 51030, 'La solicitud no existe en el campamento de la sesión.', 1;

    EXEC dbo.SOL_ObtienePasajerosSolicitudAlojamiento @solicitud = @SolicitudId;
END
GO

CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_reservas_por_pasajero
    @SolicitudId int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Id AS PasajeroSolicitudId, MIN(r.Id) AS NumeroReserva
    FROM dbo.PasajerosSolicitudAlojamiento AS p
    LEFT JOIN dbo.Reservas AS r
        ON r.SolicitudAlojamientoId = p.SolicitudAlojamientoId AND r.PasajeroId = p.UsuarioPasajeroId
    WHERE p.SolicitudAlojamientoId = @SolicitudId
    GROUP BY p.Id;
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
