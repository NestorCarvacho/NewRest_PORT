# Registro de actividad — SGH 1.6 Layout, errores, logging y auditoría

**Épica / ticket:** SGH 1.6 · Layout, manejo de errores, logging y auditoría de campos Fecha/Usuario  
**Fecha cierre:** 2026-11-16  
**Integración:** `main` — merge `ee2f6bd` (SGH 1.6) + documentación asociada en `3906963`  
**Rama histórica:** `cursor/sgh-1-6-layout-transversal-f6a4`  
**Estado:** **Completada**

**Referencias:** `Docs/SGH-1.6-documentacion-ticket.md`, `Docs/SGH-1.6-layout-errores-logging-auditoria.md`, `Docs/prompt-proyecto/layout-escritorio.md`, `errores-y-logging.md`, `auditoria-sp.md`

---

## Descripción

Se elaboró e implementó la infraestructura transversal **SGH 1.6** en NewRest_PORT como guía operativa oficial para módulos migrados: **layout general** del escritorio (layouts canónicos), **manejo centralizado de errores** en POST Mid y páginas Razor, **logging** con namespace `SGH` y **auditoría** de campos `FechaCreacion`, `FechaActualizacion`, `UsuarioCreacionId` y `UsuarioActualizacionId` mediante convención documentada y helper `AuditoriaPersistencia`. El trabajo está **mergeado en `main`** y cubre correlación `X-Correlation-Id`, `EjecutorPeticionMid` en los cuatro controladores Mid y `_LayoutMinimo` para estados de error.

## Objetivo de la documentación

Dejar un registro formal estándar (gestión de proyectos y onboarding) que describa qué se entregó, cómo debe usarlo el equipo en desarrollos futuros y qué queda explícitamente fuera del alcance, sin ambigüedad respecto al contrato JSON `registros` ni al origen del `UsuarioId` (solo sesión servidor).

## Resumen de directrices definidas

- **Layout (Clean Architecture · capa Web):** `_Layout` para operación, `_LayoutMinimo` para error/404/acceso denegado, `_LoginLayout` para cuenta; catálogo en `layout-escritorio.md`.
- **Errores:** `CorrelacionSolicitudMiddleware` + página `/Error` en español con identificador; Mid siempre vía `EjecutorPeticionMid` + `RespuestaDeMid` (sin cambiar forma del sobre JSON).
- **Logging:** niveles `SGH` en configuración; fallos Mid con `Modulo`, `Opcion`, `UsuarioId`, `CorrelacionId`; sin PII en trazas.
- **Auditoría SP:** `AuditoriaPersistencia.UsuarioDeSesion`; plantilla CUD en `auditoria-sp.md`; parámetro `@UsuarioId` desde `DatosSesion`, nunca desde JSON cliente.
- **Anti-patrones evitados:** `try/catch` duplicado en controladores Mid, mensajes SQL al usuario, auditoría sin usuario de sesión válido.

## Resultado / impacto esperado

**Única fuente de verdad** para cerrar SGH 1.6 en el gestor de incidencias (`SGH-1.6-documentacion-ticket.md` listo para copiar). Reduce deuda transversal, acelera incorporación de desarrolladores y alinea soporte (correlación log ↔ pantalla de error). Exclusiones registradas: bitácora auth HOTELDB, sink centralizado de logs, correlación dentro del JSON Mid, migración masiva de SP legado.

---

## Inventario de entregables

| Tipo | Entregable |
|------|------------|
| Código | `EjecutorPeticionMid`, `CorrelacionSolicitudMiddleware`, `_LayoutMinimo`, `AuditoriaPersistencia` |
| Config | `appsettings.json` (LogLevel `SGH`) |
| Docs equipo | `layout-escritorio.md`, `errores-y-logging.md`, `auditoria-sp.md` |
| QA | `RespuestaDeMidTests`, `AuditoriaPersistenciaTests` |

## Verificación

```bash
dotnet test SGH.sln -c Release
```

104 tests passed (Release), 2026-11-16.

---

## Anexo — Bloque único para pegar en gestor

**SGH 1.6 · Layout, manejo de errores, logging y auditoría de campos**

Implementación mergeada en `main`: layouts transversales, errores Mid centralizados, correlación de solicitudes, logging `SGH` y convención auditoría Fecha/Usuario en SP. Documentación: `Docs/SGH-1.6-documentacion-ticket.md`. Actividad **cerrada**; exclusiones: auditoría login en BD, telemetría externa, correlación en JSON Mid.