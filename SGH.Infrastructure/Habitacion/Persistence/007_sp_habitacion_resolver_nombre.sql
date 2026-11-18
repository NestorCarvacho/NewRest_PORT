/* Resolver habitación por nombre/código en el campamento (carga masiva de camas). */
CREATE OR ALTER PROCEDURE dbo.he_sp_habitacion_id_por_nombre
    @CampamentoId int,
    @Nombre       nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1) h.Id
    FROM dbo.Habitaciones AS h WITH (NOLOCK)
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    WHERE p.CampamentoId = @CampamentoId
      AND LTRIM(RTRIM(h.Nombre)) = LTRIM(RTRIM(@Nombre));
END
GO