# SGH26 — Sistema de Gestión de Hotelería

SGH es el desarrollo de una plataforma para gestionar la operación hotelera de campamentos: su infraestructura y ocupación, las solicitudes y reservas de alojamiento, los servicios operativos y los recursos asociados. El alcance funcional se basa en los módulos documentados del sistema actual y busca conservar sus reglas y flujos relevantes mientras se construye una solución con arquitectura desacoplada.

El alcance funcional previsto toma como base el comportamiento del sistema hotelero existente y considera 50 módulos. Ese alcance orienta el desarrollo, pero no significa que todos los módulos ya estén implementados en esta nueva solución.

## Alcance funcional previsto

Los módulos documentados se agrupan en las siguientes áreas:

- **Administración y organización:** campamentos; empresas, áreas y mandantes; departamentos, divisiones y unidades de negocio; pabellones, módulos, tipos de módulo, habitaciones, tipos de instalación y plantillas de ubicación; proyectos, grupos y actividades.
- **Acceso y configuración:** autenticación, usuarios, perfiles y menús, módulos, listas maestras, turnos y política de aceptación.
- **Alojamiento y ocupación:** solicitudes de alojamiento y sus pasajeros, reservas, reserva de instalaciones, censos, informes de disponibilidad y ocupabilidad, y desalojos con sus objetos y documentos.
- **Aseo y operación de habitaciones:** checklists, grupos y actividades de aseo, supervisión de aseo y estándares de validación de habitación.
- **Activos, artículos e inventarios:** activos; artículos, grupos, estados y precios; registro de artículos y registro por habitación; inventarios de habitación e ítems de inventario.
- **Elementos de protección personal (EPP):** casilleros, sus áreas y secciones, e inventario EPP.
- **Mantención, calidad y servicios:** mantenciones, órdenes de servicio y de trabajo; desviaciones, no conformidades, propuestas y seguimiento; comentarios y observaciones ESS; valijas y entregas de valija.

## Estado y enfoque técnico

La solución usa Clean Architecture en .NET 10, C# 14, ASP.NET Core (Razor Pages + controladores Mid) y SQL Server.

**Prompt y arquitectura para el equipo (compartir o copiar a proyectos nuevos):** [`Docs/prompt-proyecto/`](Docs/prompt-proyecto/README.md) — empezar por [`PROMPT-PRINCIPAL.md`](Docs/prompt-proyecto/PROMPT-PRINCIPAL.md).

```
SGH.sln
├── SGH.Domain           Reglas de negocio, entidades e interfaces de dominio
├── SGH.Application      DTOs, casos de uso, Mid y alcance de sesión
├── SGH.Infrastructure   Procedimientos almacenados, Dapper, comandos e inyección de dependencias
├── SGH.Web              Razor Pages, controladores Mid, wwwroot y autenticación
└── SGH.Tests            Pruebas automatizadas
```

Las reglas de negocio, la orquestación (Mid), la persistencia por SP y la presentación están separadas. Lecturas con Dapper; escrituras con `AD_Global` y SP donde aplica el legado. Contraseñas nuevas con BCrypt.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)

## Comandos

```powershell
dotnet restore SGH.sln
dotnet build SGH.sln
dotnet test SGH.sln
dotnet run --project SGH.Web
```

En GitHub Actions, `CI` compila la solución y `Tests` ejecuta las pruebas por separado. El workflow de tests se activa en pushes, pull requests y manualmente; incluye una prueba temporal sencilla para comprobar su funcionamiento.

## Idioma e importación de reservas

La interfaz ofrece español, inglés, portugués y francés. Inicialmente usa el idioma preferido del navegador; el selector de idioma está disponible en el acceso y en la aplicación, y recuerda la elección.

El interruptor con sol y luna permite elegir entre tema claro y oscuro tanto en el acceso como en la aplicación. La elección se guarda en el navegador; si no existe una preferencia, se usa el tema oscuro.

## Backend por Mid (POST + `opcion`)

El escritorio en `/` consume JSON con token antifalsificación. Cada módulo expone un único POST y devuelve `{ "registros": ... }` o `{ "registros": { "resultado": false, "mensaje": "..." } }`.

Los errores de los módulos se muestran como avisos en la esquina inferior derecha. En las importaciones, el aviso se complementa con el detalle de las filas no procesadas.

