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
- **Desasignar:** `sp_Solicitud_DesasignarHabitacion` delega en el mismo legado con **`@habitacion = NULL`** (deja `HabitacionId` en NULL y conserva auditoría). No usar `@habitacion = 0`: la generación de reservas exige `HabitacionId IS NULL`, no cero.

Validación por **género/sexo** no está en el legado; queda en el plan `Docs/SGH-solicitud-asignacion-genero-plan.md`.