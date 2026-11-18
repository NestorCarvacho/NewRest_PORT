# HOTELDB — orden de ejecución de scripts `.sql` del repositorio

Base de partida: una **HOTELDB legado** ya existente (procedimientos `SOL_*`, `HAB_*`, `USU_*`, tablas `Menus`, etc.). Los scripts de `SGH.Infrastructure` son **envoltorios o extensiones**; no sustituyen todo el legado.

Convenciones:

- **Copias locales en un solo lugar:** carpeta raíz `scripts-ejecutar/` (gitignored). Poblarla con `./tools/sincronizar-scripts-ejecutar.sh`; los archivos se llaman `{Modulo}_{nombre}.sql`.
- Ejecutar cada archivo en **SSMS / sqlcmd** contra la misma base que usa `ConnectionStrings:Hoteldb`.
- Abrir los `.sql` como **UTF-8** (mensajes con tildes).
- Los scripts con `CREATE OR ALTER` son **re-ejecutables** salvo que el comentario del archivo diga lo contrario.

## 1. Lista maestra (despliegue completo NewRest SQL)

| # | Ruta del archivo | Módulo | Cuándo / dependencia |
|---|------------------|--------|----------------------|
| 1 | `SGH.Infrastructure/Cuenta/Persistence/001_sp_login.sql` | Login | Independiente; contrato documentado en `Docs/login-pendiente-revision-base-de-datos.md`. |
| 2 | `SGH.Infrastructure/Navegacion/Persistence/001_sp_menu.sql` | Menú | Requiere tablas `Menus`, `MenusPerfil`. |
| 3 | `SGH.Infrastructure/Censo/Persistence/001_sp_censo.sql` | Censo | Envuelve SP legado `HAB_ObtieneCensos` / `CSO_GuardaCenso`. |
| 4 | `SGH.Infrastructure/Reserva/Persistence/001_sp_reserva.sql` | Reserva | Antes de `004_sp_generar_reservas.sql` (generación llama reserva legada). |
| 5 | `SGH.Infrastructure/Habitacion/Persistence/002_hotldb_cu_hab_extension.sql` | Habitación | **Primero** del bloque CU-HAB: tablas/columnas. |
| 6 | `SGH.Infrastructure/Habitacion/Persistence/003_sp_bano_cama.sql` | Habitación | Después de **002**; SP de baño/cama. |
| 7 | `SGH.Infrastructure/Habitacion/Persistence/005_hotldb_tipo_cama_inventario.sql` | Habitación | Tabla `TipoCama` y columnas inventario en `Camas` (CU-HAB-02). |
| 8 | `SGH.Infrastructure/Habitacion/Persistence/006_sp_tipo_cama_inventario.sql` | Habitación | SP tipos de cama y catálogo habitación extendido. |
| 8b | `SGH.Infrastructure/Habitacion/Persistence/007_sp_habitacion_resolver_nombre.sql` | Habitación | Resolver habitación por nombre (carga masiva camas). |
| 9 | `SGH.Infrastructure/Habitacion/Persistence/001_sp_habitacion.sql` | Habitación | Después de **002**, **003**, **005** y **006** (referencia `Banos`/`Camas`/`TipoCama`). |
| 10 | `SGH.Infrastructure/Habitacion/Persistence/004_banos_isactive_estado.sql` | Habitación | **Opcional**, una vez: `UPDATE` de datos; requiere tabla `Banos` de **002**. |
| 11 | `SGH.Infrastructure/Solicitud/Persistence/001_sp_solicitud.sql` | Solicitud | Base del módulo; ver `README.md`. |
| 12 | `SGH.Infrastructure/Solicitud/Persistence/002_sp_pasajeros_solicitud.sql` | Solicitud | Pasajeros en detalle. |
| 13 | `SGH.Infrastructure/Solicitud/Persistence/003_sp_guardar_pasajero.sql` | Solicitud | Alta/edición pasajero. |
| 14 | `SGH.Infrastructure/Solicitud/Persistence/005_sp_habitaciones_solicitud.sql` | Solicitud | Disponibles / asignar / desasignar (antes o junto a generar). |
| 15 | `SGH.Infrastructure/Solicitud/Persistence/004_sp_generar_reservas.sql` | Solicitud | Generar reservas; después de **003** y **005** recomendado. |
| 16 | `SGH.Infrastructure/Solicitud/Persistence/007_sp_solicitud_asegurar_periodo.sql` | Solicitud | Bloque **importación** masiva (con **008**). |
| 17 | `SGH.Infrastructure/Solicitud/Persistence/008_sp_reserva_finalizar_checkout.sql` | Solicitud | Importación / checkout. |
| 18 | `SGH.Infrastructure/Solicitud/Persistence/006_sp_cargar_pasajero.sql` | Solicitud | Carga masiva; después de **007** y **008**. |
| 19 | `SGH.Infrastructure/Navegacion/Persistence/002_menu_config_habitacion_bano.sql` | Navegación | Entradas de menú Configuración; después de CU-HAB operativo. |
| 20 | `SGH.Infrastructure/Navegacion/Persistence/003_menusperfil_habitacion_bano_por_perfil.sql` | Navegación | `MenusPerfil` por perfil (p. ej. 68); después del **002**. |

