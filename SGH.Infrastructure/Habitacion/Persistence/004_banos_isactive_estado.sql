
UPDATE dbo.Banos
SET IsActive = 1
WHERE Estado = N'ACTIVO'
  AND IsActive = 0;
GO
