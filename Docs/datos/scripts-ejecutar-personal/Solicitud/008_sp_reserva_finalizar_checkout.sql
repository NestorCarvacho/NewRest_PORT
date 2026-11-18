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