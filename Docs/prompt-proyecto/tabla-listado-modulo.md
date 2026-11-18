# Tabla, barra y filtros en Razor; JS enlaza y pinta datos

## Objetivo

Cada pantalla `/Modulo/Datos` define en **`.cshtml`**:

- Barra de acciones (botones reales, no `createElement`).
- Tabla con `<thead>` y `<tbody>` vacío.
- Formulario de **filtros** con `name=""` iguales al JSON del Mid.

El **JavaScript no construye la página**: enlaza `data-accion`, llena catálogos en `<select>`, POST al Mid y pinta filas con DataTables.

Los formularios de **alta/edición** (panel lateral de una fila) siguen generándose en JS por ahora; el canon de mutaciones sigue siendo POST JSON al Mid.

## Piezas por módulo

| Archivo | Contenido |
|---------|-----------|
| `Pages/{Modulo}/_Listado.cshtml` (misma carpeta que la página) | `section.modulo-anclado`, barra, tabla, `form.formulario-filtros-modulo` — **no** en `Shared/Modulos/_Panel*` |
| `Pages/{Modulo}/Datos.cshtml` | Ruta de la pantalla: `_ModuloPagina` + partial `_Listado` local |
| `Pages/Index.cshtml` | `_PlantillasEscritorio` (apunta a los mismos `_Listado` en `<template>`) |
| `encabezados-tabla-modulo.json` | Textos de `<th>` |
| `panel-modulo.js` | `data-accion` → handlers; clona plantilla o adopta panel de la página |
| `filtros-modulo.js` | Catálogo en selects, aplica filtro, envía `cargar(id)` |

## Atributos que usa el JS

| Atributo | Uso |
|----------|-----|
| `data-accion="actualizar"` \| `filtros` \| `nueva-habitacion` … | Click en barra |
| `data-exportar="excel"` \| `csv` | Exportar |
| `data-catalogo="grupos"` | Tras `prepararCatalogo`, rellena opciones desde el catálogo Mid |
| `name` en filtros | Mismos nombres que el objeto filtro del módulo |

## Flujo

1. GET Razor → HTML completo del módulo.
2. `escritorio.js` → `abrir(id)` mueve/adopta el panel, `inicializar` DataTables sobre `table.modulo-tabla`.
3. `cargar(id)` → Mid `listar`/`buscar` → `pintar` filas.
4. Filtros → se abre el `<form>` ya renderizado en el panel lateral (no se arma en JS).

## Rendimiento del escritorio

| Tema | Comportamiento |
|------|----------------|
| Censo por período | Una petición `listarrango` / `miscensosrango` (Mid itera días en servidor; ya no N POST desde el navegador). |
| Listados grandes | Mid devuelve `truncado` y `maxFilas` (5000); el JS avisa y acota la tabla. |
| Catálogos | Caché en memoria ~5 min por módulo (`catalogos.js`). |
| Cambio de solapa / nueva carga | `AbortController` cancela el `fetch` anterior; solapas del Index se mantienen. |
| Red | Hasta 2 reintentos en fallo de red (`api.js`). |

## Agregar un módulo nuevo

1. Añadir fila en `encabezados-tabla-modulo.json`.
2. Crear `Pages/{Modulo}/_Listado.cshtml` (barra + tabla + filtros).
3. `Pages/{Modulo}/Datos.cshtml` con `<partial name="_Listado" />` y entrada en `_PlantillasEscritorio.cshtml`.
4. JS: `fila()`, `columnas()` del paquete; acciones nuevas → `data-accion` en Razor y mapa en `panel-modulo.js`.

Ver también: `post-modulo-razor.md` (POST formulario al Mid), `layout-escritorio.md`.