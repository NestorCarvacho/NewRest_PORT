# Auditoría en SP (SGH 1.6)

## Convención HOTELDB

| Operación | Columnas típicas |
|-----------|------------------|
| Alta | `FechaCreacion` (servidor), `UsuarioCreacionId` |
| Actualización | `FechaActualizacion`, `UsuarioActualizacionId` |

El `@UsuarioId` del procedimiento debe coincidir con el usuario de sesión. En aplicación usar `AuditoriaPersistencia.UsuarioDeSesion(DatosSesion)` antes de invocar el repositorio.

## Plantilla (nuevo CUD)

```sql
-- Parámetros de aplicación (desde sesión, no desde JSON cliente)
@UsuarioId int,
-- ...

INSERT INTO dbo.Ejemplo (
    ...,
    FechaCreacion,
    UsuarioCreacionId)
VALUES (
    ...,
    SYSDATETIME(),
    @UsuarioId);

-- Actualización
UPDATE dbo.Ejemplo
SET ...,
    UsuarioActualizacionId = @UsuarioId,
    FechaActualizacion = SYSDATETIME()
WHERE Id = @Id;
```

Alias legados (`UsuarioCreacion`, `fechaCreacion`) conservar el nombre del SP existente; no renombrar en migración parcial.

Referencia: `SGH.Infrastructure/Reserva/Persistence/001_sp_reserva.sql`, `Solicitud/Persistence/001_sp_solicitud.sql`.