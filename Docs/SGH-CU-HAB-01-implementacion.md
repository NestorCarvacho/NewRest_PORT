# CU-HAB-01 — Implementación en NewRest (habitaciones)

## Alcance implementado (MVP)

| CU | NewRest | Notas |
|----|---------|--------|
| CRUD **Habitación** | Sí — `MidHabitaciones`, `/Habitacion/Datos` | Envuelve `HAB_GuardaHabitacion` / `HAB_ValidaHabitacion` vía `sp_Habitacion_Guardar` |
| CRUD **Baño** | No | No existe `dbo.Bano` en HOTELDB |
| CRUD **Cama** (N filas) | Parcial | Capacidad = `CantidadCamas`; sin tabla `Camas` ni `habilitada` por plaza |
| **Turno** horario | Parcial | Flags `usoPorTurnos` → `MinutosMin/Max`; turno operativo de faena sigue en `Turnos` / solicitud |
| RN-01 … RN-06 | Parcial | Ver matriz en conversación de análisis; reglas en `ReglasHabitacion` + validación legado |

## Scripts SQL (aplicar en HOTELDB)

`SGH.Infrastructure/Habitacion/Persistence/001_sp_habitacion.sql`

## API Mid

Ruta: `/Habitacion/Mid/MidHabitaciones.cs`  
Opciones: `listar`, `catalogo`, `obtener`, `crear`, `editar`, `inactivar`

## Próximos pasos hacia CU completo

1. Inventariar si el legado modela “baño compartido” sin tabla dedicada.
2. Camas individuales + `Reservas.CamaId` si el negocio exige RN-03 granular.
3. `HabitacionDiaSemana` / `XMLSemana` en guardado para RN-06 horaria.
4. Enlace menú HOTELDB `Menus` para producción (mapeo `/app/config/habitaciones`).