# Flujo legacy: solicitud de alojamiento

## Alcance y fuentes

Este documento describe el flujo del sistema legacy para buscar, crear y administrar solicitudes de alojamiento. La descripción inicial fue contrastada parcialmente con el código legacy y con QA, sin guardar ni modificar registros. No implica que la nueva aplicación ya implemente el mismo comportamiento.

Ruta de acceso: **Gestión > Solicitud**.

Los detalles que no se indican en este documento, como validaciones, permisos y resultados de error, quedan pendientes de verificar en el legacy.

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
