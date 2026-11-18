# CU-HAB-02 — Modelo TipoCama y inventario en Cama

## Entidades

### `dbo.TipoCama` (catálogo por campamento)

| Columna | Uso |
|---------|-----|
| `Nombre` | Etiqueta (ej. «1 plaza», «2 plazas»). |
| `AnchoCm`, `LargoCm` | Medidas del lecho para inventario y listados. |
| `IsActive` | Baja lógica. |

### `dbo.Camas` (campos nuevos)

| Columna | Uso |
|---------|-----|
| `TipoCamaId` | FK al catálogo. |
| `FechaColchon` | Último cambio de colchón. |
| `ColchonObservacion` | Notas del colchón. |
| `FechaCambioRopa` | Último cambio de ropa de cama. |
| `RopaCamaObservacion` | Notas del blanco. |

### `IsActive` vs `Habilitada`

| Campo | Significado |
|-------|-------------|
| `IsActive` | La plaza existe en la habitación (inventario). `0` = baja lógica; no se muestra en listados operativos por defecto. |
| `Habilitada` | La plaza cuenta para disponibilidad / asignación (puede estar activa pero deshabilitada por aseo). |

**Regla:** `HabitacionId` de una cama **no se cambia** al editar. Una cama no puede moverse a otra habitación (`ReglasCama` + `sp_Cama_Guardar`).

Los campos operativos (`CodigoCama`, `Habilitada`, `Orden`, `IsActive`) se conservan; el alta solo crea filas en la habitación indicada (`sp_Habitacion_SincronizarCamas`).

## SQL (orden)

1. `005_hotldb_tipo_cama_inventario.sql`
2. `006_sp_tipo_cama_inventario.sql` (incluye `sp_Cama_*` y octavo result set de `sp_Habitacion_Catalogo`)

## Mid (`MidHabitaciones`)

- Tipos: `listartiposcama`, `creartipocama`, `editartipocama`, `inactivartipocama`, `activartipocama`
- Camas: `listarcamas`, `guardarcama` (payload con inventario)

## UI escritorio

- Solapa **Tipos de cama** (`tiposcama` en menú; registrar en `Menus` si aplica).
- En habitaciones: acción **Camas** → inventario por plaza.