# Biblioteca de componentes UI (SGH 1.7)

Complemento de [`layout-escritorio.md`](layout-escritorio.md).

## Cuándo usar Razor vs JS

| Caso | Usar |
|------|------|
| Página de módulo vacía que carga escritorio | `@await Component.InvokeAsync("ModuloEscritorio", ...)` vía `_ModuloPagina` |
| Listado renderizado en servidor | `TablaListado` |
| Formulario estático (cuenta, legal) | `CampoFormulario` |
| Listado dinámico Mid (mayoría hoy) | Plantilla módulo + `tablas.js` |

## Invocar componentes

```cshtml
@await Component.InvokeAsync("TablaListado", new TablaListadoVm {
    ContenedorId = "tabla-ejemplo",
    Columnas = new[] { "Id", "Nombre", "Estado" }
})
```

```cshtml
@await Component.InvokeAsync("CampoFormulario", new CampoFormularioVm {
    Nombre = "descripcion",
    Etiqueta = "Descripción",
    Requerido = true,
    MaxLength = 200
})
```

## JavaScript

```javascript
await window.SGHBibliotecaUi.confirmar("¿Confirma la acción?", { titulo: "Confirmar" });
```

Atributos: `data-sgh-tabla`, `data-sgh-modal`, `data-sgh-campo`, `data-sgh-validacion`, `data-sgh-modulo`.

## Paginación

La paginación de listados operativos la provee **DataTables** (`tablas.js`). El componente `TablaListado` marca `data-sgh-paginacion="cliente"` para inicialización futura unificada.