### Parches incrementales (QA que ya ejecutó los `001_*` antiguos)

No re-ejecutar los `001` solo para aplicar estos cambios. Usar los archivos numerados siguientes (idempotentes con `CREATE OR ALTER`):

| Orden QA | Archivo | Requiere antes |
|----------|---------|----------------|
| A | `SGH.Infrastructure/Cuenta/Persistence/002_sp_login_passwordv2_dual.sql` | `001_sp_login.sql` (versión sin `PasswordV2` / dual hash) |
| B | `SGH.Infrastructure/Reserva/Persistence/002_sp_reserva_buscar_rango_top.sql` | `001_sp_reserva.sql` (sin `@MaxFilas` ni rango 30 días en búsqueda) |
| C | `SGH.Infrastructure/Solicitud/Persistence/009_sp_solicitud_buscar_listado.sql` | `001_sp_solicitud.sql` y, si aplica, scripts **002–008** del módulo |
| D | `005_hotldb_tipo_cama_inventario.sql` + `006_sp_tipo_cama_inventario.sql` | Bloque habitación **002** y **003** ya aplicado |

En **instalación nueva**, los `001` del repositorio actual ya incluyen el mismo contenido; los `002` / `009` anteriores son opcionales.

Tras actualizar el flujo de **importación**, el `README.md` pide **volver a desplegar**:

- `001_sp_solicitud.sql` (ítem 9)
- `004_sp_generar_reservas.sql` (ítem 13)
- Bloque `sp_Reserva_RegistrarMovimiento` en `001_sp_reserva.sql` (ítem 4)

## 2. Por módulo (si solo migrás una parte)

### Cuenta

1. `001_sp_login.sql`

No ejecutar: `001_sp_login_borrador.sql` (borrador / checklist en `Docs/login-pendiente-revision-base-de-datos.md`).

### Navegación

1. `001_sp_menu.sql`
2. `002_menu_config_habitacion_bano.sql` (solo si usás pantallas Config habitaciones/baños)

### Habitación (CU-HAB-01)

Orden fijo — ver `Docs/SGH-CU-HAB-01-implementacion.md`:

1. `002_hotldb_cu_hab_extension.sql`
2. `003_sp_bano_cama.sql`
3. `001_sp_habitacion.sql`
4. `004_banos_isactive_estado.sql` (opcional, corrección de datos)

### Reserva

1. `001_sp_reserva.sql`

### Censo

1. `001_sp_censo.sql`

### Solicitud

**Uso estándar (sin importación):**

1. `001_sp_solicitud.sql`
2. `002_sp_pasajeros_solicitud.sql`
3. `003_sp_guardar_pasajero.sql`
4. `005_sp_habitaciones_solicitud.sql`
5. `004_sp_generar_reservas.sql`

**Importación de pasajeros (además o actualización):**

1. `007_sp_solicitud_asegurar_periodo.sql`
2. `008_sp_reserva_finalizar_checkout.sql`
3. `006_sp_cargar_pasajero.sql`
4. Re-ejecutar `001_sp_solicitud.sql`, `004_sp_generar_reservas.sql`, `001_sp_reserva.sql` (movimientos).

### Informe legado `INF_Aseos` (optimización, fuera del MVC)

No bloquea la aplicación NewRest:

1. `Docs/as-is/sql/INF_Aseos_diagnostico.sql` — solo consultas de depuración (**no** altera objetos).
2. `Docs/as-is/sql/INF_Aseos_optimizado.sql` — `CREATE OR ALTER PROCEDURE dbo.INF_Aseos` tras comparar con el legado (`Docs/as-is/inf-aseos-optimizacion.md`).

## 3. Objetos legado (no hay script en este repo)

Estos deben **ya existir** en HOTELDB; los scripts nuevos los invocan:

| Prefijo / nombre | Usado por |
|------------------|-----------|
| `SOL_GuardaSolicitudAlojamiento`, `SOL_Obtiene*` | Solicitud 001, 002, … |
| `SOL_GuardaPasajero*`, `SOL_AsignaHabitacion*` | 003, 005, 006 |
| `HAB_ObtieneHabitacionesDisponiblesV2`, `HAB_ValidaCreacionReserva_Solicitud` | 005, 004 |
| `RES_CreaReserva` | 004 |
| `HAB_GuardaHabitacion`, censos `HAB_ObtieneCensos` | Habitación 001, Censo 001 |
| `USU_*`, tablas `Perfiles`, `Menus` | Login, menú |

Inventario y consultas: `Docs/datos/diccionario-hoteldb-procedimientos-almacenados.md`.