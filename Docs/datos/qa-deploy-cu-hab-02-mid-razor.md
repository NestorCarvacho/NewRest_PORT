# QA — merge CU-HAB-02 + Mid JSON y Razor OnPost

Rama: `cursor/cu-hab-02-integracion-mid-razor-f6a4`

## Qué integra este merge

| Superficie | Cómo invoca reglas |
|------------|-------------------|
| **Escritorio** (JSON) | `POST` a `*/Mid/Mid*.cs` — sin cambio de contrato. |
| **Razor** (formularios) | `OnPostAsync` en `ModuloDatosPageModel` → mismo `Mid*.EjecutarAsync` (`ConversorFormularioMid` + `EjecutorPostModulo`). |

Reglas de negocio: solo `SGH.Domain` (`ReglasTipoCama`, `ReglasCama`, `ReglasHabitacion`, etc.) y Mid; no en la página.

Referencias: `Docs/SGH-CU-HAB-02-tipo-cama-modelo.md`, `Docs/prompt-proyecto/post-modulo-razor.md`, lista maestra en `Docs/datos/hoteldb-scripts-orden-ejecucion.md`.

---

## Prerrequisitos en HOTELDB

Si **nunca** aplicaste CU-HAB-01 en QA, ejecutar antes (orden fijo):

| # | Archivo | Objetos principales |
|---|---------|---------------------|
| 1 | `SGH.Infrastructure/Habitacion/Persistence/002_hotldb_cu_hab_extension.sql` | Tablas/columnas `Habitaciones`, `Banos`, `Camas`. |
| 2 | `SGH.Infrastructure/Habitacion/Persistence/003_sp_bano_cama.sql` | `sp_Bano_*`, `sp_Cama_*` (legado sync), `sp_Habitacion_SincronizarCamas`, etc. |
| 3 | `SGH.Infrastructure/Habitacion/Persistence/001_sp_habitacion.sql` | `sp_Habitacion_*` (catálogo, CRUD habitación). |

Opcional (datos): `004_banos_isactive_estado.sql`.

---

## Scripts obligatorios para este merge (CU-HAB-02)

Ejecutar en **SSMS / sqlcmd** contra `ConnectionStrings:Hoteldb`, en este orden:

| # | Archivo | Qué deja listo |
|---|---------|----------------|
| 1 | `SGH.Infrastructure/Habitacion/Persistence/005_hotldb_tipo_cama_inventario.sql` | Tabla `dbo.TipoCama`; columnas inventario en `dbo.Camas` (`TipoCamaId`, fechas/observaciones colchón y ropa). |
| 2 | `SGH.Infrastructure/Habitacion/Persistence/006_sp_tipo_cama_inventario.sql` | Procedimientos listados abajo. |

### Procedimientos creados o actualizados en el paso 2

| Procedimiento | Uso en aplicación |
|---------------|-------------------|
| `dbo.sp_TipoCama_Listar` | Mid `listartiposcama` |
| `dbo.sp_TipoCama_Guardar` | Mid `creartipocama`, `editartipocama`, `activartipocama`, `inactivartipocama` |
| `dbo.sp_Cama_ListarPorHabitacion` | Mid `listarcamas` |
| `dbo.sp_Cama_Guardar` | Mid `guardarcama` (no permite cambiar `HabitacionId`) |
| `dbo.sp_Habitacion_Catalogo` | Octavo result set con tipos de cama; escritorio habitaciones / baños / tipos |

**Re-ejecutable:** ambos archivos son idempotentes (`IF NOT EXISTS` / `CREATE OR ALTER`).

Si QA ya tenía `001_sp_habitacion.sql` antiguo sin el catálogo extendido, basta con el paso 2 (`006` redefine `sp_Habitacion_Catalogo`). Solo re-ejecutar `001_sp_habitacion.sql` completo si el equipo prefiere alinear todo el módulo habitación con el repo actual.

---

## Menú escritorio (recomendado)

Para ver la solapa **Tipos de cama** (`modo: tiposcama`):

| # | Archivo |
|---|---------|
| 1 | `SGH.Infrastructure/Navegacion/Persistence/002_menu_config_habitacion_bano.sql` (si no está aplicado) |
| 2 | Entrada `tiposcama` en `Menus` / `MenusPerfil` según perfil (p. ej. script `003_menusperfil_habitacion_bano_por_perfil.sql` o alta manual documentada en CU-HAB). |

---

## Parches incrementales QA (no bloquean este merge)

Si la base ya corre con `001_*` viejos, el repo documenta parches **A–D** en `Docs/datos/hoteldb-scripts-orden-ejecucion.md`. Para CU-HAB-02 solo importa la fila **D**:

- `005_hotldb_tipo_cama_inventario.sql`
- `006_sp_tipo_cama_inventario.sql`

(Requieren bloque habitación **002** + **003** aplicado.)

---

## Smoke test post-SQL

1. Escritorio: solapa **Tipos de cama** — listar, crear, inactivar.
2. Escritorio: habitación → **Camas** — listar, guardar inventario (tipo, colchón, ropa).
3. Razor (opcional): formulario con `method="post"` en `*/Datos.cshtml` — misma `opcion` que el JSON; ver `TempData` en `_AvisosModuloPost` y `post-modulo-razor.md`.

Sin los scripts **005** y **006**, las opciones Mid de TipoCama/Cama fallan al ejecutar procedimientos o por columnas faltantes.