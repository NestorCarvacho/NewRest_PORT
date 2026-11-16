# Ticket · SGH 1.7 — Biblioteca de componentes compartidos

Documento para copiar al gestor de incidencias.

---

## Texto estándar para registro de actividad

**Descripción:**  
Se implementó la biblioteca de componentes compartidos SGH 1.7 en NewRest_PORT: **View Components Razor** (módulo, tabla, modal, campo validado), **plantillas HTML** en el layout y la API **`biblioteca-ui.js`** para paginación vía DataTables, filtros en panel lateral, modales de confirmación y validación visual de formularios. Los módulos del escritorio (solicitud, reserva, censo) reutilizan el shell canónico sin duplicar markup.

**Objetivo de la documentación:**  
Garantizar interfaz consistente entre módulos con un catálogo único (Razor + `data-sgh-*`) y reducir `window.confirm` / DOM ad hoc.

**Resumen de directrices definidas:**  
(1) Usar View Components para markup nuevo en servidor. (2) Módulos JS deben clonar `#sgh-ui-plantilla-modulo`. (3) Confirmaciones con `SGHBibliotecaUi.confirmar`. (4) Campos obligatorios muestran `[data-sgh-validacion]`. (5) Tablas operativas siguen `tablas.js` + DataTables.

**Resultado / impacto esperado:**  
Biblioteca documentada y cableada al layout y al módulo solicitud (confirmaciones). Base para migrar pantallas restantes a componentes Razor donde aplique.

---

## Entregables

- `SGH.Web/Ui/BibliotecaUiModels.cs`
- View Components + `Pages/Shared/Components/*/Default.cshtml`
- `_UiPlantillas.cshtml`, `biblioteca-ui.css`, `biblioteca-ui.js`
- Tests: `SGH.Tests/Ui/BibliotecaUiModelsTests.cs`

## Pruebas

`dotnet test SGH.sln -c Release`