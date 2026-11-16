/* Alinea IsActive con Estado en filas ya guardadas con estado ACTIVO e IsActive=0 */
UPDATE dbo.Banos
SET IsActive = 1
WHERE Estado = N'ACTIVO'
  AND IsActive = 0;
GO