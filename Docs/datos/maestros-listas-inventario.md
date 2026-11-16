# Inventario maestros — SGH 2.1.0

**Estado:** **EN CURSO** — diccionario y SP maestros **actualizados** (nov 2026); matriz de **41 catálogos `Listas`** sigue **pendiente** hasta consulta en HOTELDB.

## Fuentes versionadas

| Recurso | Contenido |
|---------|-----------|
| [`Diccionario_de_datos_HotelDB.xlsx`](Diccionario_de_datos_HotelDB.xlsx) | **Hoja `Tablas`:** ~154 tablas, metadatos de columnas. **Hoja `Procedimientos`:** ~525 SP/funciones usuario (`Esquema`, `Nombre`, `Tipo`, fechas). |
| [`diccionario-hoteldb-procedimientos-almacenados.md`](diccionario-hoteldb-procedimientos-almacenados.md) | Consultas SQL para repetir o ampliar el inventario en la instancia corporativa. |
| [`extractos/hoteldb-maestros-sp-filtrados.tsv`](extractos/hoteldb-maestros-sp-filtrados.tsv) | Resultado **§2** (117 SP del filtro maestros / operación). |
| [`extractos/hoteldb-maestros-sp-parametros.tsv`](extractos/hoteldb-maestros-sp-parametros.tsv) | Parámetros de SP `LIS_*`, `DEP_*`, `DIV_*`, `UNG_*`, `EMP_*` (subconjunto de **§3**). |

Exportación cruda del agente (TSV completo §3) no se versiona; basta el xlsx **Procedimientos** + extractos.

## Tablas maestras (hoja `Tablas`)

Columnas relevantes para migración MVC+Mid:

### `dbo.Listas`

`IdTabla`, `Tipo`, `Descripcion`, `Valor1`, `Orden`, `IsEditable`, auditoría, `MineraId`, `Observacion`.

- **Lectura/escritura legado:** `LIS_ObtieneListas`, `LIS_ObtenerListasPorMinera`, `LIS_GuardaItemLista`, `LIS_EliminaItemLista`.
- **Consumo NewRest (sin pantalla):** p. ej. `IdTabla = 34` / `Valor1 = 'usrpasshist'` en login; `IdTabla = 1` y `5` en Solicitud/Reserva (ver SP en `SGH.Infrastructure/**/Persistence/*.sql`).

### Organización

| Tabla | PK / relaciones | SP CRUD en HOTELDB (prefijo) |
|-------|-----------------|------------------------------|
| `UnidadesNegocio` | `MineraId` | `UNG_*` (4 SP: obtener listas/detalle, guardar, eliminar) |
| `Divisiones` | `UnidadNegocioId` | `DIV_*` (5) |
| `Departamentos` | `DivisionId` | `DEP_*` (5) |
| `Empresas` | `MineraId`, `UnidadNegocioId`, mandante | **Sin prefijo CRUD dedicado** en el filtro §2; ver nota abajo |
| `AreasEmpresa` | `EmpresaId`, `TurnoId` | **Sin prefijo CRUD dedicado** en catálogo §2 |

### NewRest hoy (solo lectura)

`sp_Solicitud_ListarOrganizacion` usa `UNG_ObtieneUnidadesNegocio`, `DIV_ObtieneDivisiones`, `DEP_ObtieneDepartamentos` (combos). No hay Mid de mantenimiento.

## Procedimientos almacenados maestros (extraídos)

### `LIS_*` — catálogo genérico `Listas`

| Procedimiento | Rol |
|---------------|-----|
| `LIS_ObtieneListas` | Lista ítems por minera (`@minera`) |
| `LIS_ObtenerListasPorMinera` | Por minera + `IdTabla` |
| `LIS_GuardaItemLista` | Alta/edición ítem (`@idTabla`, descripción, orden, minera, usuario) |
| `LIS_EliminaItemLista` | Baja por `idTabla` + `valor1` + minera |

### `UNG_*` / `DIV_*` / `DEP_*` — jerarquía

| Entidad | Obtener | Guardar | Eliminar | Filtros |
|---------|---------|---------|----------|---------|
| Unidad de negocio | `UNG_ObtieneUnidadesNegocio`, `UNG_ObtieneUnidadNegocio` | `UNG_GuardaUnidadNegocio` | `UNG_EliminaUnidadNegocio` | — |
| División | `DIV_ObtieneDivisiones`, `DIV_ObtieneDivision`, `DIV_ObtieneDivisionesByFilter` | `DIV_GuardaDivision` | `DIV_EliminaDivision` | `MineraId`, `UnidadNegocio`, `IsActive`, `Nombre` |
| Departamento | `DEP_ObtieneDepartamentos`, `DEP_ObtieneDepartamento`, `DEP_ObtieneDepartamentosByFilter` | `DEP_GuardaDepartamento` | `DEP_EliminaDepartamento` | `UnidadNegocio`, `Division`, `MineraId`, `IsActive` |

