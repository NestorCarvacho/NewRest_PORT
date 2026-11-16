# SGH 1.5 · Autorización por perfiles y menú dinámico

## Descripción / objetivo

Implementar las políticas de autorización según tablas y reglas de **`Perfiles`** / **`MenusPerfil`** del legado (ítems en **`Menus`**), y exponer un **menú dinámico por perfil** en el escritorio (equivalente funcional al sistema anterior), con **refuerzo en servidor** en páginas, controladores y POST Mid.

**Diccionario de datos:** [`Docs/datos/Diccionario_de_datos_HotelDB.xlsx`](datos/Diccionario_de_datos_HotelDB.xlsx) — extracto para este epic: [`Docs/datos/diccionario-hoteldb-perfiles-menus.md`](datos/diccionario-hoteldb-perfiles-menus.md).

**Documentación para ticket (Jira/Linear):** [`Docs/SGH-1.5-documentacion-ticket.md`](SGH-1.5-documentacion-ticket.md)

---

## Estado de implementación (rama `cursor/sgh-1-5-menu-autorizacion-f6a4`)

| Entregable | Estado |
|------------|--------|
| `sp_Menu_ObtenerPorPerfil` + `ConsultaMenuPerfil` | Implementado |
| Menú dinámico (`MenuPerfilViewComponent`) | Implementado |
| Autorización `MenuAccesoAuthorizationHandler` | Implementado |
| Tests unitarios `SGH.Tests/Navegacion` | Implementado |
| Matriz completa `Link` legado ↔ Razor | Pendiente |
| QA manual multi-perfil con HOTELDB | Pendiente |

---

## Estado previo en NewRest_PORT (antes de 1.5)

| Área | Hoy | Brecha respecto al objetivo |
|------|-----|------------------------------|
| Autorización HTTP | Políticas en `PoliticasAplicacion`: distingue solo **pasajero (`PerfilId = 2`)** vs **resto**. Ver `Docs/prompt-proyecto/autorizacion-rbac.md`. | No se discrimina por perfil 4, 5, 11, 68, 142, etc. Claims `capacidad` en cookie **sin enforcement**. |
| Menú lateral | HTML fijo en `SGH.Web/Pages/Shared/_Layout.cshtml`; la mayoría de ítems son placeholders. | No hay consulta a `Menus` / `MenusPerfil` ni filtrado por `PerfilId` de sesión. |
| UI puntual | Se ocultan «Usuarios» si `tipoPerfil = pasajero`. | No sustituye políticas ni menú completo por perfil. |
| Sesión | `perfilId`, `asignacionPerfilId`, campamento y mandante en claims (`EmisorCookieSesion`). | Origen correcto para autorizar; falta mapeo perfil → permisos / ítems de menú. |
| Alcance de datos | `AlcanceMid` / `ReglasAlcance` por campamento. | Se mantiene aparte de autorización por menú. |

---

## Alcance del entregable 1.5

### Dentro

1. **Descubrimiento AS-IS**: modelo `Perfiles`, `Menus`, `MenusPerfil`, `PerfilesUsuario` (y tablas relacionadas); procedimientos que alimentan menú y permisos en el legado (ver extracto del diccionario).
2. **Contrato de dominio**: permisos o códigos de menú atómicos; mapeo `PerfilId` → conjunto permitido (desde BD o capa que envuelva SP existentes).
3. **Autorización en servidor**: políticas o `IAuthorizationHandler` por permiso/código de menú; aplicación coherente en Razor Pages, controladores y respuestas JSON del Mid cuando la sesión no alcanza.
4. **Menú dinámico**: cargar estructura (grupos, columnas, enlaces) según sesión; integrar con buscador de módulos y solapas del escritorio; i18n vía claves existentes donde aplique.
5. **Pruebas**: unitarias de mapeo perfil→permiso; integración con perfiles representativos (pasajero, operaciones, configuración).
6. **Documentación**: actualizar `autorizacion-rbac.md` y este documento con matriz perfil → menú / política.

### Fuera (salvo decisión explícita)

- CRUD de administración de perfiles y menú en pantalla (módulo «Perfiles» del layout aún no migrado).
- ESS, `mensajeAutorizacion` y política de aceptación en login (ver `Docs/login-pendiente-revision-base-de-datos.md` § «Qué no quedó en este corte»).
- Autorización granular **dentro** de un módulo ya migrado por opción Mid (salvo que el legado lo exija por perfil en esa pantalla).

---

## Actividades

> **Nota:** Completar IDs, responsable, estimación y estado en tu herramienta de gestión. Este listado es el desglose técnico sugerido para el epic 1.5.

### A. Análisis y diseño

| ID | Actividad | Entregable |
|----|-----------|------------|
| A1 | Contrastar `Perfiles`, `Menus`, `MenusPerfil` (diccionario ya en repo) y localizar SPs/API legado que devuelven menú por `PerfilId`. | Completar § pendientes en `diccionario-hoteldb-perfiles-menus.md` + AS-IS si aplica. |
| A2 | Comparar ítems del menú fijo actual (`_Layout.cshtml`) con menú legado por perfil piloto (p. ej. 4, 68, 142). | Matriz ítem ↔ ruta Razor / URL ↔ código menú BD. |
| A3 | Definir modelo: ¿permiso por código de menú, por ruta, o ambos? ¿Una política ASP.NET por módulo o `RequireClaim` por capacidad? | ADR corto o sección en este archivo. |
| A4 | Definir comportamiento pasajero vs operadores y perfil `ideauno` (11) / sin campamento. | Reglas en dominio (`MatrizPerfiles` o sucesor). |

