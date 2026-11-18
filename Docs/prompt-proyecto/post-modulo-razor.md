# POST de formulario Razor → mismo Mid que el escritorio

## Cuándo usar

- Formularios HTML en Razor (`method="post"`) que deben aplicar las **mismas reglas** que el JSON del Mid.
- El escritorio (`escritorio.js`) sigue usando POST JSON al controlador `*/Mid/*.cs`.

## Piezas

| Archivo | Rol |
|---------|-----|
| `ConversorFormularioMid` | `IFormCollection` → `FiltroPeticion` (camelCase en nombres de campo, igual que el JS). |
| `EjecutorPostModulo` | Valida `opcion`, sesión, llama al Mid, `TempData` + redirect. |
| `ModuloDatosPageModel` | Base con `OnPostAsync` para páginas `*/Datos.cshtml`. |

## Formulario Razor

```html
<form method="post" asp-page="/Habitacion/Datos">
    @Html.AntiForgeryToken()
    <input type="hidden" name="opcion" value="editarbano" />
    <input name="id" value="12" />
    <input name="codigoNombre" value="B-01" />
    <!-- mismos nombres que el JSON del Mid -->
    <button type="submit">Guardar</button>
</form>
```

## Reglas de negocio

- **Solo** en `SGH.Domain` (`Reglas*`) y en Mid / casos de uso.
- El `PageModel` **no** duplica validaciones; solo enruta al Mid.

## Páginas con `OnPost`

- `Habitacion/Datos`, `Habitacion/Banos`
- `Reserva/Datos`
- `Censo/Datos`
- `Solicitud/Datos`

Mensajes tras el POST: `TempData` + partial `_AvisosModuloPost` (integrado en `_ModuloPagina`).