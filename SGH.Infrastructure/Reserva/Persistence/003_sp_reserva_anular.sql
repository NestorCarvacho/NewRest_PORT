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