# Inventario maestros — SGH 2.1.0

**Estado:** **EN CURSO** — matriz de catálogos `Listas` **completada desde HOTELDB** (nov 2026); faltan menú legado, mensajes as-is y SP **Empresas/Áreas**.

## Fuentes versionadas

| Recurso | Contenido |
|---------|-----------|
| [`Diccionario_de_datos_HotelDB.xlsx`](Diccionario_de_datos_HotelDB.xlsx) | **Hoja `Tablas`:** ~154 tablas, metadatos de columnas. **Hoja `Procedimientos`:** ~525 SP/funciones usuario (`Esquema`, `Nombre`, `Tipo`, fechas). |
| [`diccionario-hoteldb-procedimientos-almacenados.md`](diccionario-hoteldb-procedimientos-almacenados.md) | Consultas SQL para repetir o ampliar el inventario en la instancia corporativa. |
| [`extractos/hoteldb-maestros-sp-filtrados.tsv`](extractos/hoteldb-maestros-sp-filtrados.tsv) | Resultado **§2** (117 SP del filtro maestros / operación). |
| [`extractos/hoteldb-maestros-sp-parametros.tsv`](extractos/hoteldb-maestros-sp-parametros.tsv) | Parámetros de SP `LIS_*`, `DEP_*`, `DIV_*`, `UNG_*`, `EMP_*` (subconjunto de **§3**). |
| [`extractos/listas-hotldb-cabeceras.tsv`](extractos/listas-hotldb-cabeceras.tsv) | Cabeceras `IdTabla` (`Tipo = Cabecera`) — definición de cada catálogo. |
| [`extractos/listas-hotldb-idtabla-resumen.tsv`](extractos/listas-hotldb-idtabla-resumen.tsv) | Agregado por `IdTabla` + `Valor1` (conteo filas, `IsEditable`). |

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

`he_sp_solicitud_listar_organizacion` usa `UNG_ObtieneUnidadesNegocio`, `DIV_ObtieneDivisiones`, `DEP_ObtieneDepartamentos` (combos). No hay Mid de mantenimiento.

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

## Matriz catálogos `Listas` (HOTELDB)

Epic SGH 2.1 habla de **41 catálogos**; en la base exportada hay **40 cabeceras** `Tipo = Cabecera` (`IdTabla` **1–23**, **30–44**, **50–51**). No hay filas para `IdTabla` 24–29 ni 45–49. El más numeroso es **`IdTabla = 2` (Cargos Trabajador)**: ~1 012 ítems editables.

Una fila por **`IdTabla`** (definición del catálogo). Columna **Editable** = existe al menos un detalle con `IsEditable = 1`.

