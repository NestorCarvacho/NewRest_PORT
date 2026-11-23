/*
    Anula el último movimiento operativo: si hay check-out lo quita (vuelve a check-in);
    si solo hay check-in, lo quita (vuelve a asignada sin ingreso).
*/
CREATE OR ALTER PROCEDURE dbo.he_sp_reserva_reversar_movimiento
    @ReservaId int,
    @CampamentoId int,
    @UsuarioId int
AS
BEGIN
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