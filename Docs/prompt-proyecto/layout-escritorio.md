# Layout del escritorio (SGH 1.6)

## Layouts

| Archivo | Uso |
|---------|-----|
| `_Layout.cshtml` | Pantallas operativas autenticadas: cromo, menú por perfil, panel lateral, solapas. |
| `_LayoutMinimo.cshtml` | Error, no encontrado, acceso denegado: solo marca y versión. |
| `_LoginLayout.cshtml` | Login, cambio de clave, cookies, privacidad: tarjeta centrada. |

`_ViewStart.cshtml` asigna `_Layout` por defecto. Las páginas excepcionales deben fijar `Layout` en el `.cshtml`.

## Partials compartidos

- `MenuPerfil` — barra de módulos (SGH 1.5).
- `_SelectorIdioma`, `_SelectorTema` — preferencias.
- `FiltroPilaAvisos` — avisos tras POST de página.

Ver estilos en `wwwroot/css/escritorio.css` (clases `.estado`, `.layout-minimo-*`).