### `EMP_*` en el filtro §2 (no es CRUD Empresas)

En HOTELDB exportada, `EMP_*` del inventario corresponde sobre todo a **habitaciones / áreas de operación**, no a `dbo.Empresas`:

- `EMP_AsociaHabitacionEmpresa`, `EMP_EliminaHabitacionAsociada`, `EMP_ObtieneHabitacionesEmpresaContratista`, `EMP_GuardaLogAsociacionAreasEmpresa`

**Pendiente SGH 2.1.0:** localizar en hoja **Procedimientos** (o código legado) los SP de **alta/edición/baja** de `Empresas` y `AreasEmpresa` (p. ej. nombres sin prefijo `EMP_`, o `sp_*` / `USU_*` / menú Configuración).

## Matriz 41 catálogos `Listas` (plantilla)

Una fila por **catálogo** (`IdTabla` + código `Valor1` en filas tipo cabecera, o pantalla legado). Objetivo: 41 filas cerradas antes de SGH 2.1.1.

| # | `IdTabla` / `Valor1` | SP lectura | SP mutación | Mensajes as-is | Menú legado | Lote | Estado NewRest |
|---|----------------------|------------|-------------|----------------|-------------|------|----------------|
| 1 | `34` / `usrpasshist` | `Listas` + `sp_Usuario_*` | `LIS_*` (si editable) | `Docs/login-pendiente-revision-base-de-datos.md` | — | N/A (parámetro cuenta) | Reglas dominio; sin pantalla |
| 2 | `1` / *(estados solicitud)* | `Listas` en `001_sp_solicitud.sql` | `LIS_*` | Pendiente as-is | Pendiente `Menus` | 2.1.1* | Solo lectura en SP |
| 3 | `5` / *(estados reserva)* | `Listas` en `001_sp_reserva.sql` | `LIS_*` | Pendiente as-is | Pendiente | 2.1.1* | Solo lectura en SP |
| 4–41 | *(completar)* | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | | | | **Pendiente** |

### Consulta para cerrar filas 4–41

Ejecutar en HOTELDB (ajustar criterio `Tipo` según datos; en operación suele distinguirse cabecera vs detalle):

```sql
USE HOTELDB;
GO

-- Catálogos administrables (candidatos a pantalla Configuración → Listas)
SELECT
    l.IdTabla,
    l.Valor1 AS CodigoLista,
    MIN(l.Descripcion) AS DescripcionMuestra,
    MAX(CAST(l.IsEditable AS int)) AS AlgunaFilaEditable,
    COUNT(*) AS Filas
FROM dbo.Listas AS l WITH (NOLOCK)
GROUP BY l.IdTabla, l.Valor1
ORDER BY l.IdTabla, l.Valor1;

-- Cabeceras por IdTabla (si Tipo distingue definición de ítem)
SELECT DISTINCT IdTabla, Tipo, Valor1, Descripcion
FROM dbo.Listas WITH (NOLOCK)
WHERE Tipo IN ('Cabecera', 'cabecera')  /* validar valores reales en la base */
ORDER BY IdTabla;
```

Contraste con ítems de menú legado:

```sql
SELECT Id, Nombre, Link, PadreId, Ubicacion
FROM dbo.Menus WITH (NOLOCK)
WHERE Link LIKE '%Lista%' OR Nombre LIKE '%Lista%' OR Nombre LIKE '%Empresa%'
ORDER BY PadreId, Id;
```

## Criterio de cierre SGH 2.1.0

- [x] Diccionario xlsx con **Tablas** + **Procedimientos** en repo.
- [x] SP maestros `LIS_`, `UNG_`, `DIV_`, `DEP_` inventariados con parámetros (extractos).
- [ ] **41 filas** de la matriz completas (código, menú, lote).
- [ ] SP CRUD **Empresas** / **AreasEmpresa** identificados y documentados aquí.

Cuando el inventario cierre, abrir **SGH 2.1.1** (primer lote de Listas). Ver [`Docs/SGH-2.1-maestros-base.md`](../SGH-2.1-maestros-base.md).