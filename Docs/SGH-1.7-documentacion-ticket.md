# Ticket · SGH 1.7 — Biblioteca de componentes compartidos (paginación, filtros, tablas, modales, validaciones)

Documento para copiar al gestor de incidencias (Jira, Linear, Azure DevOps).  
**Estado:** implementado en rama `cursor/sgh-1-7-biblioteca-ui-f6a4` (pendiente merge a `main` si aplica).  
**Épica técnica:** `Docs/SGH-1.7-biblioteca-componentes-ui.md`

---

## Resumen (descripción del ticket)

**Objetivo:** Crear la **biblioteca de componentes** reutilizable que usan los módulos del escritorio para mantener una **interfaz consistente**: tablas y paginación, filtros, modales de confirmación y validación visual de campos.

**Problema que resolvía:** Cada módulo armaba DOM con `createElement`, `window.confirm` y mensajes de error inconsistentes; no había catálogo Razor ni contrato `data-sgh-*` entre layout, CSS y JavaScript del escritorio.

**Solución entregada:** Enfoque **híbrido Razor + JS**: View Components para markup canónico, plantillas `<template>` en `_Layout`, API `biblioteca-ui.js` (`SGHBibliotecaUi`), integración con `tablas.js` (DataTables), `formularios.js` (filtros en panel lateral) y confirmaciones en módulo **Solicitud**.

---

## Texto estándar para registro de actividad (copiar)

**Descripción:**  
Se elaboró e implementó la biblioteca de componentes compartidos **SGH 1.7** en NewRest_PORT: View Components Razor (**módulo**, **tabla**, **modal**, **campo validado**), plantillas HTML en el layout (`_UiPlantillas`), estilos `biblioteca-ui.css` y cliente `biblioteca-ui.js` para shells de módulo, confirmaciones modales, validación de campos y convivencia con **paginación DataTables** y **filtros** en panel lateral. El shell de listados del escritorio clona la plantilla canónica; solicitud reemplaza `window.confirm` por el modal SGH en acciones críticas.

**Objetivo de la documentación:**  
Dar al equipo una **única referencia** para nuevas pantallas: qué componente Razor usar, qué atributos `data-sgh-*` respetar y cómo confirmar/validar sin duplicar HTML ni romper el patrón MVC+Mid (listados siguen cargándose por POST Mid).

**Resumen de directrices definidas:**

- **Paginación:** listados operativos vía `wwwroot/js/core/tablas.js` + DataTables; componente `TablaListado` marca `data-sgh-paginacion="cliente"` para tablas renderizadas en servidor.
- **Filtros:** panel lateral + `formularios.js`; mensajes bajo `[data-sgh-validacion]` y clase `campo-error`.
- **Tablas:** plantilla de módulo `#sgh-ui-plantilla-modulo` + `clonarModuloShell`; columnas definidas en paquetes JS del módulo.
- **Modales:** `ModalDialogo` global en `_Layout`; confirmar con `await window.SGHBibliotecaUi.confirmar(mensaje, { titulo })`.
- **Validaciones UI:** obligatorios en formularios dinámicos (cliente); reglas de negocio siguen en **Mid**. `CampoFormularioViewComponent` para formularios Razor estáticos.
- **Componentes Razor:** catálogo en `SGH.Web/ViewComponents/` y modelos `SGH.Web/Ui/BibliotecaUiModels.cs`.

**Resultado / impacto esperado:**  
UI homogénea en escritorio, menos deuda de markup duplicado y base para migrar más módulos y páginas servidor a View Components. **Exclusiones:** listados Mid no pasan aún 100 % server-render; un solo modal global; reserva/censo conservan `confirm` nativo hasta migración explícita.

---

## Alcance

| Dentro del ticket | Fuera (seguimiento aparte) |
|-------------------|----------------------------|
| View Components: ModuloEscritorio, TablaListado, ModalDialogo, CampoFormulario | Tag Helpers propios |
| Plantillas `_UiPlantillas` + `biblioteca-ui.js` / CSS | Sustituir DataTables por paginación solo Razor |
| `panel-modulo.js` usa shell plantilla | Reescribir todos los módulos JS |
| Validación visual en `formularios.js` | Validación de negocio en JavaScript |
| Confirmaciones modal en **solicitud** | Todos los `window.confirm` del repo |
| Guía `prompt-proyecto/biblioteca-componentes-ui.md` | Storybook / catálogo visual aparte |

---

## Catálogo de componentes

| Pieza | Tipo | Responsabilidad |
|-------|------|-------------------|
| **ModuloEscritorio** | View Component | Bienvenida + `data-modulo-inicial` (`_ModuloPagina`) |
| **TablaListado** | View Component | `<table class="tabla">` + cabeceras + `data-sgh-tabla` |
| **ModalDialogo** | View Component | Diálogo confirmar/cancelar (`data-sgh-modal`) |
| **CampoFormulario** | View Component | Campo + ayuda + `data-sgh-validacion` |
| **Plantilla módulo** | Razor `<template>` | Shell barra + contenedor tabla |
| **biblioteca-ui.js** | JS | `clonarModuloShell`, `confirmar`, `validarFormulario` |
| **tablas.js** | JS (existente) | DataTables, paginación, vacío ilustrado |
| **formularios.js** | JS (existente) | Filtros, grillas, Select2, validación obligatorios |

