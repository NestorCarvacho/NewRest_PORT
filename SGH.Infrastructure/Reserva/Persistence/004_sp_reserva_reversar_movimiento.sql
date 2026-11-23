/*
    Revierte el último movimiento operativo: si hay check-out lo quita (vuelve a check-in);
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
        THROW 51020, 'La reserva no corresponde al campamento.', 1;

    DECLARE @Estado char(1);
    DECLARE @CheckIn datetime2;
    DECLARE @CheckOut datetime2;

    SELECT @Estado = Estado, @CheckIn = CheckIn, @CheckOut = CheckOut
    FROM dbo.Reservas
    WHERE Id = @ReservaId;

    IF @Estado <> 'A'
        THROW 51028, 'Solo se puede reversar movimientos en reservas asignadas.', 1;

    IF @CheckOut IS NOT NULL
    BEGIN
        UPDATE dbo.Reservas
        SET CheckOut = NULL,
            UsuarioActualizacionId = @UsuarioId,
            FechaActualizacion = GETDATE()
        WHERE Id = @ReservaId AND CampamentoId = @CampamentoId;
        RETURN;
    END

    IF @CheckIn IS NOT NULL
    BEGIN
        UPDATE dbo.Reservas
        SET CheckIn = NULL,
            UsuarioActualizacionId = @UsuarioId,
            FechaActualizacion = GETDATE()
        WHERE Id = @ReservaId AND CampamentoId = @CampamentoId;
        RETURN;
    END

    THROW 51029, 'La reserva no tiene check-in ni check-out para reversar.', 1;
END
GO