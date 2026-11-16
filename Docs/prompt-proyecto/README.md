# Paquete de prompt — arquitectura SGH / NewRest

Carpeta **autocontenida** para compartir con el equipo y usar como **instrucción principal** en proyectos .NET del ecosistema hotelero (migración MVC + Mid).

## Cómo usarla

| Herramienta | Qué hacer |
|-------------|-----------|
| **Cursor** | Copiar `cursor/flujo-mvc-mid.mdc` a `.cursor/rules/` del proyecto. En *Project Rules* o el chat, indicar: «Seguí `Docs/prompt-proyecto/PROMPT-PRINCIPAL.md`». |
| **GitHub Copilot** | Copiar el contenido de `PROMPT-PRINCIPAL.md` + `directrices-desarrollo.md` a `.github/copilot-instructions.md`, o enlazar con rutas relativas a esta carpeta. |
| **Chat (Claude, etc.)** | Adjuntar **`PROMPT-PRINCIPAL.md`** al inicio del hilo; para migrar una pantalla, adjuntar también **`flujo-mvc-mid.md`**. |
| **Proyecto nuevo** | Copiar **toda la carpeta** `prompt-proyecto` al repo (`Docs/prompt-proyecto`). Seguir **`arquitectura-base.md`** §8. |

## Contenido de la carpeta

| Archivo | Rol |
|---------|-----|
| **`PROMPT-PRINCIPAL.md`** | Texto corto para pegar como system / instrucción base (léelo primero). |
| **`arquitectura-base.md`** | Capas, Mid, contrato JSON, sesión, checklists, módulos de referencia. |
| **`flujo-mvc-mid.md`** | Recorrido pantalla a pantalla (archivos, rutas, controlador, repositorio, JS). |
| **`autorizacion-rbac.md`** | Políticas HTTP, perfiles, alcance de campamento. |
| **`directrices-desarrollo.md`** | Capas, datos, seguridad, estilo C#, pruebas. |
| **`layout-escritorio.md`** | Layouts `_Layout`, `_LayoutMinimo`, `_LoginLayout` (SGH 1.6). |
| **`errores-y-logging.md`** | Correlación, `EjecutorPeticionMid`, matriz Mid vs Razor (SGH 1.6). |
| **`auditoria-sp.md`** | Columnas Fecha/Usuario y plantilla CUD (SGH 1.6). |
| **`biblioteca-componentes-ui.md`** | View Components, plantillas y `biblioteca-ui.js` (SGH 1.7). |
| **`cursor/flujo-mvc-mid.mdc`** | Misma regla que `flujo-mvc-mid.md`, formato Cursor (`alwaysApply`). |

## Repo de referencia

Implementación viva: repositorio **NewRest_PORT** — proyectos `SGH.Domain`, `SGH.Application`, `SGH.Infrastructure`, `SGH.Web`, `SGH.Tests`.

Reglas de negocio por módulo (legado): en cada repo, carpeta `Docs/as-is/` (no va en este paquete; es específica del producto).

## Mantenimiento

Si cambia el contrato `registros`, el Mid o la sesión, actualizar aquí y volver a compartir la carpeta (o sincronizar con el repo central).
