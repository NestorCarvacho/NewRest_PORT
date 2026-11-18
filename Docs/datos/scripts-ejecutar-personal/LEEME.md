# Scripts HOTELDB — copia para ejecución manual

Carpeta de **uso personal**: mismos `.sql` que en `SGH.Infrastructure/**/Persistence/`, reunidos aquí para no buscarlos módulo por módulo.

- **Fuente canónica del repo:** `SGH.Infrastructure/...` (si hay duda, compará con el original).
- **Base:** la de `ConnectionStrings:Hoteldb` (HOTELDB).
- **Orden detallado y dependencias:** [hoteldb-scripts-orden-ejecucion.md](../hoteldb-scripts-orden-ejecucion.md).

No ejecutes `001_sp_login_borrador.sql` (no está en esta carpeta a propósito).

## Orden rápido (despliegue completo)

| # | Archivo en esta carpeta |
|---|-------------------------|
| 1 | `Cuenta/001_sp_login.sql` |
| 2 | `Navegacion/001_sp_menu.sql` |
| 3 | `Censo/001_sp_censo.sql` |
| 4 | `Reserva/001_sp_reserva.sql` |
| 5 | `Habitacion/002_hotldb_cu_hab_extension.sql` |
| 6 | `Habitacion/003_sp_bano_cama.sql` |
| 7 | `Habitacion/005_hotldb_tipo_cama_inventario.sql` |
| 8 | `Habitacion/006_sp_tipo_cama_inventario.sql` |
| 8b | `Habitacion/007_sp_habitacion_resolver_nombre.sql` |
| 9 | `Habitacion/001_sp_habitacion.sql` |
| 10 | `Habitacion/004_banos_isactive_estado.sql` (opcional, datos) |
| 11–18 | `Solicitud/001` … `008` (ver doc maestro) |
| 19–20 | `Navegacion/002_*`, `Navegacion/003_*` |

## Parches incrementales (QA)

Solo si ya tenías versiones viejas de los `001`:

| Archivo |
|---------|
| `Cuenta/002_sp_login_passwordv2_dual.sql` |
| `Reserva/002_sp_reserva_buscar_rango_top.sql` |
| `Solicitud/009_sp_solicitud_buscar_listado.sql` |

## Actualización listado por fila (escritorio)

Si aplicaste solo el cambio de **una fila sin recargar 5000 registros**, re-ejecutá en HOTELDB:

1. `Habitacion/001_sp_habitacion.sql` (`sp_Habitacion_Listar` con `@HabitacionId`)
2. `Habitacion/003_sp_bano_cama.sql` (`sp_Bano_Listar` con `@BanoId`)
3. `Habitacion/006_sp_tipo_cama_inventario.sql` (`sp_TipoCama_Listar` con `@TipoCamaId`)

## Contenido por carpeta

- **Cuenta** — login
- **Navegacion** — menú y perfiles
- **Habitacion** — CU-HAB, baños, camas, tipos de cama
- **Reserva** — reservas y búsqueda
- **Censo** — censo
- **Solicitud** — solicitudes, pasajeros, importación

Los archivos se copian manualmente o al commitear cambios en Infrastructure; si editás un SP en el código, volvé a copiarlo acá o ejecutá desde la ruta canónica.