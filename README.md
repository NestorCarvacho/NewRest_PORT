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

La solución se organiza con Clean Architecture en .NET 10, C# 14, ASP.NET Core Razor Pages y SQL Server:

```
SGH.sln
├── SGH.Domain           Entidades, value objects, enums, interfaces y excepciones de negocio
├── SGH.Application      DTOs, interfaces de servicios, casos de uso y validadores
├── SGH.Infrastructure   Persistencia, repositorios, seguridad e inyección de dependencias
├── SGH.Web              Interfaz Razor Pages, ViewModels y middlewares
└── SGH.Tests            Pruebas automatizadas
```

La arquitectura separa las reglas de negocio, los casos de uso, los adaptadores técnicos y la presentación. La solución usa procedimientos almacenados para interactuar con SQL Server: EF Core para escrituras y Dapper para lecturas. Las contraseñas nuevas o actualizadas se almacenan con BCrypt.

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

En **Reserva**, **Importar Excel** permite descargar una plantilla `.xlsx` y cargar varias reservas. La hoja debe incluir `Grupo Empresa`, `Turno`, `Fecha Reserva` y `Fecha Llegada`; `Fecha Salida` y `Observación` son opcionales. Los nombres de grupo y turno deben coincidir con los catálogos disponibles, y las fechas deben ser celdas de fecha o usar `yyyy-MM-dd` o `yyyy-MM-dd HH:mm`. Se importan las filas válidas y se informa el número y motivo de cada fila rechazada. No hay un límite adicional de filas impuesto por la aplicación; el tamaño práctico depende de los límites de la solicitud y de los recursos disponibles.

## Backend de solicitudes de alojamiento

El backend de solicitudes dispone de los handlers autenticados de Razor Pages en `/Solicitud/Datos`:

- `GET ?handler=Buscar`: filtros `desde`, `hasta`, `grupoId`, `solicitanteId`, `estado` y `numero`. Las fechas son obligatorias; `0` significa sin filtro en grupo, solicitante, estado y número.
- `GET ?handler=Detalle&id=...`: detalle de la solicitud.
- `GET ?handler=Pasajeros&id=...`: pasajeros de una solicitud perteneciente al mandante y campamento de la sesión.
- `GET ?handler=HabitacionesDisponibles&id=...`: disponibilidad calculada por el procedimiento legado para las fechas, empresa y campamento de la solicitud.
- `POST ?handler=AgregarCupos`: cuerpo JSON con `id` de solicitud y `cantidad` entre 1 y 999; crea cupos sin pasajero usando las fechas de la solicitud.
- `POST ?handler=EliminarPasajero`: cuerpo JSON con `id` de solicitud y `pasajeroId`; rechaza pasajeros de otras solicitudes. Ambas operaciones rechazan solicitudes reservadas o anuladas.
- `POST ?handler=GuardarPasajero`: cuerpo JSON con `solicitudId`, `pasajeroId` (`0` para alta), `usuarioPasajeroId`, `fechaCheckIn` y `fechaCheckOut` opcional. El usuario debe pertenecer al catálogo activo del grupo.
- `POST ?handler=AsignarHabitacion`: cuerpo JSON con `solicitudId`, `pasajeroId` y `habitacionId`. Comprueba la disponibilidad legada y descuenta las asignaciones pendientes de la misma solicitud.
- `POST ?handler=GenerarReservas`: cuerpo JSON con `id` de solicitud. Exige solicitud pendiente, al menos un pasajero o cupo y habitación para cada uno; vuelve a consultar disponibilidad, ejecuta la validación legada y, si no hay conflictos, genera las reservas en una transacción.
- `GET ?handler=PlantillaPasajeros` y `POST ?handler=CargarPasajeros&id=...`: descarga de plantilla y carga de archivo `.xlsx` en `archivo`. La hoja `pasajeros` requiere las columnas `Identificador`, `TipoIdentificador`, `Nombres`, `Apellidos` y `Genero` (`M` o `F`). Las filas se procesan individualmente y se informan sus resultados.
- `POST ?handler=NotificarReservas`: reintenta enviar el correo para una solicitud con reservas generadas.
- `GET ?handler=Catalogo&grupoId=...`: grupos, estados, solicitantes activos y turnos. El grupo incluye su turno, unidad de negocio y contrato por defecto; `grupoId=0` consulta los solicitantes sin restringir por grupo.
- `GET ?handler=Organizacion&unidadId=...&divisionId=...`: unidades de negocio de la minera del mandante, divisiones de la unidad y departamentos de la división. Use `0` cuando aún no se ha seleccionado unidad o división.
- `POST ?handler=Crear`, `Actualizar` y `Anular`: cuerpos JSON; la actualización y la anulación requieren el `id`. Los POST de Razor Pages requieren el token antifalsificación.

