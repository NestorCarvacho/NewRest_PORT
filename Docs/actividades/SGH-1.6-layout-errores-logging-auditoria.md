# Registro de actividad — SGH 1.6 Layout, errores, logging y auditoría

**Épica / ticket:** SGH 1.6 · Layout, manejo de errores, logging y auditoría de campos Fecha/Usuario  
**Fecha:** 2026-11-16  
**Rama de referencia (planificación):** `cursor/sgh-1-6-layout-transversal-f6a4`  
**Referencias en repo:** `Docs/SGH-1.6-layout-errores-logging-auditoria.md`, `SGH.Web/Pages/Shared/_Layout.cshtml`, `SGH.Web/Mid/RespuestaDeMid.cs`, `SGH.Application/Sesion/DatosSesion.cs`

---

## Descripción

Se definió y documentó la épica **SGH 1.6** como infraestructura transversal para NewRest_PORT: **layout general del escritorio**, **manejo centralizado de errores** (Razor y POST Mid), **logging** con criterios de seguridad y **auditoría** de campos `FechaCreacion`, `FechaActualizacion`, `UsuarioCreacionId` y `UsuarioActualizacionId` (y alias legados) en persistencia vía procedimientos almacenados. El registro parte del **estado real del código** tras SGH 1.5 (menú en layout, sesión con `UsuarioId`) y fija actividades A–E para implementación futura sin romper el contrato JSON `registros` del patrón MVC+Mid.

## Objetivo de la documentación

Proporcionar al equipo un **marco único** para que cada módulo migrado reutilice las mismas reglas transversales: qué layout usar, cómo propagar errores al usuario y al log, qué datos de sesión alimentan la auditoría en SQL Server y qué queda explícitamente fuera del ticket (p. ej. bitácoras de negocio o cambio del sobre `registros`).

## Resumen de directrices definidas

- **Layout:** Shell en `_Layout.cshtml` (cromo, `MenuPerfil`, contenido, panel lateral, tema/idioma); definir layout mínimo para páginas anónimas y matriz de partials obligatorios (actividades B1–B3).
- **Errores:** Producción usa `UseExceptionHandler("/Error")` y `UseStatusCodePagesWithReExecute("/NoEncontrado")`; APIs Mid usan `RespuestaDeMid` con HTTP 200 y `registros.resultado = false`; objetivo C1–C3 = centralizar `DeExcepcion` y correlación log ↔ `RequestId`.
- **Logging:** Configuración base en `appsettings.json`; extender con niveles por `SGH.*`, logs estructurados con `UsuarioId`/`CampamentoId`/`opcion` Mid, sin PII (RUT, contraseña, hash).
- **Auditoría:** El `UsuarioId` efectivo sale de **`LectorSesion` / `DatosSesion`**, no del JSON del navegador; en SP, alta y actualización deben seguir convención HOTELDB documentada; autenticación global en BD sigue pendiente según `login-pendiente-revision-base-de-datos.md`.
- **Persistencia:** 100 % SP; nuevos CUD incluyen parámetros de auditoría acordados; mensajes al usuario no exponen detalle de proveedor SQL (regla existente del Mid).

## Resultado / impacto esperado

Una **hoja de ruta técnica** para SGH 1.6 que reduce divergencia entre módulos (menos `catch` duplicados, mismos campos de auditoría, mismo shell de UI) y acelera revisiones de QA y seguridad. Sirve como texto listo para ticket Jira/Linear y como índice de entregables en `Docs/SGH-1.6-layout-errores-logging-auditoria.md`.

---

## Anexo — Texto para gestor de incidencias (copiar)

**Descripción:**  
Construir la infraestructura transversal del proyecto NewRest_PORT: layout general del escritorio, manejo centralizado de errores (páginas y POST Mid), política de logging y auditoría de campos Fecha/Usuario en HOTELDB, alineado con Clean Architecture y el patrón MVC+Mid.

**Objetivo:**  
Garantizar que todos los desarrollos futuros compartan el mismo shell UI, la misma forma de fallar de forma segura y trazable, y el mismo contrato de auditoría en procedimientos almacenados, usando el `UsuarioId` de sesión del servidor.

**Resumen de directrices:**  
(1) Layout canónico y excepciones documentadas. (2) Errores centralizados respetando JSON `registros`. (3) Logging estructurado sin secretos. (4) Auditoría CUD vía SP con convención `Fecha*` / `Usuario*`. (5) Inventario AS-IS y criterios de aceptación en repositorio.

**Resultado / impacto esperado:**  
Menor deuda transversal, onboarding más rápido y trazabilidad operativa entre interfaz, log y columnas de auditoría en base de datos.