# Épica · SGH 1.6 — Layout, manejo de errores, logging y auditoría de campos

Documento de planificación y registro para gestor de incidencias. Complementa [`prompt-proyecto/directrices-desarrollo.md`](prompt-proyecto/directrices-desarrollo.md) y el flujo Mid en [`prompt-proyecto/flujo-mvc-mid.md`](prompt-proyecto/flujo-mvc-mid.md).

---

## Resumen

**Objetivo:** Construir la **infraestructura transversal** que todo módulo migrado reutiliza: shell de escritorio (layout), **manejo centralizado de errores**, **logging** coherente y **auditoría** de columnas `Fecha*` / `Usuario*` al persistir en HOTELDB.

**Problema que aborda:** Hoy el escritorio funciona, pero las piezas transversales crecieron por módulo: `RespuestaDeMid` y `try/catch` repetidos en controladores, `ILogger` puntual, páginas de error mínimas y auditoría repartida entre procedimientos (`@UsuarioId`, `UsuarioCreacionId`, convenciones distintas) sin una guía única para nuevos SP y pantallas.

**Depende de:** Login y sesión (`UsuarioId` en claims / `DatosSesion`); menú dinámico SGH 1.5 en `_Layout` (`MenuPerfil`).

**Estado (2026-11-16):** **Implementado** en rama `cursor/sgh-1-6-layout-transversal-f6a4`. Pendiente fuera de alcance: bitácora de autenticación en BD, sink centralizado de logs, reescritura masiva de SP legado.

---

## Entregables implementados

| ID | Entregable | Ubicación |
|----|------------|-----------|
| B1 | Layout mínimo (error / 404 / acceso denegado) | `Pages/Shared/_LayoutMinimo.cshtml` |
| B2 | Guía de layouts | `Docs/prompt-proyecto/layout-escritorio.md` |
| C1 | Ejecutor central de excepciones Mid | `Mid/EjecutorPeticionMid.cs` + controladores |
| C2 | Correlación solicitud | `Infraestructura/CorrelacionSolicitudMiddleware.cs`, `/Error` |
| C3 | Matriz errores/logging | `Docs/prompt-proyecto/errores-y-logging.md` |
| D1 | Niveles `SGH` en configuración | `appsettings.json`, `appsettings.Development.json` |
| E1 | Helper auditoría sesión | `Application/Sesion/AuditoriaPersistencia.cs` |
| E2 | Plantilla SP | `Docs/prompt-proyecto/auditoria-sp.md` |
| E3 | Pruebas | `SGH.Tests/Web/RespuestaDeMidTests.cs`, `Sesion/AuditoriaPersistenciaTests.cs` |

---

## Estado actual (AS-IS en NewRest_PORT)

| Ámbito | Qué hay | Brecha |
|--------|---------|--------|
| **Layout** | `_Layout` (escritorio), `_LayoutMinimo` (estado/error), `_LoginLayout` (cuenta). Guía en `layout-escritorio.md`. | Nuevas pantallas deben declarar layout explícitamente si no usan `_ViewStart`. |
| **Errores HTTP** | Correlación + `/Error` en español con layout mínimo. | JSON Mid no incluye aún `correlacionId` en el cuerpo (solo cabecera HTTP). |
| **Errores Mid** | `EjecutorPeticionMid` en los cuatro controladores Mid. | Lectura JSON inválida sigue en controlador (validación local). |
| **Logging** | Política `SGH` documentada; fallos Mid con módulo/opción/usuario/correlación. | Login y otros flujos fuera de Mid pueden seguir con logs propios. |
| **Auditoría BD** | `AuditoriaPersistencia` + plantilla SP; SP existentes sin cambio masivo. | Migrar llamadas gradualmente a `UsuarioDeSesion`; auditoría auth en BD pendiente. |

---

## Alcance

| Dentro de SGH 1.6 | Fuera (otros tickets) |
|-------------------|------------------------|
| Estándar de layout compartido y documentación de partials | Rediseño visual completo (solo consolidar y reglas) |
| Pipeline de excepciones para Razor + POST Mid | Cambiar contrato `registros` del Mid |
| Política de logging (niveles, campos, secretos) | Plataforma centralizada (Seq, Application Insights) si no está en entorno |
| Guía + plantilla para auditoría en SP nuevos | Reescribir todos los SP legado existentes |
| Opcional: filtro/middleware que unifique `try/catch` Mid | Auditoría de negocio tipo bitácora por entidad (tablas de log dedicadas) |

