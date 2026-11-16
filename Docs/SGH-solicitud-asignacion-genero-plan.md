# Plan — género en asignación automática de habitaciones (solicitud)

**Contexto:** La pantalla de solicitud ya expone el género en la importación masiva (`Genero` en planilla y en `Usuarios`). `HAB_ObtieneHabitacionesDisponiblesV2` devuelve `sexo` por habitación. Hoy **Asignar automático** no cruza pasajero vs. sexo de la cama.

## Objetivo

Al usar asignación automática (y opcionalmente al asignar manual), respetar la regla de negocio de compatibilidad de sexo entre pasajero y habitación/cama, con mensajes claros cuando no haya cabida.

## Alcance propuesto

| Fase | Qué | Dónde |
|------|-----|--------|
| 1 | Confirmar fuente de género del pasajero en listado solicitud (`PasajeroSolicitudDto` / `SOL_ObtienePasajeros*`) | SQL + consulta |
| 2 | Exponer `genero` en JSON `pasajeros` del Mid | Dapper / DTO |
| 3 | Regla en dominio: `ReglasSolicitudHabitacion` (o ampliar `ReglasHabitacion`) — tabla de compatibilidad sexo habitación vs. género pasajero | `SGH.Domain` |
| 4 | Validación servidor al `asignar` (manual): fallar antes de `SOL_Asigna*` si no compatible | `sp_Solicitud_AsignarHabitacion` o capa C# |
| 5 | **Asignar automático** en `solicitud.js`: filtrar habitaciones donde `sexo` sea compatible; omitir o avisar pasajeros sin match | UI + mismo contrato que fase 4 |
| 6 | Pruebas unitarias de reglas + test de Mid con habitación de sexo exclusivo | `SGH.Tests` |

## Datos a levantar en HOTELDB

- Valores de `Listas` / `Habitaciones.Sexo` y mapeo con género de usuario (`M`/`F`/otros).
- Si el legado valida en `SOL_AsignaHabitacionPasajeroSolicitudEmpresa` (no replicar reglas contradictorias).

## Criterios de aceptación

- Pasajero **M** no se asigna automáticamente a habitación solo **F** (y viceversa), según matriz acordada.
- Mensaje explícito: *"No hay habitación compatible con el género del pasajero X"* cuando el lote automático no complete.
- Asignación manual y masiva automática comparten la misma regla (Mid o SP único).

## Fuera de alcance (inicial)

- Cambiar sexo de habitación en caliente.
- Reglas de convivencia por empresa o estándar de habitación más allá del sexo.