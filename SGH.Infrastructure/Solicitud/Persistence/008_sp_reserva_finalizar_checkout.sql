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
    LEFT JOIN dbo.Empresas AS e ON e.Id = @EmpresaMandanteId
    LEFT JOIN dbo.Campamentos AS c ON c.Id = r.CampamentoId AND e.Id IS NOT NULL AND c.MineraId = e.MineraId
    WHERE r.CheckOut IS NOT NULL
      AND r.Estado = 'A'
      AND (@ReservaId IS NULL OR r.Id = @ReservaId)
      AND (@IdentificadorRut IS NULL OR (u.EmpresaMandanteId = @EmpresaMandanteId AND RTRIM(u.Rut) = RTRIM(@IdentificadorRut)))
      AND (@IdentificadorRut IS NULL OR c.Id IS NOT NULL);
END
GO