---

## Actividades propuestas

### A · Descubrimiento y convención

| ID | Actividad | Entregable |
|----|-----------|------------|
| A1 | Inventariar partials, layouts y páginas anónimas vs autenticadas. | Matriz en este doc o `Docs/actividades/`. |
| A2 | Inventariar columnas de auditoría en diccionario HOTELDB (muestra por módulo migrado). | Extracto en `Docs/datos/` o anexo. |
| A3 | Contrastar mensajes de error visibles al usuario vs `Docs/as-is/` (sin filtrar secretos ni detalle SQL en prod). | Checklist QA. |

### B · Layout

| ID | Actividad | Entregable |
|----|-----------|------------|
| B1 | Definir `_Layout` canónico y `_LayoutMinimo` (login, error, acceso denegado). | `Pages/Shared/` + sección en `Docs/prompt-proyecto/`. |
| B2 | Documentar slots: menú, solapas, panel lateral, avisos (`FiltroPilaAvisos`). | Guía para nuevas pantallas MVC+Mid. |
| B3 | Alinear accesibilidad y versión (`Aplicacion:Version`) en cromo. | Criterio de aceptación visual. |

### C · Manejo de errores

| ID | Actividad | Entregable |
|----|-----------|------------|
| C1 | Centralizar envoltorio Mid (filtro o middleware) que aplique reglas de `RespuestaDeMid` sin copiar `try/catch`. | `SGH.Web` + tests. |
| C2 | Enriquecer `/Error` y respuestas JSON con id de correlación compartido con logs. | `Error.cshtml.cs`, cabecera opcional. |
| C3 | Documentar matriz: excepción → HTTP → cuerpo (página vs `registros`). | `Docs/prompt-proyecto/flujo-mvc-mid.md` o anexo. |

### D · Logging

| ID | Actividad | Entregable |
|----|-----------|------------|
| D1 | `appsettings` + Development: niveles por namespace (`SGH.*`). | Configuración en repo. |
| D2 | Helper o convención de logs estructurados en Mid/controlador: `Opcion`, `UsuarioId`, `CampamentoId` (sin RUT/clave). | Guía en directrices. |
| D3 | Registrar fallos de SP / conexión con mensaje de `LIB_ControlResultado` solo en servidor. | Alineado con regla Mid (no nombrar motor al usuario). |

### E · Auditoría Fecha / Usuario

| ID | Actividad | Entregable |
|----|-----------|------------|
| E1 | Convención obligatoria: alta = `FechaCreacion` + `UsuarioCreacionId`; mutación = `FechaActualizacion` + `UsuarioActualizacionId`; origen = `DatosSesion.UsuarioId`, nunca JSON cliente. | Doc + ejemplo en `001_sp_*.sql`. |
| E2 | Plantilla de parámetros SP para CUD migrados. | Snippet en `Docs/prompt-proyecto/`. |
| E3 | Prueba de regresión: crear/editar en un módulo piloto y verificar columnas en BD. | Evidencia QA. |

---

## Criterios de aceptación (borrador)

1. Toda pantalla operativa migrada usa el layout documentado o la excepción explícita (login, error, legal).
2. Ningún controlador Mid nuevo duplica bloques `catch` completos sin usar el mecanismo central de SGH 1.6.
3. Un error no controlado en producción muestra página genérica; el mismo incidente deja traza en log con correlación buscable.
4. Los POST Mid siguen respondiendo HTTP 200 y `registros.resultado = false` según canon MVC+Mid.
5. Los SP nuevos o tocados en el alcance reciben auditoría desde `UsuarioId` de sesión con la convención de nombres acordada.
6. Documentación actualizada en `Docs/actividades/` y, si aplica, `Docs/prompt-proyecto/`.

---

## Referencias

- Registro formal de actividad: [`actividades/SGH-1.6-layout-errores-logging-auditoria.md`](actividades/SGH-1.6-layout-errores-logging-auditoria.md)
- Login y auditoría pendiente: [`login-pendiente-revision-base-de-datos.md`](login-pendiente-revision-base-de-datos.md)
- Directrices código: [`prompt-proyecto/directrices-desarrollo.md`](prompt-proyecto/directrices-desarrollo.md) («log en excepciones manejadas»)