# HOTELDB — cambios en SP con bloque de auditoría

En la base **HOTELDB** los procedimientos `he_sp_*` llevan un bloque de auditoría al inicio del cuerpo (`BEGIN`), con **author**, **date** y **description**. Esa versión (script SSMS con `USE`, `SET ANSI_NULLS`, cabecera de objeto y auditoría) es la referencia al desplegar.

## Flujo con Cloud Agent / desarrollo

1. El agente indica **qué SP hay que tocar** (`dbo.he_sp_...`) y describe el cambio funcional (sin asumir el script ya auditado en HOTELDB).
2. Quien despliega **adjunta o pega** el script actual del SP desde SSMS (como en `he_sp_solicitud_listar_pasajeros`, fecha 2026-11-20).
3. Se aplica el cambio **conservando** el bloque de auditoría; si el cambio es nuevo, se actualiza `date` y `description` (y `author` si corresponde).
4. El script resultante se ejecuta en HOTELDB y, si aplica, se incorpora al lote diario `Docs/datos/migracion/YYYYMMDD_modificaciones.sql`.
5. En ese lote, cada bloque nuevo o modificado lleva una línea de trazabilidad del bundle, encima de la sección:

   `-- YYYYMMDD ncarvacho: descripción breve del cambio (una o dos líneas).`

   Además se mantiene el bloque `author` / `date` / `description` dentro del `BEGIN` del SP en HOTELDB.

Los archivos en `SGH.Infrastructure/**/Persistence/*.sql` del repo son **plantilla / contrato** para la aplicación; pueden ir sin `USE [HOTELDB]` ni cabecera SSMS. Tras un cambio importante, conviene **sincronizar** la plantilla del repo con el cuerpo lógico acordado, pero el despliegue autoritativo es el script auditado de la base.

## Plantilla de auditoría (dentro del `BEGIN`)

```sql
    --======================================================
    --  author: Nestor Carvacho
    --  date: YYYY-MM-DD
    --  description: Texto breve del propósito del SP.
    --======================================================
```

## Ejemplo (referencia en base)

`dbo.he_sp_solicitud_listar_pasajeros` — auditoría `date: 2026-11-20`, descripción *Pasajeros vinculados a la solicitud.*

## SP tocados en el lote 2026-11-23 (reserva / solicitud)

Para re-sincronizar desde HOTELDB, pedir script completo de cada uno si ya existen con auditoría previa:

| SP | Archivo plantilla en repo |
|----|---------------------------|
| `he_sp_reserva_anular` | `SGH.Infrastructure/Reserva/Persistence/003_sp_reserva_anular.sql` |
| `he_sp_reserva_reversar_movimiento` | `SGH.Infrastructure/Reserva/Persistence/004_sp_reserva_reversar_movimiento.sql` |
| `he_sp_solicitud_contacto_correo` | `001_sp_solicitud.sql` |
| `he_sp_solicitud_guardar` | `001_sp_solicitud.sql` |
| `he_sp_solicitud_reservas_por_pasajero` | `002_sp_pasajeros_solicitud.sql` |
| `he_sp_solicitud_generar_reservas` | `004_sp_generar_reservas.sql` |
| `he_sp_solicitud_habitaciones_disponibles` | `005_sp_habitaciones_solicitud.sql` |
| `he_sp_solicitud_asignar_habitacion` | `005_sp_habitaciones_solicitud.sql` |
| `he_sp_solicitud_desasignar_habitacion` | `005_sp_habitaciones_solicitud.sql` |

SP **no** incluidos en ese lote pero relacionados: `he_sp_solicitud_listar_pasajeros` (auditoría 2026-11-20 en base).

## SP tocados en el lote 2026-11-23 (censo QR)

| SP | Archivo plantilla en repo |
|----|---------------------------|
| `he_sp_censo_listar_catalogo` (ampliado) | `SGH.Infrastructure/Censo/Persistence/005_sp_censo_qr.sql` y `001_sp_censo.sql` |
| `he_sp_censo_obtener_habitacion_qr` | `SGH.Infrastructure/Censo/Persistence/005_sp_censo_qr.sql` y `001_sp_censo.sql` |