| Ruta | Opciones principales |
|------|----------------------|
| `/Reserva/Mid/MidReservas.cs` | `listar`, `catalogo`, `crear`, `editar`, `movimiento`, `importar` (multipart), plantilla en `/Reserva/PlantillaReservas` |
| `/Censo/Mid/MidCenso.cs` | `listar`, `miscensos`, `catalogo`, `guardar`, `guardarqr` |
| `/Solicitud/Mid/MidSolicitudes.cs` | `listar`, `catalogo`, `organizacion`, `detalle`, `pasajeros`, `habitaciones`, `crear`, `editar`, `anular`, `cupos`, `pasajero`, `eliminar`, `asignar`, `generar`, `notificar`, `cargar` (multipart), plantilla en `/Solicitud/PlantillaPasajeros` |
| `/Censo/Mid/MidInformesCenso.cs` | `gestiondiaria`, `informegeneral`, `ocupabilidad`, `consultar`, `descargar`, `consultarservicio`, `descargarservicio` |

Las reglas y procedimientos del legado se conservan; el mandante, campamento y usuario salen de la sesión del servidor.

La tabla de Reserva muestra solo la fecha de llegada y salida previstas, y la fecha y hora de los movimientos efectivos de check-in y check-out. Las acciones de check-in y check-out están disponibles en cada reserva según su estado y se identifican con iconos de entrada y salida; al pasar el cursor se muestra su nombre. Para consultar movimientos ya realizados, abra Filtros, seleccione «Asignada con Checkout» y aplique la búsqueda. La carga inicial comprende los últimos 30 días de fecha de reserva; al aplicar filtros, deje vacías las fechas para no restringir la búsqueda.

El mandante, campamento y usuario se toman de la sesión, no del cuerpo. Las consultas y modificaciones se limitan al mandante y campamento de la sesión, una restricción adicional respecto del listado legado, que filtraba solo por mandante. La anulación conserva el cambio al estado `A` del legado. El alta requiere correo válido, grupo con turno por defecto y fecha de salida cuando se usan turnos; la unidad, división y departamento pueden ser obligatorios según la configuración del grupo. En edición, el procedimiento legado modifica los campos de contacto y organización, pero no las fechas, el grupo ni el turno.

Antes de usar estos handlers, ejecutar en la base de destino el script [001_sp_solicitud.sql](SGH.Infrastructure/Solicitud/Persistence/001_sp_solicitud.sql). Este reutiliza `SOL_GuardaSolicitudAlojamiento`; el detalle y los solicitantes reutilizan `SOL_ObtieneSolicitudAlojamiento` y `USU_ObtieneUsuarios`. Los catálogos organizacionales reutilizan `UNG_ObtieneUnidadesNegocio`, `DIV_ObtieneDivisiones` y `DEP_ObtieneDepartamentos`. El listado, la anulación, la primera reserva generada y la comprobación transaccional de alcance requieren procedimientos nuevos porque el legado hacía esas operaciones directamente sobre entidades. Verificar los contratos y permisos de los procedimientos legados en cada base antes del despliegue.

El menú **Gestión > Solicitud alojamiento** permite filtrar, crear, editar y anular solicitudes; administrar pasajeros y cupos; importar pasajeros, asignar habitaciones y generar reservas. El listado se ordena inicialmente por fecha de solicitud descendente y, para una misma fecha, por número descendente; el usuario puede cambiar el orden desde las columnas. La generación requiere que las habitaciones estén asignadas previamente. La importación conserva el comportamiento legado: puede crear usuarios o actualizar nombres, género, grupo y empresa de usuarios existentes; la pantalla pide confirmación antes de importarlos. Si una carga se interrumpe, algunas filas pueden haber sido procesadas y deben revisarse antes de repetirla.

En el detalle de una solicitud pendiente se pueden seleccionar pasajeros individualmente o todos a la vez. La selección habilita la eliminación y la asignación de habitaciones, así como la generación de reservas para la solicitud completa. La eliminación y la asignación se realizan una por una: si alguna operación falla, se informa cuántas se completaron y se actualiza el detalle. El flujo todavía no incluye la exportación del informe ni la creación individual de usuarios desde la solicitud.

Al asignar habitaciones, el número de camas disponibles descuenta las preasignaciones de la solicitud y las selecciones realizadas en el panel. El indicador es verde cuando todas las camas están disponibles, amarillo cuando solo quedan algunas y rojo cuando no queda ninguna. Use el contador de cada habitación para asignar camas a los pasajeros en orden; el panel impide superar la disponibilidad o la cantidad de pasajeros. La disponibilidad mostrada es provisional y el servidor vuelve a validarla al guardar.

