# HOTELDB — `HAB_GuardaHabitacion` (as-is)

Referencia extraída del procedimiento en servidor (nov 2026). **No está versionado el cuerpo completo en el repo**; este resumen guía CU-HAB y `sp_Habitacion_Guardar`.

## Rol en el flujo

| Paso | Qué hace |
|------|----------|
| 1 | Llama `HAB_ValidaHabitacion` (validaciones; el **INSERT** de alta, si existe, suele estar aquí — pendiente volcar definición). |
| 2 | `BEGIN TRAN` → **`UPDATE Habitaciones ... WHERE Id = @id`** (no hay `INSERT` en `HAB_GuardaHabitacion`). |
| 3 | `@ret = @id` tras el update. |
| 4 | Archivos en `files.HabitacionFiles` (y copia desde `files.PlantillaUbicacionFiles` si `@PlantillaUbicacionId > 0`). |
| 5 | `HabitacionDiaSemana` desde `@XMLSemana` (RN horaria legado). |
| 6 | `COMMIT` o `ROLLBACK` en `CATCH`. |

Por tanto, **alta con `@id = 0` no la completa este SP** salvo que `HAB_ValidaHabitacion` cree la fila antes. CU-HAB usa **INSERT nativo** en `sp_Habitacion_Guardar`.

## Columnas en el UPDATE legado

Fija explícitamente **`SexoTabla = 11`** y **`Sexo = @sexo`** (`varchar(1)`).

No toca `EstadoTabla` en el UPDATE (solo `estado = @estado`). En filas existentes: **`EstadoTabla = 8`**, **`Estado`** típico `D`.

No actualiza en este SP: `ModuloId`, `BanoId`, flags CU-HAB (`PermiteUsoPorTurnos`, etc.) — eso lo cubre `sp_Habitacion_Guardar` nativo.

## FK `FK_Habitaciones_Listas_SexoTabla_Sexo`

|(padre)|(referencia)|
|-------|------------|
| `Habitaciones.SexoTabla` | `Listas.IdTabla` |
| `Habitaciones.Sexo` | `Listas.Valor1` |

Muestra en producción: `SexoTabla=11`, `Sexo=M`, `EstadoTabla=8`, `Estado=D`.

## Relación con NewRest

- `sp_Habitacion_Catalogo`: `IdTabla 11` → sexos, `IdTabla 8` → estados habitación (coherente con datos anteriores).
- Sin `files.HabitacionFiles` válido, el legado falla al copiar plantilla; CU-HAB evita `HAB_*` y persiste sin adjuntos.

## Pendiente

Volver a exportar **`HAB_ValidaHabitacion`** para documentar el alta (`INSERT`) y paridad total con la pantalla histórica.