### B. Infraestructura y datos

| ID | Actividad | Entregable |
|----|-----------|------------|
| B1 | Exponer lectura de menú por `PerfilId` (`Menus` ⨝ `MenusPerfil`, `IsActive`) vía SP existente o envoltorio en `SGH.Infrastructure`. | Repositorio + script SQL idempotente si aplica. |
| B2 | Exponer resolución perfil → permisos / ítems si no viene en el mismo SP del menú. | Métodos en capa Cuenta/Configuración. |
| B3 | Validar en HOTELDB LocalDB con datos de prueba por perfil. | Notas en A1 o script de datos de prueba. |

### C. Aplicación — autorización

| ID | Actividad | Entregable |
|----|-----------|------------|
| C1 | Implementar handler o políticas que usen `PerfilId` / claims derivados del legado (no solo `EsPasajero`). | `SGH.Web/Autorizacion` + registro en `Program.cs`. |
| C2 | Ajustar `EmisorCookieSesion` para emitir claims alineados al diseño (p. ej. capacidades reales por perfil, no bloque fijo para todos los no-pasajeros). | Claims + pruebas. |
| C3 | Cablear `EjecutarCheckInOut` y `AdministrarUsuarios` a recursos concretos cuando existan rutas. | Atributos `[Authorize(Policy = …)]`. |
| C4 | Alinear mensajes de rechazo con legado (JSON Mid y HTML). | Reutilizar textos de `Docs/as-is/` cuando existan. |

### D. Aplicación — menú dinámico

| ID | Actividad | Entregable |
|----|-----------|------------|
| D1 | Servicio o ViewComponent que construya el árbol de menú desde BD para la sesión actual. | C# + modelo de vista. |
| D2 | Reemplazar menú estático en `_Layout.cshtml` por render dinámico; conservar estructura visual (grupos, paneles, `data-modulo`). | Layout + partial. |
| D3 | Filtrar buscador de módulos (`buscar-modulo`) con los mismos ítems visibles. | `escritorio.js` / fuente común de ítems. |
| D4 | Ocultar o deshabilitar ítems sin página migrada según regla acordada (ocultar vs visible deshabilitado). | Criterio documentado en A3. |

### E. Calidad y cierre

| ID | Actividad | Entregable |
|----|-----------|------------|
| E1 | Ampliar `PoliticasAutorizacionTests` y pruebas de menú por perfil. | `SGH.Tests`. |
| E2 | Prueba manual: login con asignaciones de distintos perfiles; verificar menú y acceso directo por URL. | Evidencia / checklist. |
| E3 | Actualizar `Docs/prompt-proyecto/autorizacion-rbac.md` y README si cambia el modelo. | Docs sincronizados. |

---

## Actividades adicionales del equipo

*(Pegar aquí las actividades que entregue el equipo — título, descripción, dependencias — para mantener una sola fuente de verdad junto al desglose A–E.)*

| ID | Título | Descripción | Depende de |
|----|--------|-------------|------------|
| | | | |

---

## Criterios de aceptación (borrador)

1. Un usuario con **perfil pasajero** ve solo los ítems de menú y puede abrir solo las URLs que el legado permite a ese perfil; el acceso directo a otras rutas devuelve no autorizado (página o JSON coherente).
2. Usuarios con **perfiles operativos distintos** (p. ej. 4 vs 68) ven menús **diferentes** cuando `MenusPerfil` lo define así en BD.
3. Las páginas y POST Mid migrados exigen la misma autorización que el menú muestra (no se confía solo en ocultar enlaces).
4. El alcance por campamento/mandante sigue saliendo de sesión en servidor; el menú no puede ampliar campamento vía cliente.
5. Documentación actualizada con matriz de referencia y procedimientos usados.

---

## Dependencias

- Base **HOTELDB** con tablas documentadas en `Docs/datos/` y SP de menú/perfiles del legado (contrastar en LocalDB o entorno aprobado).
- Login y selección campamento/perfil ya operativos (`Docs/login-pendiente-revision-base-de-datos.md`).
- Convención MVC + Mid para pantallas nuevas (`.cursor/rules/flujo-mvc-mid.mdc`).

---

## Referencias de código y datos

- Diccionario HOTELDB: `Docs/datos/Diccionario_de_datos_HotelDB.xlsx`, extracto perfiles/menú: `Docs/datos/diccionario-hoteldb-perfiles-menus.md`
- Políticas: `SGH.Web/Autorizacion/PoliticasAplicacion.cs`, `SGH.Web/Program.cs`
- Claims: `SGH.Web/Cuenta/EmisorCookieSesion.cs`
- Menú estático: `SGH.Web/Pages/Shared/_Layout.cshtml`, `SGH.Web/wwwroot/js/escritorio.js`, `wwwroot/js/core/menu-modulos.js`
- Perfiles en dominio: `SGH.Domain/Cuenta/MatrizPerfiles.cs`
- Documentación RBAC: `Docs/prompt-proyecto/autorizacion-rbac.md`