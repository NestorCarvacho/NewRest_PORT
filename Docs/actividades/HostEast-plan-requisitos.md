# Plan de implementación — Requisitos HostEast

Documento para revisión previa a ejecución. Cada requisito incluye estado en NewRest_PORT, brecha, tareas, criterios de aceptación, dependencias y prioridad.

## Estado de implementación (rama `cursor/hosteast-requisitos-f6a4`)

| # | Estado |
|---|--------|
| 1 Offline | Hecho: cola unificada, censo QR en cola, banner con **Sincronizar datos**, sync con antiforgery |
| 2 Camas | Hecho: campo **Estado** 1/0 en UI |
| 3 Sesión 4 h | Hecho: 240 min, cookie persistente, `ExpireTimeSpan` |
| 4 Fechas solicitud | Hecho: UI + validación servidor; SQL check-in opcional |
| 5 Layout asignación | Hecho: columnas en CSS + markup |
| 6 Color generar | Hecho: clase `boton-generar-reservas` |
| 7 Correo prefill | Hecho: `he_sp_solicitud_contacto_correo` + catálogo |
| 8 Generar sin hab. | Hecho: UI + Mid + `GestionSolicitudes` |
| 9 Nº reserva pasajero | Hecho: SP + columna en detalle |
| 10 Anular reserva | Doc: `Docs/as-is/reserva-anular-legacy.md` |
| 11 Finalizar | Doc: `Docs/as-is/reserva-finalizar-hosteast.md` |
| 12 Check-in masivo | Hecho: columna Sel. + botón **Check-in masivo** + Mid `movimientosmasivo` |

**Convención técnica del proyecto:** MVC + Mid + `he_sp_*` donde aplique.

**Referencia offline existente:** `SGH.Web/wwwroot/js/modulos/censo-qr.js` (localStorage + evento `online` + `sincronizarPendientes`).

**Orden sugerido de oleadas:** sesión y validaciones de solicitud → UX solicitud/reserva → camas → finalizar legacy → offline → check-in masivo.

---

## R0 — Criterios transversales

| # | Tema | Entregable |
|---|------|------------|
| R0.1 | Trazabilidad | Issue/épica por requisito; PR vinculado. |
| R0.2 | QA | Matriz prueba manual + tests donde haya regla de dominio. |
| R0.3 | i18n | Cadenas nuevas en `idioma.js`. |
| R0.4 | Docs as-is | Flujos legacy documentados antes de codificar (finalizar reserva, correos). |

---

## 1. Modo offline, caché y sincronización

**Requisito:** Administrar caché/local storage para modo offline (botón o detección sin internet); capacidad de guardar y subir al conectar; opción “sincronizar datos”.

**Estado actual:** Solo censo QR usa cola en `localStorage`. No hay servicio global ni opción Mid de sincronización.

**Tareas:**

- Definir operaciones en cola (fase 1: censo; fase 2: según negocio).
- `wwwroot/js/core/colaOffline.js`: persistencia, `navigator.onLine`, botón sincronizar, idempotencia.
- UI: banner sin conexión; contador pendientes en escritorio.
- Pruebas: offline en DevTools, replay al `online`.

**Criterios de aceptación:**

- Cola durable sin red; sync manual o automático al reconectar; sin duplicados por re-sync.

**Prioridad:** Media-alta (fase 2 si alcance amplio).

---

## 2. Mantenedor de camas (nombre + estado 1/0)

**Requisito:** Mantenedor con nombre de cama y estado activado/desactivado (o habilitado/deshabilitado), campo **estado** 1/0.

**Estado actual:** `habitacion.js` + `guardarcama`; campos `Habilitada` e `IsActive` en `he_sp_cama_guardar`.

**Tareas:**

- Producto: mapear “estado” a `IsActive`, `Habilitada` o uno solo expuesto.
- UI: formulario/listado simplificado con etiqueta **Estado** (1/0).
- Reglas en `ReglasCama` si aplica.

**Criterios:** CRUD mínimo nombre + estado persistido en HOTELDB.

**Prioridad:** Media.

---

## 3. Sesión: 4 h sin actividad y reabrir pestaña

**Requisito:** Cerrar sesión tras 4 h sin actividad; al cerrar pestaña y volver, usuario sigue logueado si no expiró.

**Estado actual:** `SlidingExpiration` en `Program.cs`; `MinutosOperaciones` = 480 en `appsettings.json`; `IsPersistent = false` en `EmisorCookieSesion.cs`.

**Tareas:**

- Config: 240 minutos (`Sesion:MinutosOperaciones` + tests).
- `AddCookie`: `ExpireTimeSpan` coherente (4 h).
- Evaluar cookie persistente hasta `ExpiresUtc` para reabrir pestaña.
- Opcional: detector inactividad en cliente alineado al servidor.
- Actualizar docs de cookies/login.

**Criterios:** 4 h inactivo → login; reabrir pestaña antes del tope → misma sesión.

