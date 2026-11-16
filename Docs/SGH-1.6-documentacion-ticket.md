# Ticket · SGH 1.6 — Layout, manejo de errores, logging y auditoría de campos

Documento para copiar al gestor de incidencias (Jira, Linear, Azure DevOps).  
**Estado:** implementado y mergeado en `main` (merge `ee2f6bd` / `3906963`, 2026-11-16).  
Rama histórica: `cursor/sgh-1-6-layout-transversal-f6a4`.

---

## Resumen (descripción del ticket)

**Objetivo:** Construir la **infraestructura transversal** que reutilizan los módulos migrados: shell de escritorio (layouts), **manejo centralizado de errores** (Razor y POST Mid), **logging** coherente y **auditoría** de columnas `Fecha*` / `Usuario*` en HOTELDB.

**Problema que resolvía:** `try/catch` y `ILogger` duplicados en cada controlador Mid, páginas de error genéricas en inglés, sin correlación solicitud–log, y sin convención única para pasar `UsuarioId` a SP en CUD.

**Solución entregada:** `_LayoutMinimo`, `CorrelacionSolicitudMiddleware`, `EjecutorPeticionMid`, niveles `SGH` en configuración, `AuditoriaPersistencia` y guías en `Docs/prompt-proyecto/`.

---

## Texto estándar para registro de actividad (copiar)

**Descripción:**  
Se elaboró e implementó la infraestructura transversal SGH 1.6 en NewRest_PORT: layout mínimo para páginas de estado, correlación de solicitudes (`X-Correlation-Id`), ejecutor central de excepciones en POST Mid (`EjecutorPeticionMid`), política de logging por namespace `SGH` y convención de auditoría Fecha/Usuario en procedimientos almacenados (`AuditoriaPersistencia` + plantilla SQL). El trabajo quedó integrado en `main` sin modificar el contrato JSON `registros` del patrón MVC+Mid.

**Objetivo de la documentación:**  
Proporcionar al equipo un marco operativo y verificable para nuevos módulos: qué layout usar, cómo fallan las APIs Mid, qué se registra en log y cómo alimentar columnas de auditoría desde `DatosSesion.UsuarioId`, evitando PII y detalle de SQL al usuario final.

**Resumen de directrices definidas:**

- **Estructura de layouts:** `_Layout` (escritorio con menú), `_LayoutMinimo` (error, 404, acceso denegado), `_LoginLayout` (cuenta). Guía: `Docs/prompt-proyecto/layout-escritorio.md`.
- **Manejo de errores Mid:** `EjecutorPeticionMid` en controladores Censo, Informes Censo, Reservas y Solicitudes; `RespuestaDeMid` intacto (HTTP 200 + `registros`). Matriz: `Docs/prompt-proyecto/errores-y-logging.md`.
- **Errores Razor:** `UseExceptionHandler("/Error")` con identificador de correlación visible; `/NoEncontrado` y `/AccesoDenegado` con layout mínimo.
- **Logging:** `LogLevel` para `SGH` y `SGH.Web.Mid.EjecutorPeticionMid` en `appsettings.json`; eventos de fallo con módulo, opción, `UsuarioId` y correlación; prohibido registrar RUT, claves o hashes.
- **Auditoría CUD:** `AuditoriaPersistencia.UsuarioDeSesion(DatosSesion)` antes de persistir; plantilla SP en `Docs/prompt-proyecto/auditoria-sp.md`. SP legado no reescritos masivamente.

**Resultado / impacto esperado:**  
Fuente de verdad técnica para cerrar el ticket SGH 1.6: menos deuda transversal, onboarding más rápido y trazabilidad soporte (id en `/Error` ↔ log). Alcance documentado con exclusiones: bitácora auth en BD, telemetría centralizada (Seq/App Insights), `correlacionId` en cuerpo JSON Mid.

---

## Alcance

| Dentro del ticket | Fuera (seguimiento aparte) |
|-------------------|----------------------------|
| `_LayoutMinimo` + guía de layouts | Rediseño visual completo del escritorio |
| `EjecutorPeticionMid` + refactor 4 controladores | Filtro global para todos los endpoints futuros |
| `CorrelacionSolicitudMiddleware` + `/Error` en español | Correlación en JSON `registros` |
| Logging `SGH` documentado | Sink externo obligatorio |
| `AuditoriaPersistencia` + plantilla SP | Reescritura masiva SP legado |
| Pruebas `RespuestaDeMidTests`, `AuditoriaPersistenciaTests` | Auditoría tabla `autenticacion` legado |

---

## Implementación técnica (referencia)

| Componente | Ruta |
|------------|------|
| Layout mínimo | `SGH.Web/Pages/Shared/_LayoutMinimo.cshtml` |
| Correlación | `SGH.Web/Infraestructura/CorrelacionSolicitudMiddleware.cs` |
| Ejecutor Mid | `SGH.Web/Mid/EjecutorPeticionMid.cs` |
| Registro DI | `SGH.Web/Program.cs` (`UseMiddleware`, `AddScoped<EjecutorPeticionMid>`) |
| Auditoría sesión | `SGH.Application/Sesion/AuditoriaPersistencia.cs` |
| Épica / entregables | `Docs/SGH-1.6-layout-errores-logging-auditoria.md` |
| Registro actividad | `Docs/actividades/SGH-1.6-layout-errores-logging-auditoria.md` |

### Layouts en uso

| Página | Layout |
|--------|--------|
| Módulos operativos | `_Layout` (default `_ViewStart`) |
| `/Error`, `/NoEncontrado`, `/AccesoDenegado` | `_LayoutMinimo` |
| Login, cambio clave, cookies, privacidad | `_LoginLayout` |

### Controladores que usan `EjecutorPeticionMid`

- `CensoController`
- `CensoInformesController`
- `ReservasController`
- `SolicitudesController`

La lectura JSON inválida del body sigue validándose en el controlador (antes del ejecutor).

---

## Despliegue / configuración

- Sin scripts SQL nuevos obligatorios para SGH 1.6.
- Revisar `Logging:LogLevel:SGH` en `appsettings.json` del entorno.
- Cabecera de respuesta `X-Correlation-Id` disponible para monitoreo perimetral.

---

## Pruebas y criterios de aceptación

```bash
dotnet test SGH.sln -c Release
```

| Criterio | Estado |
|----------|--------|
| POST Mid falla con `registros.resultado = false` y log estructurado | Cumplido |
| Error no controlado en prod → `/Error` con id de correlación | Cumplido |
| Páginas 404 / acceso denegado sin menú completo | Cumplido |
| Helper auditoría con `UsuarioId` inválido rechaza operación | Cumplido (tests) |
| Documentación `prompt-proyecto` actualizada | Cumplido |

**Última ejecución registrada:** 104 tests passed (Release).

---

## Riesgos y seguimiento

| Tema | Nota |
|------|------|
| Módulos nuevos Mid | Deben inyectar `EjecutorPeticionMid`; no copiar `catch` manual. |
| CUD nuevos | Usar plantilla `auditoria-sp.md` y `UsuarioDeSesion`. |
| Login / PageModels | Pueden seguir con logging propio; unificar en ticket futuro si aplica. |

---

## Referencias cruzadas

- Flujo Mid: `Docs/prompt-proyecto/flujo-mvc-mid.md`
- Directrices C#: `Docs/prompt-proyecto/directrices-desarrollo.md`
- Login / auditoría pendiente BD: `Docs/login-pendiente-revision-base-de-datos.md`