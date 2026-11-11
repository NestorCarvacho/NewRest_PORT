# Flujo legacy: solicitud de alojamiento

## Alcance y fuentes

Este documento describe el flujo del sistema legacy para buscar, crear y administrar solicitudes de alojamiento. La descripción inicial fue contrastada parcialmente con el código legacy y con QA, sin guardar ni modificar registros. No implica que la nueva aplicación ya implemente el mismo comportamiento.

Ruta de acceso: **Gestión > Solicitud**.

Los detalles que no se indican en este documento, como validaciones, permisos y resultados de error, quedan pendientes de verificar en el legacy.

**Seguimiento en la nueva aplicación:** ver [§ Checklist de paridad — NewRest_PORT](#checklist-de-paridad--newrest_port) (revisión contra `MidSolicitudes`, `GestionSolicitudes`, `solicitud.js`, `/Solicitud/Datos`).

## 1. Buscar solicitudes

La pantalla presenta el filtro **Buscar Solicitudes de alojamiento**:

| Campo o acción | Control |
|---|---|
| Grupo Empresa | Lista desplegable |
| Desde | Selector de fecha |
| Hasta | Selector de fecha |
| Solicitante | Lista desplegable |
| Estado | Lista desplegable |
| Número de solicitud | Entrada de texto |
| Buscar | Botón |
| Crear | Botón |

**Buscar** permite consultar solicitudes con los filtros de la pantalla. **Crear** abre el formulario **Administrar Solicitudes de alojamiento \| Nueva solicitud**.

## 2. Crear una solicitud

El formulario de nueva solicitud contiene:

| Campo | Control y comportamiento observado |
|---|---|
| Número | Solo lectura |
| Fecha | Selector de fecha; por defecto, la fecha actual |
| Grupo empresa | Lista desplegable |
| Solicitante | Lista desplegable; opcional |
| Con turno | Interruptor; desactivado por defecto |
| Turno principal | Lista desplegable; desactivada mientras **Con turno** está desactivado |
| Rotación | Lista desplegable; desactivada mientras **Con turno** está desactivado |
| ADC | Entrada de texto; opcional |
| Check in | Selector de fecha |
| Check out | Selector de fecha |
| Estado | Texto de solo lectura |
| Observación | Texto multilínea; opcional |
| Unidad de Negocio | Lista desplegable; opcional |
| División | Lista desplegable; opcional |
| Departamento | Lista desplegable; opcional |
| Número de Contrato | Texto; opcional |
| Email de contacto en campamento | Texto; obligatorio según la validación del código legacy |
| Número de teléfono contacto campamento | Texto; opcional |
| Reservar visita | Interruptor; desactivado por defecto |

Al presionar **Guardar**, se registra la solicitud mediante el o los procedimientos almacenados correspondientes. Después de guardarla, quedan disponibles las siguientes opciones:

- **Exportar**.
- **Cargar pasajeros**.
- **Crear Usuarios**.
- **Agregar Pasajero**.
- Icono de basura: **anular solicitud**.
- Icono de disquete: **guardar solicitud**.

### Referencia técnica para verificar

El [README de la solución](../../README.md) indica que el backend nuevo reutiliza `SOL_GuardaSolicitudAlojamiento` para guardar solicitudes. Esto sirve como referencia para la investigación, pero no confirma por sí solo todos los procedimientos ejecutados por la pantalla legacy ni sus parámetros o secuencia. Es necesario verificarlos en el código o la base de datos legacy.

## 3. Exportar pasajeros

**Exportar** descarga un archivo Excel con las siguientes columnas, en este orden:

1. `Id`
2. `FechaSolicitud`
3. `RutPasajero`
4. `NombrePasajero`
5. `RutUsuarioSolicitante`
6. `NombreUsuarioSolicitante`
7. `EmpresaMandante`
8. `Campamento`
9. `observacion`
10. `Estado`
11. `FechaLlegada`
12. `FechaSalida`
13. `TipoTurno`
14. `Turno`
15. `ConSistemaTurno`
16. `TipoTurnoRotacion`
17. `TurnoRotacion`
18. `Habitacion`

## 4. Cargar pasajeros

**Cargar pasajeros** ofrece:

1. Descargar una plantilla Excel. El código legacy de carga lee la hoja `pasajeros` y las columnas `Identificador`, `TipoIdentificador`, `Nombres`, `Apellidos` y `Genero`; no utiliza las 18 columnas del informe de exportación.
2. Seleccionar un archivo Excel con los datos ingresados.
3. Elegir **Cancelar** o **Cargar**.

Quedan pendientes de verificar los formatos y validaciones de las columnas, el tratamiento de filas inválidas y el resultado de la carga.

## 5. Crear usuario

**Crear Usuarios** abre el modal **Crear Usuario** con los siguientes campos:

| Campo | Control u opciones |
|---|---|
| Identificador | Entrada de texto |
| Tipo identificador | Lista desplegable: RUT o DNI |
| Nombre | Entrada de texto |
| Apellido | Entrada de texto |
| Género | Lista desplegable: Femenino, masculino u otros |

El modal ofrece **Cancelar** y **Guardar**.

## 6. Agregar pasajero

**Agregar Pasajero** abre el modal **Agregar Pasajero**:

| Campo | Control |
|---|---|
| ¿Por cantidad? | Interruptor sí/no |
| Pasajero | Lista desplegable |
| Check In | Texto de solo lectura |
| Check out | Texto de solo lectura |

El modal ofrece **Cancelar** y **Guardar**. Al activar **¿Por cantidad?**, se reemplaza el selector de pasajero por un campo de cantidad, con valores entre 1 y 999.

## 7. Lista de pasajeros

Debajo de las opciones se muestra una tabla con los pasajeros ingresados:

| Columna | Contenido |
|---|---|
| Selección | Casillas de selección individual y masiva |
| Habitación | Puede estar vacía hasta su asignación o tener una habitación preasignada si está disponible para el rango de fechas |
| Identificador | Identificador del pasajero |
| Tipo identificador | Tipo de identificador |
| Nombre | Nombre del pasajero |
| Acciones | Eliminar pasajero de la lista o editar pasajero |

Al seleccionar uno o más pasajeros se habilitan, junto a **Exportar**, las opciones **Eliminar pasajero**, **Generar Reserva** y **Asignar habitaciones**.

### Asignar habitaciones

**Asignar habitaciones** despliega:

- Lista desplegable **Seleccione modulo**.
- Lista desplegable **Seleccione Piso**.
- Botones **Cancelar** y **Asignar**.

El usuario selecciona habitaciones disponibles para los pasajeros seleccionados; las habitaciones elegidas se asocian a los pasajeros en el orden de selección. Para tres pasajeros en habitaciones de una cama se requieren tres habitaciones. Quedan pendientes de verificar todas las validaciones de disponibilidad.

### Generar reserva

**Generar Reserva** abre un modal con la pregunta:

> ¿Esta seguro que desea generar las reservas para la solicitud seleccionada?

El modal ofrece **Cancelar** y **Continuar**. Aunque la selección habilita la acción, el código legacy valida que todos los pasajeros tengan habitación y envía la solicitud completa para generar sus reservas.

## 8. Puntos pendientes de levantamiento

- Procedimientos almacenados ejecutados por cada acción, sus parámetros y secuencia.
- Campos obligatorios no identificados como opcionales, validaciones y mensajes de error.
- Valores disponibles y dependencias de las listas desplegables, incluidos turno y rotación.
- Estados de la solicitud, transiciones, permisos y restricciones para editar, guardar o anular.
- Comportamiento de **Reservar visita**.
- Reglas de importación, exportación, selección y edición de pasajeros.
- Reglas de preasignación, asignación de habitaciones y generación de reservas.

## Checklist de paridad — NewRest_PORT

Leyenda de **Estado:** **Cumple** = equivalente funcional en PORT; **Parcial** = implementado con diferencias de UX o alcance; **No** = ausente; **Extra** = en PORT y no descrito en este AS-IS; **Pendiente** = no contrastado con legacy/QA.

Última revisión: flujo documentado en §1–§7 frente a `SGH.Application/Mid/MidSolicitudes.cs`, `GestionSolicitudes`, `wwwroot/js/modulos/solicitud.js`, `Pages/Solicitud/Datos`.

### Acceso y navegación

| Ítem AS-IS | Estado | Implementación PORT | Notas |
|------------|--------|---------------------|--------|
| Ruta **Gestión > Solicitud** | Parcial | Menú: enlace **Solicitud alojamiento** → `/Solicitud/Datos` | Ítems «Pasajeros de solicitud» y «Reserva por solicitud» del menú siguen sin enlace. |
| Política de acceso | Extra | `[Authorize(GestionarReservas)]` en página y `SolicitudesController` | No detallado en AS-IS. |

### §1 — Buscar solicitudes

| Ítem AS-IS | Estado | Implementación PORT | Notas |
|------------|--------|---------------------|--------|
| Filtro Grupo empresa | Cumple | `catalogo` + `listar.grupoId` | |
| Desde / Hasta | Cumple | `listar` con fechas obligatorias | |
| Solicitante | Cumple | `listar.solicitanteId` | |
| Estado | Cumple | `listar.estado` | |
| Número de solicitud | Cumple | `listar.numero` | |
| Botón Buscar | Cumple | Tabla vía módulo `solicitud` (`buscar`) | |
| Botón Crear | Cumple | `abrirNueva` → formulario nueva solicitud | |

### §2 — Crear / editar solicitud (formulario)

| Ítem AS-IS | Estado | Implementación PORT | Notas |
|------------|--------|---------------------|--------|
| Número solo lectura | Cumple | Campo `numero` readonly | |
| Fecha solicitud (default hoy) | Cumple | `fechaSolicitud` en alta | |
| Grupo empresa | Cumple | Select + `catalogo` | Deshabilitado en edición. |
| Solicitante opcional | Cumple | Select | Deshabilitado en edición. |
| Con turno / turno / rotación | Cumple | Checkbox + selects; turnos deshabilitados si no hay sistema de turno | En edición, fechas y turnos bloqueados (alineado README). |
| ADC, check-in, check-out | Cumple | Campos fecha | Alta editable; edición según reglas servidor. |
| Estado solo lectura | Cumple | Campo texto readonly | |
| Observación | Cumple | Textarea | |
| UN / división / departamento | Cumple | `organizacion` + selects encadenados | |
| Nº contrato | Cumple | Texto; precarga desde grupo en alta | |
| Email contacto obligatorio | Cumple | `email` required + `MailAddress` en `GestionSolicitudes` | |
| Teléfono contacto | Cumple | Texto opcional | |
| Reservar visita | Parcial | Checkbox `reservaVisita` → SP | Reglas de negocio completas: **Pendiente** (§8). |
| Guardar → SP legacy | Cumple | `crear` / `editar` → `sp_Solicitud_Guardar` / `SOL_Guarda…` | |
| **Exportar** (tras guardar) | No | — | Sin opción Mid ni UI. |
| **Cargar pasajeros** | Cumple | «Importar pasajeros» en detalle | |
| **Crear usuarios** | No | — | Solo vía importación masiva (comportamiento parcial de §5). |
| **Agregar pasajero** | Cumple | Botón en detalle estado `P` | |
| Anular solicitud | Cumple | «Anular solicitud» + confirmación | |
| Guardar solicitud (edición) | Cumple | «Editar solicitud» + guardar | Botón texto, no icono disquete. |

### §3 — Exportar pasajeros (18 columnas)

| Ítem AS-IS | Estado | Implementación PORT | Notas |
|------------|--------|---------------------|--------|
| Descarga Excel informe completo | No | — | Columnas §3 sin implementar. |

### §4 — Cargar pasajeros

| Ítem AS-IS | Estado | Implementación PORT | Notas |
|------------|--------|---------------------|--------|
| Plantilla Excel | Cumple | GET `/Solicitud/PlantillaPasajeros` | |
| Hoja `pasajeros` | Cumple | `PlanillaPasajerosExcel` | |
| Columnas Identificador…Género | Cumple | Validación en lectura + `GestionSolicitudes.CargarPasajerosAsync` | |
| Seleccionar archivo .xlsx | Cumple | `enviarArchivo` multipart | |
| Cancelar / Cargar | Cumple | «Volver» / «Importar» | |
| Confirmación usuarios existentes | Cumple | Aviso + `confirm` antes de importar | |
| Formatos y filas inválidas | Parcial | Rechazo por fila en respuesta | Paridad fina con legacy: **Pendiente** QA. |

### §5 — Crear usuario (modal)

| Ítem AS-IS | Estado | Implementación PORT | Notas |
|------------|--------|---------------------|--------|
| Modal Crear Usuario (RUT/DNI, nombre, apellido, género) | No | — | `sp_Solicitud_CargarPasajero` puede crear en masa. |
| Alta individual sin planilla | Parcial | Solo importación | |

### §6 — Agregar pasajero

| Ítem AS-IS | Estado | Implementación PORT | Notas |
|------------|--------|---------------------|--------|
| ¿Por cantidad? | Cumple | Checkbox → opción `cupos` (1–999) | |
| Pasajero (lista) | Cumple | Select usuarios (`USU_ObtieneUsuarios`, perfil 2) | |
| Check-in / check-out solo lectura | Cumple | Fechas readonly desde solicitud | |
| Cancelar / Guardar | Cumple | Panel modal | |
| Opción `pasajero` / `cupos` en Mid | Cumple | `MidSolicitudes` | |

### §7 — Lista y acciones sobre pasajeros

| Ítem AS-IS | Estado | Implementación PORT | Notas |
|------------|--------|---------------------|--------|
| Tabla con selección individual/masiva | Cumple | Checkboxes + «seleccionar todos» | |
| Columnas habitación, id, tipo, nombre | Cumple | Tabla en `mostrarDetalle` | Cupos: texto «Cupo sin identificar». |
| Editar / eliminar por fila | Cumple | Botones por fila | |
| Eliminar con selección | Cumple | «Eliminar pasajeros» (secuencial) | Informa cuántos se eliminaron si hay error. |
| **Exportar** con selección | No | — | |
| **Generar reserva** con selección | Parcial | Botón habilitado si `selección.size > 0`; Mid genera **toda** la solicitud (`generar` + `id`) | Backend valida todos con habitación (`sp_Solicitud_GenerarReservas`). UX distinta al legacy. |
| **Asignar habitaciones** con selección | Cumple | Solo pasajeros seleccionados | |
| Módulo y piso en asignación | Cumple | Filtros módulo/piso + lista habitaciones | Etiquetas «módulo/piso» vs texto legacy. |
| Orden pasajero ↔ habitación | Cumple | Una habitación por pasajero seleccionado | |
| Confirmación generar reservas | Cumple | Mismo texto de confirmación | |
| Validar todos con habitación antes de generar | Cumple | SP + mensajes 51051, 51056, etc. | |
| Generar solicitud completa (no solo selección) | Cumple | `GenerarReservasAsync(solicitudId)` | |
| Preasignación automática en lista | Pendiente | — | AS-IS: habitación puede venir preasignada; no verificado en PORT. |

### Backend Mid (opciones)

| Opción | Estado | Notas |
|--------|--------|--------|
| `listar`, `catalogo`, `organizacion`, `detalle`, `pasajeros`, `habitaciones` | Cumple | |
| `crear`, `editar`, `anular` | Cumple | |
| `cupos`, `pasajero`, `eliminar`, `asignar`, `generar` | Cumple | |
| `plantilla`, `cargar` (multipart) | Cumple | |
| `exportar` | No | Gap frente §3. |
| `notificar` (correo estado `R`) | Extra | Post-generación / reenvío manual. |
| Alcance campamento en mutaciones | Extra | `ExigirSolicitudDelCampamentoAsync` | Más estricto que listado solo mandante (README). |

### §8 — Pendientes (siguen abiertos en ambos lados)

| Tema | En PORT |
|------|---------|
| Secuencia exacta de SP por acción | Documentado en README/SQL; paridad mensajes: **Pendiente** QA. |
| Validaciones y mensajes exhaustivos | Parcial en `GestionSolicitudes`; **Pendiente** vs legacy. |
| Catálogos dependientes (turno/rotación) | Cumple en UI; reglas finas **Pendiente**. |
| Estados y transiciones / permisos por perfil | Parcial (`GestionarReservas`); matriz fina **Pendiente**. |
| **Reservar visita** (reglas) | Campo guardado; comportamiento **Pendiente**. |
| Exportación y reglas de selección | Exportación **No**; selección generar **Parcial**. |
| Preasignación y competencia con otros sistemas | Bloqueo applock en generar; asignaciones ajenas: README. |

### Próximos pasos sugeridos (producto)

1. Implementar **`exportar`** (Mid + Excel 18 columnas §3) y botón en detalle con selección.
2. Implementar modal **`Crear usuario`** o documentar que importación lo sustituye.
3. Ajustar **Generar reservas**: habilitar sin exigir selección (o exigir «seleccionar todos») si se busca paridad UX con legacy.
4. Enlazar o eliminar entradas de menú huérfanas bajo Gestión.
