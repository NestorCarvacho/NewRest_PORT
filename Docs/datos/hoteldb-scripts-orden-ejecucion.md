# HOTELDB — orden de ejecución de scripts `.sql` del repositorio

Base de partida: una **HOTELDB legado** ya existente (procedimientos `SOL_*`, `HAB_*`, `USU_*`, tablas `Menus`, etc.). Los scripts de `SGH.Infrastructure` son **envoltorios o extensiones**; no sustituyen todo el legado.

Convenciones:

- **Nombres de SP propios del repo:** `dbo.he_sp_{tabla}_{accion}` (prefijo proyecto `he_`). Ver `Docs/datos/he-sp-nomenclatura-procedimientos.md`.
- **Copias locales en un solo lugar:** carpeta raíz `scripts-ejecutar/` (gitignored). Poblarla con `./tools/sincronizar-scripts-ejecutar.sh`; los archivos se llaman `{Modulo}_{nombre}.sql`.
- **SSMS sin terminal:** paquetes por módulo en `Docs/datos/paquetes-ssms/` (ej. `Censo_ejecutar_HOTELDB.sql` + `LEEME.md`).
- Ejecutar cada archivo en **SSMS / sqlcmd** contra la misma base que usa `ConnectionStrings:Hoteldb`.
- Abrir los `.sql` como **UTF-8** (mensajes con tildes).
- Los scripts con `CREATE OR ALTER` son **re-ejecutables** salvo que el comentario del archivo diga lo contrario.

## 1. Lista maestra (despliegue completo NewRest SQL)

| # | Ruta del archivo | Módulo | Cuándo / dependencia |
|---|------------------|--------|----------------------|
| 1 | `SGH.Infrastructure/Cuenta/Persistence/001_sp_login.sql` | Login | Independiente; contrato documentado en `Docs/login-pendiente-revision-base-de-datos.md`. |
| 2 | `SGH.Infrastructure/Navegacion/Persistence/001_navegacion_hotldb.sql` | Menú / navegación | `he_sp_menu_obtener_por_perfil` + datos Menus/MenusPerfil CU-HAB. |
| 3 | `SGH.Infrastructure/Censo/Persistence/001_sp_censo.sql` | Censo | Envuelve SP legado `HAB_ObtieneCensos` / `CSO_GuardaCenso`. |
| 4 | `SGH.Infrastructure/Reserva/Persistence/001_reserva_hotldb.sql` | Reserva | Todos los `he_sp_reserva_*`; desplegar antes o junto con solicitud (generación/movimientos). |
| 5 | `SGH.Infrastructure/Habitacion/Persistence/001_habitacion_hotldb.sql` | Habitación | CU-HAB-01/02: tablas, columnas y todos los `he_sp_*` en un solo script. |
| 6 | `SGH.Infrastructure/Solicitud/Persistence/001_solicitud_hotldb.sql` | Solicitud | Todos los `he_sp_solicitud_*` e importación (`006`–`008` incluidos). Requiere **reserva** desplegada para generar/movimientos. |

### Parches incrementales (QA que ya ejecutó los `001_*` antiguos)

No re-ejecutar los `001` solo para aplicar estos cambios. Usar los archivos numerados siguientes (idempotentes con `CREATE OR ALTER`):

| Orden QA | Archivo | Requiere antes |
|----------|---------|----------------|
| A | ~~`002_sp_login_passwordv2_dual.sql`~~ | **Obsoleto.** Volver a ejecutar solo `001_sp_login.sql` o `Docs/datos/paquetes-ssms/Login_ejecutar_HOTELDB.sql`. |
| B | ~~`002_sp_reserva_buscar_rango_top.sql`~~ | **Obsoleto.** Re-ejecutar `001_reserva_hotldb.sql` o `Docs/datos/paquetes-ssms/Reserva_ejecutar_HOTELDB.sql`. |
| C | ~~`009_sp_solicitud_buscar_listado.sql`~~ | **Obsoleto.** Re-ejecutar `001_solicitud_hotldb.sql` o `Docs/datos/paquetes-ssms/Solicitud_ejecutar_HOTELDB.sql`. |
| D | ~~parches CU-HAB por archivo~~ | **Obsoleto.** Re-ejecutar `001_habitacion_hotldb.sql` o `Docs/datos/paquetes-ssms/Habitacion_ejecutar_HOTELDB.sql`. |

En **instalación nueva**, los `001` del repositorio actual ya incluyen el mismo contenido; los `002` / `009` anteriores son opcionales.

Tras actualizar el flujo de **importación**, el `README.md` pide **volver a desplegar**:

- `001_solicitud_hotldb.sql` (ítem 6)
- `001_reserva_hotldb.sql` (ítem 4)

## 2. Por módulo (si solo migrás una parte)

### Censo (solo este módulo, un archivo)

1. `Docs/datos/paquetes-ssms/Censo_ejecutar_HOTELDB.sql` en SSMS contra **HOTELDB** (equivale a `SGH.Infrastructure/Censo/Persistence/001_sp_censo.sql`).

### Cuenta / Login (un solo archivo)

1. `Docs/datos/paquetes-ssms/Login_ejecutar_HOTELDB.sql` en SSMS contra **HOTELDB**, **o** `SGH.Infrastructure/Cuenta/Persistence/001_sp_login.sql` (mismo contenido: ALTER arriba, SP abajo).

No ejecutar: `001_sp_login_borrador.sql` ni `002_sp_login_passwordv2_dual.sql` (obsoleto; unificado en `001`).

### Navegación (un solo archivo)

1. `Docs/datos/paquetes-ssms/Navegacion_ejecutar_HOTELDB.sql` en SSMS contra **HOTELDB**, **o** `SGH.Infrastructure/Navegacion/Persistence/001_navegacion_hotldb.sql`.

Incluye el SP de menú, alta de ítems Config habitaciones/baños y asignación `MenusPerfil` (perfil 1 y 68). Cambiar `@PerfilId` en el último bloque si aplica otro perfil.

Sincronizar paquete: `python3 tools/empaquetar-navegacion-ssms.py`.

### Habitación (CU-HAB — un solo archivo)

1. `Docs/datos/paquetes-ssms/Habitacion_ejecutar_HOTELDB.sql` en SSMS contra **HOTELDB**, **o** `SGH.Infrastructure/Habitacion/Persistence/001_habitacion_hotldb.sql` (mismo contenido: DDL arriba, `he_sp_*` abajo).

Sincronizar copia SSMS tras editar Persistence: `python3 tools/empaquetar-habitacion-ssms.py`.

### Reserva (un solo archivo)

1. `Docs/datos/paquetes-ssms/Reserva_ejecutar_HOTELDB.sql` en SSMS contra **HOTELDB**, **o** `SGH.Infrastructure/Reserva/Persistence/001_reserva_hotldb.sql`.

Sincronizar paquete: `python3 tools/empaquetar-reserva-ssms.py`.

### Censo

1. `001_sp_censo.sql`

### Solicitud (un solo archivo)

1. `Docs/datos/paquetes-ssms/Solicitud_ejecutar_HOTELDB.sql` en SSMS contra **HOTELDB**, **o** `SGH.Infrastructure/Solicitud/Persistence/001_solicitud_hotldb.sql`.

Incluye flujo estándar (buscar, pasajeros, habitaciones, generar reservas) e **importación masiva** (`asegurar_periodo`, `he_sp_reserva_finalizar_checkout`, `cargar_pasajero`). Desplegar **reserva** antes si aún no está.

```bash
python3 tools/empaquetar-solicitud-ssms.py
```

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