---

## Implementación técnica (referencia)

| Artefacto | Ruta |
|-----------|------|
| Modelos UI | `SGH.Web/Ui/BibliotecaUiModels.cs` |
| ModuloEscritorio | `ViewComponents/ModuloEscritorioViewComponent.cs` → `Pages/Shared/Components/ModuloEscritorio/Default.cshtml` |
| TablaListado | `ViewComponents/TablaListadoViewComponent.cs` → `.../TablaListado/Default.cshtml` |
| ModalDialogo | `ViewComponents/ModalDialogoViewComponent.cs` → `.../ModalDialogo/Default.cshtml` |
| CampoFormulario | `ViewComponents/CampoFormularioViewComponent.cs` → `.../CampoFormulario/Default.cshtml` |
| Plantillas | `Pages/Shared/_UiPlantillas.cshtml` |
| Layout (CSS + modal + plantillas) | `Pages/Shared/_Layout.cshtml` |
| API cliente | `wwwroot/js/core/biblioteca-ui.js` |
| Estilos | `wwwroot/css/biblioteca-ui.css` |
| Shell módulos escritorio | `wwwroot/js/core/panel-modulo.js` |
| Inicialización UI | `wwwroot/js/escritorio.js` → `window.SGHBibliotecaUi` |
| Confirmaciones (piloto) | `wwwroot/js/modulos/solicitud.js` → `confirmarUi` |

### Atributos `data-sgh-*` (contrato)

| Atributo | Uso |
|----------|-----|
| `data-sgh-modulo` | Contenedor de módulo en solapa |
| `data-sgh-barra` | Barra de acciones (Filtros, etc.) |
| `data-sgh-tabla` | Contenedor tabla / DataTables |
| `data-sgh-modal` | Raíz del modal |
| `data-sgh-campo` | Input/select/textarea validable |
| `data-sgh-validacion` | Mensaje de error del campo |

### Ejemplo Razor (tabla estática)

```cshtml
@await Component.InvokeAsync("TablaListado", new TablaListadoVm {
    ContenedorId = "tabla-ejemplo",
    Columnas = new[] { "Id", "Nombre", "Estado" }
})
```

### Ejemplo JavaScript (confirmación)

```javascript
const acepta = await window.SGHBibliotecaUi.confirmar(
    "¿Confirma la operación?",
    { titulo: "Confirmar" });
if (!acepta) return;
```

---

## Despliegue / configuración

- Sin cambios en HOTELDB ni `appsettings` obligatorios.
- Despliegue Web estándar; asegurar que `_Layout` incluye `biblioteca-ui.css` y `ModalDialogo`.
- CDN existentes: DataTables y Select2 (ya en layout).

---

## Pruebas y criterios de aceptación

```bash
dotnet test SGH.sln -c Release
```

| Criterio | Estado esperado |
|----------|-----------------|
| Módulos escritorio abren solapa con shell plantilla (solicitud, reserva, censo) | Cumplido |
| Filtros abren panel lateral y campos requeridos muestran error visual | Cumplido |
| Listado usa DataTables (paginación/búsqueda) | Sin regresión (`tablas.js`) |
| Acciones críticas en solicitud usan modal SGH | Cumplido (anular, generar, eliminar, importar) |
| View Components compilables y documentados | Cumplido |
| Tests unitarios modelos UI | Cumplido (`BibliotecaUiModelsTests`) |

**Última ejecución registrada:** 106 tests passed (Release).

### Prueba manual sugerida (QA)

1. Login → abrir **Solicitud** → **Filtros** → dejar fechas vacías → mensaje en campo.
2. Detalle solicitud pendiente → **Anular** → debe aparecer modal SGH (no diálogo nativo del navegador).
3. Verificar listado con paginación DataTables al pie de la grilla.

---

## Riesgos y seguimiento

| Tema | Nota |
|------|------|
| Modal único | No encadenar dos confirmaciones simultáneas |
| Nuevos módulos | Usar plantilla módulo; no recrear `section.modulo` a mano |
| Páginas solo Razor | Preferir `CampoFormulario` y `TablaListado` |
| Reserva / Censo | Migrar `window.confirm` en ticket posterior si aplica |

---

## Referencias cruzadas

- Guía equipo: `Docs/prompt-proyecto/biblioteca-componentes-ui.md`
- Layout escritorio: `Docs/prompt-proyecto/layout-escritorio.md`
- Registro actividad: `Docs/actividades/SGH-1.7-biblioteca-componentes-ui.md`
- Flujo Mid (datos listado): `Docs/prompt-proyecto/flujo-mvc-mid.md`