| # | `IdTabla` | Catálogo | Ítems | Editable | SP lectura | SP mutación | Lote propuesto | NewRest hoy |
|---|-----------|----------|-------|----------|------------|-------------|----------------|-------------|
| 1 | 1 | Estados de Solicitud de Alojamiento | 4 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1a | Solicitud (`001_solicitud_hotldb.sql`) |
| 2 | 2 | Cargos Trabajador | 1012 | Sí | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1c | Pendiente |
| 3 | 3 | Tipos de Empresa | 4 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1b | Pendiente |
| 4 | 4 | Area Pabellón | 3 | Sí | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1b | Pendiente |
| 5 | 5 | Origen Reserva | 4 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1a | Reserva — origen (`001_reserva_hotldb.sql`) |
| 6 | 6 | Estados Reserva | 5 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1a | Reserva — estados (`EstadoTabla`) |
| 7 | 7 | Estados Cama | 3 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1a | Pendiente |
| 8 | 8 | Estados Habitación | 5 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1a | Pendiente |
| 9 | 9 | Estados Registro Desviacion (OT) | 5 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 10 | 10 | Tipos de turno | 3 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1b | Pendiente |
| 11 | 11 | Sexos | 4 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1b | Pendiente |
| 12 | 12 | Estados registro Artículos | 6 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1d | Pendiente |
| 13 | 13 | Artículos EPP | 2 | Sí | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1d | Pendiente |
| 14 | 14 | Estados Artículos EPP | 5 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1d | Pendiente |
| 15 | 15 | Estados Casilleros EPP | 5 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1d | Pendiente |
| 16 | 16 | Estados Desalojos | 5 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1f | Pendiente |
| 17 | 17 | Estados de Solicitud de Empresa | 5 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1a | Pendiente |
| 18 | 18 | Tipo Acción No Conformidad | 3 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 19 | 19 | Área No Conformidad | 2 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 20 | 20 | Estados Origen Mejora No Conformidad | 7 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 21 | 21 | Estados Solución Propuesta No Conformidad | 4 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 22 | 22 | Estados No Conformidad | 7 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 23 | 23 | Sectores Artículos | 8 | Sí | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1d | Pendiente |
| 24 | 30 | Estados de Items de Valija | 4 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1f | Pendiente |
| 25 | 31 | Estados de Entrega de Items de Valija | 5 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1f | Pendiente |
| 26 | 32 | Configuracion Bloqueo Checkin Checkout | 3 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1f | Pendiente |
| 27 | 33 | Tipos de Valija | 3 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1f | Pendiente |
| 28 | 34 | Parámetros generales | 4 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1g | `usrpasshist`, `usrtimecaduca`, `timeoutnotif` (login) |
| 29 | 35 | Tipo de comentario | 4 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1g | Pendiente |
| 30 | 36 | Estado Comentario | 4 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1g | Pendiente |
| 31 | 37 | Tipo de observación ESS | 3 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1g | Pendiente |
| 32 | 38 | Estados del proyecto | 5 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 33 | 39 | Tipo proyecto preventivo | 3 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 34 | 40 | Estados de la actividad preventiva | 8 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 35 | 41 | Origen del registro de desviación | 4 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 36 | 42 | Porcentaje de proyectos | 2 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 37 | 43 | Clasificación Actividades | 3 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1e | Pendiente |
| 38 | 44 | Informes PBI | 3 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1h | Pendiente |
| 39 | 50 | Areas Sugerencias | 5 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1h | Pendiente |
| 40 | 51 | Sistema | 3 | No | `LIS_ObtenerListasPorMinera` | `LIS_GuardaItemLista` / `LIS_EliminaItemLista` | 2.1.1h | Pendiente |

### Lotes sugeridos (SGH 2.1.1)

| Lote | `IdTabla` incluidos | Nota |
|------|---------------------|------|
| **2.1.1a** | 1, 5–8, 17 | Operación alojamiento / solicitudes |
| **2.1.1b** | 3, 4, 10, 11 | Tipos y datos de persona |
| **2.1.1c** | 2 | Cargos — volumen alto; ticket dedicado |
| **2.1.1d** | 12–15, 23 | EPP y artículos |
| **2.1.1e** | 9, 18–22, 38–43, 41–42 | OT / no conformidad / proyectos |
| **2.1.1f** | 16, 30–33, 32 | Valija, desalojos, check-in |
| **2.1.1g** | 34–37 | Parámetros y comentarios (`34` enlazado a cuenta) |
| **2.1.1h** | 44, 50, 51 | PBI, sugerencias, selector sistema |

Pendiente por fila: enlazar **`Menus`** legado y mensajes **`Docs/as-is/`**.

### Consultas SQL (repetir / auditar)

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
- [x] **40 cabeceras** `IdTabla` inventariadas (matriz + TSV); lotes 2.1.1a–h propuestos.
- [ ] Menú legado y as-is **por catálogo**; aclarar epic **41** vs huecos 24–29 / 45–49.
- [ ] SP CRUD **Empresas** / **AreasEmpresa** identificados y documentados aquí.

Cuando el inventario cierre, abrir **SGH 2.1.1** (primer lote de Listas). Ver [`Docs/SGH-2.1-maestros-base.md`](../SGH-2.1-maestros-base.md).