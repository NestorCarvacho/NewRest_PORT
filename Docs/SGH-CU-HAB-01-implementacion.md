# CU-HAB-01 — Implementación en NewRest (habitaciones)

## Alcance implementado

| CU | NewRest | Notas |
|----|---------|--------|
| CRUD **Habitación** | Sí | `MidHabitaciones`, `/Habitacion/Datos`; `sp_Habitacion_*` + legado `HAB_*` |
| CRUD **Baño** | Sí | Tabla `dbo.Banos`, `sp_Bano_*`, pestaña Baños |
| CRUD **Cama** (N filas) | Sí | Tabla `dbo.Camas`, sincronización al guardar habitación; toggles `habilitada` en edición |
| **Turno** horario (operativo faena) | Parcial | Flags explícitos + `TiempoHigienizacionMin`; turnos de faena en `Turnos` / solicitud |
| RN-01 … RN-06 | Parcial | RN-01/RN-02 en SP y `ReglasBano` / `ReglasHabitacion`; resto según matriz CU |

## Scripts SQL (aplicar en HOTELDB, en orden)

1. `SGH.Infrastructure/Habitacion/Persistence/002_hotldb_cu_hab_extension.sql` — tablas y columnas
2. `SGH.Infrastructure/Habitacion/Persistence/003_sp_bano_cama.sql` — procedimientos baño/cama
3. `SGH.Infrastructure/Habitacion/Persistence/001_sp_habitacion.sql` — habitación (lee `Banos`/`Camas`)

Sin **002** y **003**, **001** referencia columnas/objetos inexistentes.

## API Mid

Ruta: `/Habitacion/Mid/MidHabitaciones.cs`

UI: solapas del escritorio (`habitaciones`, `banos`), igual que Censo/Reserva; páginas `/Habitacion/Datos` y `/Habitacion/Banos` abren el módulo inicial vía `escritorio.js`.

**Habitaciones:** `listar`, `catalogo`, `obtener`, `crear`, `editar`, `inactivar`

**Baños:** `listarbanos`, `crearbano`, `editarbano`, `inactivarbano`

**Camas:** `listarcamas`, `guardarcama`

## Próximos pasos

1. `HabitacionDiaSemana` / `XMLSemana` en guardado para RN-06 horaria.
2. Menú Configuración en HOTELDB: `002_menu_config_habitacion_bano.sql` + mapeo `/app/config/habitaciones` y `/app/config/banos`.
3. Reserva por `CamaId` si el negocio exige RN-03 a nivel de plaza.