**Asignar automático** conserva las selecciones previas y completa los pasajeros restantes usando las habitaciones visibles, en orden, antes de guardar inmediatamente. Si la selección no alcanza para todos, no se envían asignaciones y se informa cuántos pasajeros quedan pendientes. Al guardar, las operaciones siguen siendo individuales: un cambio de disponibilidad u otro error del servidor puede dejar asignaciones parciales.

Para consultar pasajeros, ejecutar también [002_sp_pasajeros_solicitud.sql](SGH.Infrastructure/Solicitud/Persistence/002_sp_pasajeros_solicitud.sql). Este procedimiento comprueba el alcance y reutiliza `SOL_ObtienePasajerosSolicitudAlojamiento`.

Para guardar pasajeros identificados y generar reservas, ejecutar [003_sp_guardar_pasajero.sql](SGH.Infrastructure/Solicitud/Persistence/003_sp_guardar_pasajero.sql), [005_sp_habitaciones_solicitud.sql](SGH.Infrastructure/Solicitud/Persistence/005_sp_habitaciones_solicitud.sql) y [004_sp_generar_reservas.sql](SGH.Infrastructure/Solicitud/Persistence/004_sp_generar_reservas.sql). Reutilizan `SOL_GuardaPasajeroSolicitudAlojamiento`, `HAB_ValidaCreacionReserva_Solicitud` y `RES_CreaReserva`, respectivamente. La generación informa el primer conflicto legado.

Al actualizar estos procedimientos, abrir los scripts como UTF-8 para conservar los mensajes con caracteres acentuados; los cambios en los archivos no actualizan automáticamente la base de datos.

Para consultar, asignar y desasignar habitaciones, ejecutar [005_sp_habitaciones_solicitud.sql](SGH.Infrastructure/Solicitud/Persistence/005_sp_habitaciones_solicitud.sql), que reutiliza `HAB_ObtieneHabitacionesDisponiblesV2` y `SOL_AsignaHabitacionPasajeroSolicitudEmpresa` (asignar con id de habitación; desasignar con `@habitacion = NULL` — ver `Docs/as-is/sol-asigna-habitacion-pasajero.md`). El cálculo legado no cuenta asignaciones pendientes de otras solicitudes de alojamiento; el bloqueo de la nueva aplicación solo coordina sus propias asignaciones y generaciones, no escrituras realizadas por otros sistemas.

Para importar pasajeros, ejecutar primero [007_sp_solicitud_asegurar_periodo.sql](SGH.Infrastructure/Solicitud/Persistence/007_sp_solicitud_asegurar_periodo.sql) y [008_sp_reserva_finalizar_checkout.sql](SGH.Infrastructure/Solicitud/Persistence/008_sp_reserva_finalizar_checkout.sql), luego [006_sp_cargar_pasajero.sql](SGH.Infrastructure/Solicitud/Persistence/006_sp_cargar_pasajero.sql) (reutiliza `SOL_GuardaPasajeroSolicitudAlojamientoMasivo`). Volver a desplegar [001_sp_solicitud.sql](SGH.Infrastructure/Solicitud/Persistence/001_sp_solicitud.sql), [004_sp_generar_reservas.sql](SGH.Infrastructure/Solicitud/Persistence/004_sp_generar_reservas.sql) y el bloque `he_sp_reserva_registrar_movimiento` en [001_reserva_hotldb.sql](SGH.Infrastructure/Reserva/Persistence/001_reserva_hotldb.sql).

El correo se envía después de confirmar las reservas. Si falla o no está configurado, la pantalla informa que las reservas ya fueron generadas y ofrece volver a enviar el correo desde el detalle; no se vuelven a generar las reservas. Configure `CorreoSolicitudes:Habilitado`, `Host`, `Puerto`, `Ssl` y `Remitente`, y, si el servidor requiere autenticación, `Usuario` y `Contrasena`. Guarde credenciales en `secrets.json` (ver `secrets.example.json`), User Secrets o variables de entorno (`CorreoSolicitudes__Contrasena`). Ver `Docs/prompt-proyecto/configuracion-secretos.md`. La notificación no se reintenta automáticamente.
