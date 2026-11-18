# CU-HAB-02 — Camas: propuesta de columnas (inventario por plaza)

**Estado:** pendiente de autorización de negocio / QA.  
**No ejecutar SQL** hasta marcar cada columna como aprobada.  
**Base actual:** `SGH.Infrastructure/Habitacion/Persistence/002_hotldb_cu_hab_extension.sql` + `003_sp_bano_cama.sql`.

## 1. Qué hay hoy (ya en HOTELDB si aplicaron CU-HAB-01)

Cada **habitación** tiene N filas en `dbo.Camas`. La reserva y la solicitud **no usan `CamaId`** todavía: operan con conteo (`CantidadCamas`, camas habilitadas, asignación por cantidad).

| Columna | Tipo | ¿Qué hace? |
|---------|------|------------|
| `Id` | `int` IDENTITY | Identificador único de la plaza/cama. |
| `HabitacionId` | `int` FK | Habitación a la que pertenece. |
| `CodigoCama` | `nvarchar(50)` | Etiqueta visible (ej. `Cama 1`, `A`, `Litera sup.`). Único por habitación entre activas. |
| `Habilitada` | `bit` | **Operativa:** si cuenta para disponibilidad y puede recibir pasajero (junto con el agregado en `Habitaciones`). Por defecto `1`. |
| `Orden` | `int` | Orden en UI y en listados (`ORDER BY Orden, CodigoCama`). |
| `IsActive` | `bit` | Baja lógica; no borra historial. Al inactivar, deja de contar en `CantidadCamas` calculada. |
| `FechaCreacion`, `UsuarioCreacionId` | auditoría | Alta del registro. |
| `FechaActualizacion`, `UsuarioActualizacionId` | auditoría | Última modificación. |

**API Mid (ya cableada):** `listarcamas`, `guardarcama` — solo exponen los campos de la tabla anterior. La pantalla de habitaciones hoy edita **N° camas** a nivel habitación, no el detalle por fila.

---

## 2. Objetivo del módulo (inventario)

Registrar **por cada cama/plaza** datos de mantenimiento e inventario (tipo de plaza, cambio de colchón, estado del blanco, etc.), sin romper el flujo actual de solicitud/reserva por **cantidad**.

Reglas de diseño propuestas:

1. **Inventario vive en `dbo.Camas`** en una primera fase (una fila = una plaza con sus atributos).
2. Catálogos cerrados vía **`dbo.Listas`** (`IdTabla` + `Valor1`), mismo patrón que sexo/estado de habitación.
3. Campos nuevos **NULL por defecto** en datos existentes; la sincronización `sp_Habitacion_SincronizarCamas` no los rellena.
4. **`Habilitada`** sigue siendo operativo; inventario no bloquea asignación en v1 (alertas solo en UI/informes).

---

## 3. Columnas nuevas propuestas

Leyenda de autorización: marcar **Sí** / **No** / **Ajustar** en la columna *Tu visto*.

### 3.1 Tipo de plaza y capacidad

| Columna propuesta | Tipo | Oblig. | ¿Qué hace? | UI | Impacto otros módulos | Tu visto |
|-------------------|------|--------|------------|----|-------------------------|----------|
| `CantidadPlazas` | `tinyint` NOT NULL DEFAULT `1` | Sí en alta manual | Número de plazas que ocupa esta cama (1 = individual, 2 = doble/matrimonial, etc.). Útil para inventario y futura asignación por plaza. | Número, mín 1 | No cambia solicitud hasta usar `CamaId`. Puede validar que suma de plazas ≤ capacidad de habitación (regla futura). | |
| `TipoCamaTabla` | `int` NULL | No | `IdTabla` en `Listas` para catálogo *tipo de cama* (ver §4). | Select | Solo maestros / reportes en v1. | |
| `TipoCama` | `varchar(10)` NULL | No | Código detalle (`Valor1`) enlazado a `TipoCamaTabla` (ej. `1P`, `2P`, `LIT_SUP`). | Select (dependiente) | Igual que arriba. | |
| `AnchoCm` | `smallint` NULL | No | Ancho nominal del lecho (cm). Opcional si el catálogo ya define medida. | Número | Ninguno v1. | |
| `LargoCm` | `smallint` NULL | No | Largo nominal (cm). | Número | Ninguno v1. | |

### 3.2 Colchón

| Columna propuesta | Tipo | Oblig. | ¿Qué hace? | UI | Impacto otros módulos | Tu visto |
|-------------------|------|--------|------------|----|-------------------------|----------|
| `FechaCambioColchon` | `date` NULL | No | Última fecha en que se **reemplazó** el colchón en esta plaza (no el último lavado de funda). | Date picker | Alertas de antigüedad; no bloquea reserva en v1. | |
| `FechaProximoCambioColchon` | `date` NULL | No | Próximo cambio programado (manual o política del campamento). Si NULL, UI puede calcular `FechaCambioColchon + N meses` si definen N. | Date picker | Informes / color en grilla. | |
| `ColchonTipoTabla` | `int` NULL | No | Catálogo Listas *tipo colchón* (espuma, resorte, ortopédico, etc.). | Select | Inventario. | |
| `ColchonTipo` | `varchar(10)` NULL | No | Código detalle Listas. | Select | Inventario. | |
| `ColchonMarca` | `nvarchar(80)` NULL | No | Marca o proveedor del colchón actual. | Texto | Inventario. | |
| `ColchonObservacion` | `nvarchar(500)` NULL | No | Notas (daños, restricciones médicas del activo, etc.). | Textarea | Ninguno. | |