**Prioridad:** Alta.

---

## 4. Nueva solicitud: fechas en blanco y fecha solicitud ≥ hoy

**Requisito:** En alta, check-in y check-out en blanco; fecha solicitud no menor al día actual.

**Estado actual:** `solicitud.js` precarga `d.fechaActual()` en los tres campos.

**Tareas:**

- UI: check-in/out vacíos en alta; `min` = hoy en fecha solicitud.
- Dominio/Mid: rechazar fecha solicitud en el pasado.
- Edición: mantener bloqueo actual de fechas.

**Criterios:** Alta cumple UI + servidor; validar compatibilidad con `he_sp_solicitud_guardar`/SOL.

**Prioridad:** Alta.

---

## 5. Listado asignados al costado del side modal

**Requisito:** Al asignar, listado pasajero–habitación al **costado** del modal, no debajo.

**Estado actual:** `abrirHabitacionesMultiples` en `solicitud.js`; lista y `habitaciones-seleccion` en columna vertical.

**Tareas:**

- Layout dos columnas (habitaciones | asignados) en CSS (`escritorio.css`).
- Responsive: stack en móvil.

**Criterios:** En escritorio, asignados visibles lateralmente sin scroll extra.

**Prioridad:** Media.

---

## 6. Botón “Generar reserva” con otro color

**Requisito:** Distinguir “Generar reservas” del resto de botones.

**Tareas:** Clase `boton-generar-reservas` en tema HostEast; contraste accesible.

**Prioridad:** Baja.

---

## 7. Correo: empresa → campamento → vacío

**Requisito:** Prellenar correo de contacto: correo empresa; si no, correo campamento; si no, vacío para el usuario.

**Estado actual:** Campo obligatorio sin autollenado desde catálogo en alta.

**Tareas:**

- Exponer emails en `catalogo` o sesión (HOTELDB).
- UI: prellenar solo en alta según regla.
- Mantener validación `MailAddress` al guardar.

**Criterios:** Tres escenarios QA documentados.

**Prioridad:** Media.

---

## 8. No generar reserva sin habitaciones asignadas

**Requisito:** Bloquear generación hasta que todos los pasajeros tengan habitación.

**Estado actual:** SP valida `HabitacionId IS NULL`; UI habilita generar por selección de checkboxes, no por habitaciones en todos.

**Tareas:**

- UI: `generar` habilitado solo si todos `habitacionId > 0`.
- Mantener validación SP como autoridad.

**Prioridad:** Alta.

---

## 9. Número de reserva en detalle / Ver

**Requisito:** En detalle y al crear/ver, mostrar número de reserva asociado a cada pasajero.

**Tareas:**

- Ampliar `listar_pasajeros` o detalle con join a `Reservas`.
- Columna “Nº reserva” en tabla pasajeros.

**Prioridad:** Media-alta.

---

## 10. Reserva: botón anular

**Requisito:** Anular reserva (hoy solo check-in/check-out).

**Tareas:**

- Documentar anulación legacy.
- Mid `anular` + SP o legado.
- UI con confirmación y estados permitidos.

**Dependencia:** Investigación legacy.

**Prioridad:** Media.

---

## 11. Finalizar reserva (liberar habitación) — paridad legacy

**Requisito:** Replicar flujo legacy “finalizar reserva” para dejar habitación disponible.

**Estado actual:** `he_sp_reserva_finalizar_checkout` → Estado `F` (ver `Docs/as-is/sol-carga-pasajero-reservas.md`).

**Tareas:**

1. Documento as-is con pantalla legacy y estados.
2. Decidir botón explícito vs automático post check-out.
3. Mid + UI + pruebas.

**Prioridad:** Alta (doc); implementación tras sign-off.

---

## 12. Check-in masivo en reservas

**Requisito:** Selección múltiple en pantalla reserva; check-in conjunto.

**Estado actual:** Movimiento por fila (`movimientos-reserva.js`).

**Tareas:**

- UI multiselección + acción masiva.
- Mid batch o N× `he_sp_reserva_registrar_movimiento`.
- Reporte éxito/fallo por id.

**Prioridad:** Media; después de estados/anular/finalizar definidos.

---

## Orden de ejecución (oleadas)

| Oleada | Ítems |
|--------|--------|
| 1 | 3, 4, 8 |
| 2 | 7, 5, 6, 9 |
| 3 | 2, 11 (doc), 10 |
| 4 | 1, 12 |

---

## Checklist de aprobación (stakeholders)

1. Alcance offline: ¿solo censo u otros módulos?
2. Campo “estado” en camas: ¿IsActive, Habilitada, o uno UI?
3. Cookie persistente: ¿aceptable en PCs compartidas?
4. ¿SOL permite alta sin fechas check-in/out?
5. Pantalla legacy de referencia para “finalizar reserva”.

---

*Generado para revisión HostEast — NewRest_PORT / SGH.Web.*