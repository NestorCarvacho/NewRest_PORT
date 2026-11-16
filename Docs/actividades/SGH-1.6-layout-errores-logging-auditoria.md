# Registro de actividad — SGH 1.6 Layout, errores, logging y auditoría

**Épica / ticket:** SGH 1.6 · Layout, manejo de errores, logging y auditoría de campos Fecha/Usuario  
**Fecha:** 2026-11-16  
**Rama:** `cursor/sgh-1-6-layout-transversal-f6a4`  
**Estado:** **Completada** (implementación MVP + documentación; exclusiones explícitas abajo)

**Referencias:** `Docs/SGH-1.6-layout-errores-logging-auditoria.md`, `Docs/prompt-proyecto/layout-escritorio.md`, `errores-y-logging.md`, `auditoria-sp.md`

---

## Descripción

Se implementó la infraestructura transversal SGH 1.6 en NewRest_PORT: **layout mínimo** para páginas de estado, **middleware de correlación**, **ejecutor central de errores** en controladores Mid (`EjecutorPeticionMid`), **política de logging** por namespace `SGH`, helper **`AuditoriaPersistencia`** y guías en `prompt-proyecto` para layout, errores y plantilla de auditoría en SP.

## Objetivo de la documentación

Dejar constancia verificable de qué quedó operativo en código y qué sigue fuera de alcance, para cerrar el ticket y orientar módulos nuevos sin repetir `try/catch` ni ambigüedad en columnas Fecha/Usuario.

## Resumen de directrices definidas

- **Layouts:** `_Layout` (operación), `_LayoutMinimo` (error/404/acceso denegado), `_LoginLayout` (cuenta).
- **Errores Mid:** un solo `catch` vía `EjecutorPeticionMid`; contrato `registros` sin cambios.
- **Correlación:** `X-Correlation-Id` y mismo id en `/Error`.
- **Logging:** niveles `SGH` en `appsettings`; eventos de fallo Mid con módulo, opción, `UsuarioId` y correlación; sin PII.
- **Auditoría:** `AuditoriaPersistencia.UsuarioDeSesion` + plantilla SQL documentada; SP legado no reescritos en bloque.

## Resultado / impacto esperado

La épica puede marcarse **completa** para el alcance acordado: transversal en Web/Application documentada y probada (`dotnet test`). Quedan **exclusiones conscientes:** bitácora de autenticación en HOTELDB, sink externo de logs, correlación dentro del JSON Mid, migración masiva de SP a la plantilla.

---

## Verificación

```bash
dotnet test SGH.sln -c Release
```

| Prueba añadida | Qué valida |
|----------------|------------|
| `RespuestaDeMidTests` | Mensajes genéricos vs reglas de dominio |
| `AuditoriaPersistenciaTests` | `UsuarioId` de sesión obligatorio |

---

## Anexo — Texto para gestor (actividad cerrada)

**Descripción:** Implementación de layout transversal, manejo centralizado de errores Mid, correlación de solicitudes, logging estructurado por namespace y convención de auditoría Fecha/Usuario para SP.

**Resultado:** Entregado en código y documentación en repo; pruebas automatizadas en verde. Auditoría de login y telemetría centralizada quedan para tickets posteriores.