# Tabla de listado: estructura en Razor, datos en Mid + JS

## Objetivo

- **`.cshtml`**: marca, barra de acciones (vacía al servir) y tabla con `<thead>` fijo; `<tbody>` vacío.
- **Backend (`Mid*`)**: reglas de negocio y datos (`registros`).
- **JavaScript**: POST `listar` / `buscar`, mapea filas (`fila()`), pinta con DataTables (`tablas.pintar`). Sin armar toda la página en DOM.

## Fuente única de encabezados

`SGH.Web/wwwroot/datos/encabezados-tabla-modulo.json`

| Consumidor | Uso |
|----------|-----|
| `_TablaModulo.cshtml` | Renderiza `<th>` en servidor. |
| `encabezados-tabla.js` | Paneles creados solo en cliente (Index, menú sin página dedicada). |
| `EncabezadosTablaModulo.cs` | Misma lista en Razor. |

Al agregar columna: actualizar el JSON y las funciones `columnas()` / `fila()` del módulo (definición DataTables y mapeo).

## Páginas módulo (`*/Datos.cshtml`)

1. `ViewData["ModuloId"]` = clave de solapa (`habitaciones`, `reserva`, …).
2. `_ModuloPagina` incluye `section.modulo-anclado` + `_TablaModulo`.
3. `escritorio.js` adopta el panel anclado, monta botones en `.modulo-barra` y enlaza DataTables sobre la tabla existente.

## Página índice (solo solapas)

Si no hay anclado, `panel-modulo.js` crea la misma estructura en cliente usando el JSON (comportamiento anterior, mismo aspecto).

## Formularios y reglas

- Listados: siempre vía Mid (JSON desde escritorio).
- Formularios Razor opcionales: ver `post-modulo-razor.md` (mismo Mid, sin duplicar `Reglas*` en la página).

## Archivos clave

| Pieza | Ruta |
|-------|------|
| Partial tabla | `Pages/Shared/_TablaModulo.cshtml` |
| Shell módulo | `Pages/Shared/_ModuloPagina.cshtml` |
| Tablas + pintado | `wwwroot/js/core/tablas.js` |
| Panel | `wwwroot/js/core/panel-modulo.js` |