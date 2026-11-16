# Épica · SGH 1.7 — Biblioteca de componentes compartidos

**Estado:** implementado en `cursor/sgh-1-7-biblioteca-ui-f6a4`  
**Ticket:** `Docs/SGH-1.7-documentacion-ticket.md`

## Resumen

Biblioteca **híbrida Razor + JavaScript** para UI consistente: View Components para markup canónico, plantillas `<template>` en layout, y `biblioteca-ui.js` para modales, validación y shells de módulo.

## Componentes Razor (View Components)

| Componente | Uso | Ubicación |
|------------|-----|-----------|
| `ModuloEscritorio` | Bienvenida módulo (`_ModuloPagina`) | `ViewComponents/ModuloEscritorioViewComponent.cs` |
| `TablaListado` | Tabla + cabeceras (servidor o referencia) | `TablaListadoViewComponent.cs` |
| `ModalDialogo` | Confirmaciones (layout global) | `ModalDialogoViewComponent.cs` |
| `CampoFormulario` | Campo con `data-sgh-validacion` | `CampoFormularioViewComponent.cs` |

Modelos: `SGH.Web/Ui/BibliotecaUiModels.cs`

## Cliente (`data-sgh-*`)

| API | Función |
|-----|---------|
| `clonarModuloShell(id)` | Panel de módulo (tabla + barra) desde plantilla Razor |
| `confirmar(mensaje, { titulo })` | Modal SGH (fallback `window.confirm`) |
| `validarFormulario(form)` | Campos `[data-sgh-campo]` |
| `inicializarModales()` | Enlaza botones del modal global |

Expuesto en escritorio como `window.SGHBibliotecaUi`.

## Integración actual

- **Módulos operativos:** `panel-modulo.js` clona `#sgh-ui-plantilla-modulo`.
- **Tablas / paginación:** `tablas.js` (DataTables) sin cambio de contrato.
- **Filtros:** panel lateral + `formularios.js` con mensajes `sgh-ui-validacion`.
- **Modales:** `ModalDialogo` en `_Layout`; solicitud usa `confirmarUi` en acciones destructivas.

## Guía

`Docs/prompt-proyecto/biblioteca-componentes-ui.md`