El mandante, campamento y usuario se toman de la sesión, no del cuerpo. Las consultas y modificaciones se limitan al mandante y campamento de la sesión, una restricción adicional respecto del listado legado, que filtraba solo por mandante. La anulación conserva el cambio al estado `A` del legado. El alta requiere correo válido, grupo con turno por defecto y fecha de salida cuando se usan turnos; la unidad, división y departamento pueden ser obligatorios según la configuración del grupo. En edición, el procedimiento legado modifica los campos de contacto y organización, pero no las fechas, el grupo ni el turno.

Antes de usar estos handlers, ejecutar en la base de destino el script [001_sp_solicitud.sql](SGH.Infrastructure/Solicitud/Persistence/001_sp_solicitud.sql). Este reutiliza `SOL_GuardaSolicitudAlojamiento`; el detalle y los solicitantes reutilizan `SOL_ObtieneSolicitudAlojamiento` y `USU_ObtieneUsuarios`. Los catálogos organizacionales reutilizan `UNG_ObtieneUnidadesNegocio`, `DIV_ObtieneDivisiones` y `DEP_ObtieneDepartamentos`. El listado, la anulación, la primera reserva generada y la comprobación transaccional de alcance requieren procedimientos nuevos porque el legado hacía esas operaciones directamente sobre entidades. Verificar los contratos y permisos de los procedimientos legados en cada base antes del despliegue.

El menú **Gestión > Solicitud alojamiento** permite filtrar, crear, editar y anular solicitudes; administrar pasajeros y cupos; importar pasajeros, asignar habitaciones y generar reservas. La generación requiere que las habitaciones estén asignadas previamente. La importación conserva el comportamiento legado: puede crear usuarios o actualizar nombres, género, grupo y empresa de usuarios existentes; la pantalla pide confirmación antes de importarlos. Si una carga se interrumpe, algunas filas pueden haber sido procesadas y deben revisarse antes de repetirla.

En el detalle de una solicitud pendiente se pueden seleccionar pasajeros individualmente o todos a la vez. La selección habilita la eliminación y la asignación de habitaciones, así como la generación de reservas para la solicitud completa. La eliminación y la asignación se realizan una por una: si alguna operación falla, se informa cuántas se completaron y se actualiza el detalle. El flujo todavía no incluye la exportación del informe ni la creación individual de usuarios desde la solicitud.

Para consultar pasajeros, ejecutar también [002_sp_pasajeros_solicitud.sql](SGH.Infrastructure/Solicitud/Persistence/002_sp_pasajeros_solicitud.sql). Este procedimiento comprueba el alcance y reutiliza `SOL_ObtienePasajerosSolicitudAlojamiento`.

Para guardar pasajeros identificados y generar reservas, ejecutar [003_sp_guardar_pasajero.sql](SGH.Infrastructure/Solicitud/Persistence/003_sp_guardar_pasajero.sql), [005_sp_habitaciones_solicitud.sql](SGH.Infrastructure/Solicitud/Persistence/005_sp_habitaciones_solicitud.sql) y [004_sp_generar_reservas.sql](SGH.Infrastructure/Solicitud/Persistence/004_sp_generar_reservas.sql). Reutilizan `SOL_GuardaPasajeroSolicitudAlojamiento`, `HAB_ValidaCreacionReserva_Solicitud` y `RES_CreaReserva`, respectivamente. La generación informa el primer conflicto legado.

Para consultar y asignar habitaciones, ejecutar [005_sp_habitaciones_solicitud.sql](SGH.Infrastructure/Solicitud/Persistence/005_sp_habitaciones_solicitud.sql), que reutiliza `HAB_ObtieneHabitacionesDisponiblesV2` y `SOL_AsignaHabitacionPasajeroSolicitudEmpresa`. El cálculo legado no cuenta asignaciones pendientes de otras solicitudes de alojamiento; el bloqueo de la nueva aplicación solo coordina sus propias asignaciones y generaciones, no escrituras realizadas por otros sistemas.

Para importar pasajeros, ejecutar [006_sp_cargar_pasajero.sql](SGH.Infrastructure/Solicitud/Persistence/006_sp_cargar_pasajero.sql), que reutiliza `SOL_GuardaPasajeroSolicitudAlojamientoMasivo` y revierte los cambios de cada fila rechazada.

El correo se envía después de confirmar las reservas. Si falla o no está configurado, la pantalla informa que las reservas ya fueron generadas y ofrece volver a enviar el correo desde el detalle; no se vuelven a generar las reservas. Configure `CorreoSolicitudes:Habilitado`, `Host`, `Puerto`, `Ssl` y `Remitente`, y, si el servidor requiere autenticación, `Usuario` y `Contrasena`. Guarde las credenciales en User Secrets o variables de entorno, no en archivos del repositorio. La notificación no se reintenta automáticamente.
