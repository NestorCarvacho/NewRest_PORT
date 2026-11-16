# Errores y logging (SGH 1.6)

## Correlación

- Middleware `CorrelacionSolicitudMiddleware`: guarda `SGH.CorrelacionId` y cabecera `X-Correlation-Id`.
- `/Error` muestra el mismo identificador para soporte.

## POST Mid

| Situación | HTTP | Cuerpo | Log |
|-----------|------|--------|-----|
| Validación controlador (`opcion` vacía, archivo inválido) | 200 | `registros.resultado = false` | No |
| Fallo de Mid (`EjecucionMid.Fallo`) | 200 | `registros.mensaje` | No |
| Regla dominio (`InvalidOperationException`, etc.) | 200 | mensaje de excepción | `EjecutorPeticionMid` (Error) |
| Error inesperado (producción) | 200 | mensaje genérico | `EjecutorPeticionMid` con módulo, opción, `UsuarioId`, correlación |
| Error inesperado (desarrollo) | 200 | `ex.Message` | Igual |

Implementación: `EjecutorPeticionMid` en controladores `*Controller` de Mid; no duplicar `catch` locales salvo lectura JSON del body.

## Razor / no capturado

Excepción sin manejar en producción → `UseExceptionHandler("/Error")` (layout mínimo).

404 → `/NoEncontrado` (layout mínimo).

## Logging

- Namespaces `SGH` en `appsettings.json`; `SGH.Web.Mid.EjecutorPeticionMid` en Warning (producción).
- No registrar RUT, contraseñas ni hashes.
- Incluir `Modulo`, `Opcion`, `UsuarioId`, `CorrelacionId` en fallos Mid.

Ver [`flujo-mvc-mid.md`](flujo-mvc-mid.md) para el contrato `registros`.