### 3.3 Ropa de cama / blanco

| Columna propuesta | Tipo | Oblig. | ¿Qué hace? | UI | Impacto otros módulos | Tu visto |
|-------------------|------|--------|------------|----|-------------------------|----------|
| `FechaCambioRopaCama` | `date` NULL | No | Último cambio completo de juego de ropa (sábanas/funda) en esa plaza. | Date picker | Aseos / censo futuro; no obligatorio v1. | |
| `EstadoRopaTabla` | `int` NULL | No | Catálogo estado del blanco (nuevo, en uso, en lavandería, dañado). | Select | Operaciones aseo. | |
| `EstadoRopa` | `varchar(10)` NULL | No | Código Listas. | Select | Operaciones aseo. | |

### 3.4 Somier / base y activo fijo

| Columna propuesta | Tipo | Oblig. | ¿Qué hace? | UI | Impacto otros módulos | Tu visto |
|-------------------|------|--------|------------|----|-------------------------|----------|
| `TieneSomier` | `bit` NULL | No | Indica si la plaza tiene somier/base registrada. | Checkbox | Inventario. | |
| `FechaCambioSomier` | `date` NULL | No | Último reemplazo del somier/base. | Date picker | Mantenimiento. | |
| `CodigoActivoFijo` | `nvarchar(50)` NULL | No | Código de activo del ERP/inventario del mandante (si existe). | Texto | Integración externa opcional. | |

### 3.5 Estado general de la plaza

| Columna propuesta | Tipo | Oblig. | ¿Qué hace? | UI | Impacto otros módulos | Tu visto |
|-------------------|------|--------|------------|----|-------------------------|----------|
| `EstadoFisicoTabla` | `int` NULL | No | Catálogo estado físico (bueno, regular, en reparación, dado de baja). **Distinto** de `Habilitada`: una cama puede estar inhabilitada por aseo (`Habilitada=0`) pero estado físico “bueno”. | Select | Puede cruzarse con `Habilitada` en reglas futuras. | |
| `EstadoFisico` | `varchar(10)` NULL | No | Código Listas. | Select | Idem. | |
| `Observaciones` | `nvarchar(1000)` NULL | No | Texto libre de la plaza. | Textarea | Ninguno. | |

---

## 4. Catálogos `Listas` sugeridos (a confirmar IdTabla en HOTELDB)

| Código negocio | `Valor1` ejemplo | Uso |
|----------------|------------------|-----|
| **Tipo cama / plaza** (`tipocama` o nuevo IdTabla) | `1P` 1 plaza, `15P` 1½ plaza, `2P` 2 plazas, `LIT_SUP` / `LIT_INF` litera | `TipoCamaTabla` / `TipoCama` |
| **Tipo colchón** | `ESP`, `RES`, `ORTO` | `ColchonTipo*` |
| **Estado ropa** | `NUEVO`, `USO`, `LAV`, `DAN` | `EstadoRopa*` |
| **Estado físico plaza** | `BUE`, `REG`, `REP`, `BAJ` | `EstadoFisico*` |

SGH 2.1 (maestros) puede cargar estos códigos; hasta entonces valores fijos en Listas de QA acordados con operaciones.

---

## 5. Lo que NO se propone en esta fase

| Tema | Motivo |
|------|--------|
| Tabla hija `CamaArticulos` (colchón, almohada, frazada por fila) | Más flexible, pero más UI y SP; reservar para v2 si necesitan **varios activos** por plaza. |
| `CamaId` en `Reservas` / pasajeros solicitud | Ya anotado en CU-HAB-01; requiere reglas RN-03 y cambio en SP legado. |
| Fotos / adjuntos por cama | Depende de `files.*` (hoy fuera de alcance habitación). |
| Historial de cambios de colchón | v1 solo **última fecha**; historial = tabla `CamaHistorial` futura o auditoría transversal. |

---

## 6. Orden de implementación (después de autorizar columnas)

1. Script incremental `005_hotldb_camas_inventario.sql` (solo columnas **aprobadas**).
2. Extender `sp_Cama_ListarPorHabitacion` / `sp_Cama_Guardar` + DTOs / Mid / UI (pestaña o modal “Camas” en edición de habitación).
3. Documentar en `hoteldb-scripts-orden-ejecucion.md` (parche QA, no re-editar `002` en ambientes ya aplicados).
4. Pruebas: alta habitación con 3 camas, editar inventario por fila, verificar que solicitud sigue usando conteos.

---

## 7. Decisiones que necesitamos de ustedes

1. ¿Basta **una fila por plaza** con columnas de colchón/ropa, o necesitan **varios artículos** por plaza (tabla hija)?
2. ¿`CantidadPlazas` + catálogo `TipoCama` cubren “1 plaza o mayor”, o solo catálogo sin número?
3. ¿Política de **meses entre cambios de colchón** (número fijo por campamento) para calcular alertas?
4. ¿`CodigoActivoFijo` es obligatorio en algunos campamentos?
5. ¿Inhabilitar automáticamente (`Habilitada=0`) si `EstadoFisico=BAJ` o colchón vencido, o solo advertir?

Cuando respondan con **Sí/No/Ajustar** por fila (o un listado corto de columnas aprobadas), se implementa el script y el módulo en código.