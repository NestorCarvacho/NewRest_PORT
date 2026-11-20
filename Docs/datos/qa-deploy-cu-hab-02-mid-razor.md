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

Ejecutar en **SSMS** contra **HOTELDB** (una sola vez, idempotente):

| Archivo | Contenido |
|---------|-----------|
| `Docs/datos/paquetes-ssms/Habitacion_ejecutar_HOTELDB.sql` | CU-HAB-01/02: tablas `Banos`, `Camas`, `TipoCama`, columnas, `UPDATE` baños y todos los `he_sp_*` habitación/baño/cama. |

Misma definición: `SGH.Infrastructure/Habitacion/Persistence/001_habitacion_hotldb.sql`.

---

## Procedimientos relevantes para CU-HAB-02 (dentro del archivo único)

### Procedimientos creados o actualizados en el paso 2

| Procedimiento | Uso en aplicación |
|---------------|-------------------|
| `dbo.he_sp_tipo_cama_listar` | Mid `listartiposcama` |
| `dbo.he_sp_tipo_cama_guardar` | Mid `creartipocama`, `editartipocama`, `activartipocama`, `inactivartipocama` |
| `dbo.he_sp_cama_listar_por_habitacion` | Mid `listarcamas` |
| `dbo.he_sp_cama_guardar` | Mid `guardarcama` (no permite cambiar `HabitacionId`) |
| `dbo.he_sp_habitacion_catalogo` | Octavo result set con tipos de cama; escritorio habitaciones / baños / tipos |

**Re-ejecutable:** `CREATE OR ALTER` e `IF NOT EXISTS` en el script único.

---

## Menú escritorio (recomendado)

Para ver la solapa **Tipos de cama** (`modo: tiposcama`):

| # | Archivo |
|---|---------|
| 1 | `SGH.Infrastructure/Navegacion/Persistence/002_menu_config_habitacion_bano.sql` (si no está aplicado) |
| 2 | Entrada `tiposcama` en `Menus` / `MenusPerfil` según perfil (p. ej. script `003_menusperfil_habitacion_bano_por_perfil.sql` o alta manual documentada en CU-HAB). |

---

## Parches incrementales QA (no bloquean este merge)

Parches viejos por archivo **obsoletos**: volver a ejecutar `001_habitacion_hotldb.sql` o el paquete SSMS (ver fila **D** en `hoteldb-scripts-orden-ejecucion.md`).

---

## Smoke test post-SQL

1. Escritorio: solapa **Tipos de cama** — listar, crear, inactivar.
2. Escritorio: habitación → **Camas** — listar, guardar inventario (tipo, colchón, ropa).
3. Razor (opcional): formulario con `method="post"` en `*/Datos.cshtml` — misma `opcion` que el JSON; ver `TempData` en `_AvisosModuloPost` y `post-modulo-razor.md`.

Sin los scripts **005** y **006**, las opciones Mid de TipoCama/Cama fallan al ejecutar procedimientos o por columnas faltantes.