# CU-HAB-01 — Implementación en NewRest (habitaciones)

## Alcance implementado

| CU | NewRest | Notas |
|----|---------|--------|
| CRUD **Habitación** | Sí | `MidHabitaciones`, `/Habitacion/Datos`; `he_sp_habitacion_guardar` nativo (ver `Docs/datos/hab-guarda-habitacion-as-is.md`; `HAB_GuardaHabitacion` solo UPDATE) |
| CRUD **Baño** | Sí | Tabla `dbo.Banos`, `he_sp_bano_*`, pestaña Baños |
| CRUD **Cama** (N filas) | Sí | Tabla `dbo.Camas`, sincronización al guardar habitación; toggles `habilitada` en edición |
| **Turno** horario (operativo faena) | Parcial | Flags explícitos + `TiempoHigienizacionMin`; turnos de faena en `Turnos` / solicitud |
| RN-01 … RN-06 | Parcial | RN-01/RN-02 en SP y `ReglasBano` / `ReglasHabitacion`; resto según matriz CU |

## Scripts SQL (aplicar en HOTELDB, en orden)

1. `SGH.Infrastructure/Habitacion/Persistence/001_habitacion_hotldb.sql` — tablas, columnas y todos los procedimientos CU-HAB (un solo archivo; QA: `Docs/datos/paquetes-ssms/Habitacion_ejecutar_HOTELDB.sql`)

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