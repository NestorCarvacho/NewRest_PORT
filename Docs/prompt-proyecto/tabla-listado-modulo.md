# Tabla, barra y filtros en Razor; JS enlaza y pinta datos

## Objetivo

Cada pantalla `/Modulo/Datos` define en **`.cshtml`**:

- Barra de acciones (botones reales, no `createElement`).
- Tabla con `<thead>` y `<tbody>` vacío.
- Formulario de **filtros** con `name=""` iguales al JSON del Mid.

El **JavaScript no construye la página**: enlaza `data-accion`, llena catálogos en `<select>`, POST al Mid y pinta filas con DataTables.

Los formularios de **alta/edición** (panel lateral de una fila) siguen generándose en JS por ahora; pueden migrarse a partials como en `Habitacion/_ModuloHabitacionBanos.cshtml`.

## Piezas por módulo

| Archivo | Contenido |
|---------|-----------|
| `Pages/Shared/Modulos/_Panel{Modulo}.cshtml` | `section.modulo-anclado`, barra, tabla, `form.formulario-filtros-modulo` |
| `Pages/{Modulo}/Datos.cshtml` | `_ModuloPagina` + partial del panel |
| `Pages/Index.cshtml` | `_PlantillasEscritorio` (mismos paneles en `<template>` para menú/solapas) |
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

## Agregar un módulo nuevo

1. Añadir fila en `encabezados-tabla-modulo.json`.
2. Crear `_PanelNuevo.cshtml` (barra + tabla + filtros).
3. Página `Datos.cshtml` y entrada en `_PlantillasEscritorio.cshtml`.
4. JS: `fila()`, `columnas()` del paquete; acciones nuevas → `data-accion` en Razor y mapa en `panel-modulo.js`.

Ver también: `post-modulo-razor.md` (POST formulario al Mid), `layout-escritorio.md`.