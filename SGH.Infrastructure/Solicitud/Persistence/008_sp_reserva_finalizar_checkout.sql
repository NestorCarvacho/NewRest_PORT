/*
    Reservas con check-out operativo deben salir de Estado 'A' para que
    SOL_GuardaPasajeroSolicitudAlojamientoMasivo no las trate como activas.
*/
CREATE OR ALTER PROCEDURE dbo.sp_Reserva_FinalizarCheckout
    @ReservaId int = NULL,
    @EmpresaMandanteId int = NULL,
    @IdentificadorRut varchar(25) = NULL,
    @UsuarioId int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE r
    SET r.FechaSalida = COALESCE(r.FechaSalida, r.CheckOut),
        r.Estado = 'F',
        r.FechaActualizacion = GETDATE(),
        r.UsuarioActualizacionId = COALESCE(@UsuarioId, r.UsuarioActualizacionId)
    FROM dbo.Reservas AS r
    LEFT JOIN dbo.Usuarios AS u ON u.Id = r.PasajeroId
    LEFT JOIN dbo.Empresas AS eSesion ON eSesion.Id = @EmpresaMandanteId
    LEFT JOIN dbo.Empresas AS eRes ON eRes.Id = r.EmpresaMandanteId
    WHERE r.CheckOut IS NOT NULL
      AND r.Estado = 'A'
      AND (@ReservaId IS NULL OR r.Id = @ReservaId)
      AND (@IdentificadorRut IS NULL OR (
            u.EmpresaMandanteId = @EmpresaMandanteId
            AND RTRIM(u.Rut) = RTRIM(@IdentificadorRut)
            AND eSesion.Id IS NOT NULL
            AND eRes.MineraId = eSesion.MineraId));
END
GO