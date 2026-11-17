# HOTELDB — `SOL_AsignaHabitacionPasajeroSolicitudEmpresa` (as-is)

Procedimiento legado de asignación **sin validaciones** de disponibilidad, sexo ni alcance: solo hace `UPDATE` en la fila del pasajero.

| `@origen` | Tabla |
|-----------|--------|
| `solicitudEmpresa` | `PasajerosSolicitudEmpresa` (+ `ErrorSolicitud = null`) |
| `solicitud` | `PasajerosSolicitudAlojamiento` |

Campos actualizados en solicitud de alojamiento:

- `HabitacionId = @habitacion`
- `FechaActualizacion = getdate()`
- `UsuarioActualizacionId = @usuario`

Devuelve `resultado = @id` y `mensaje = 'OK'` si `@ret` coincide con el id del pasajero.

## NewRest

- **Asignar:** `sp_Solicitud_AsignarHabitacion` valida alcance, disponibilidad vía `HAB_ObtieneHabitacionesDisponiblesV2` y luego delega en el legado con `@origen = 'solicitud'`.
- **Desasignar:** `sp_Solicitud_DesasignarHabitacion` hace `UPDATE` con `HabitacionId = NULL` y la misma auditoría que el legado (`FechaActualizacion`, `UsuarioActualizacionId`). No usar `0`: la generación exige `HabitacionId IS NULL`.

Validación por **género/sexo** no está en el legado; queda en el plan `Docs/SGH-solicitud-asignacion-genero-plan.md`.