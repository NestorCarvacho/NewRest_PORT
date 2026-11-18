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

    /* Fuera de la transacción: si el masivo rechaza la fila, el cierre no debe revertirse con ROLLBACK. */
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