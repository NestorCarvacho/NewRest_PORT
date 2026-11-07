# Directrices de Copilot para SGH

## Fuentes del proyecto

- Antes de implementar o modificar una regla funcional, consulta los documentos del módulo en [`Docs/as-is/`](../Docs/as-is/) y el informe [`INFORME_AS_IS_CONSOLIDADO.md`](../Docs/as-is/INFORME_AS_IS_CONSOLIDADO.md). Conserva las reglas, validaciones y flujos documentados; no inventes comportamiento cuando la documentación no lo especifica.
- La arquitectura, persistencia, seguridad y convenciones obligatorias están en [`instrucciones_del_arquitectura.md`](../Docs/instrucciones/instrucciones_del_arquitectura.md). Estas directrices resumen sus requisitos principales; consulta el documento ante cualquier duda.
- Los documentos AS-IS describen el sistema legado y su comportamiento, no autorizan a copiar su arquitectura técnica al nuevo sistema.

## Arquitectura y responsabilidades por capa

La solución usa .NET 10 / C# 14, Razor Pages, SQL Server y Clean Architecture. Ubica cada archivo en el proyecto y carpeta de su responsabilidad. Mantén la dirección de dependencias: `Web -> Application -> Domain` y `Infrastructure -> Application + Domain`; Domain no depende de otras capas. No agregues referencias entre proyectos que inviertan esta dirección.

### `SGH.Domain` — reglas y modelo del negocio

- Ubica cada módulo en su carpeta, por ejemplo `Cuenta/`. Dentro de esa carpeta: entidades en `Entities/`, enums en `Enums/`, objetos de valor en `ValueObjects/`, excepciones de negocio en `Exceptions/` e interfaces del dominio en `Interfaces/`.
- Contiene reglas e invariantes del negocio independientes de la UI, SQL y frameworks.
- No debe referenciar EF Core, Dapper, ASP.NET Core, Infrastructure ni librerías de presentación.
- No pongas aquí DTOs de pantalla, consultas SQL ni implementaciones de repositorio.

### `SGH.Application` — casos de uso y coordinación

- Ubica cada módulo en su carpeta, por ejemplo `Cuenta/`. Dentro de esa carpeta: modelos de transferencia en `DTOs/`, contratos de casos de uso en `Interfaces/`, casos de uso en `UseCases/`, validaciones en `Validators/` y opciones del módulo en `Opciones/`.
- Coordina las operaciones de negocio usando Domain y abstracciones; define contratos que implementará Infrastructure.
- Depende solo de Domain. No accede directamente a SQL Server, EF Core, Dapper, Razor Pages ni detalles de infraestructura.
- Expón resultados como DTOs; valida entradas antes de ejecutar operaciones y mantén los casos de uso enfocados en una operación/responsabilidad.

### `SGH.Infrastructure` — adaptadores y servicios técnicos

- Ubica el registro de servicios en `DependencyInjection/` y la conexión compartida en `Persistence/`. Cada módulo tiene su carpeta, por ejemplo `Cuenta/`, con comandos en `Commands/`, lecturas en `Queries/`, scripts del módulo en `Persistence/` y servicios criptográficos en `Security/`.
- Implementa los contratos de Application/Domain y contiene detalles de EF Core, Dapper, SQL Server, servicios externos y hashing.
- Depende de Application y Domain. No pongas reglas de presentación aquí ni hagas que Application dependa de esta capa.
- Los comandos llaman SP mediante EF Core; las lecturas llaman SP mediante Dapper. Registra aquí la implementación de las dependencias y expón un método de registro para que Web componga la aplicación.

### `SGH.Web` — presentación Razor Pages

- Ubica páginas y sus `PageModel` en `SGH.Web/Pages/`, modelos específicos de la UI en `ViewModels/`, middleware en `Middlewares/` y recursos estáticos en `wwwroot/`.
- Los `PageModel` validan/modelan la interacción web y delegan operaciones a casos de uso o servicios de Application. No contienen reglas de negocio ni acceden al DbContext/repositorios directamente.
- Usa ViewModels/InputModels propios para formularios y mapea los resultados de Application para mostrarlos. No expongas entidades de persistencia directamente a las vistas.
- Configura autenticación/autorización, middleware, manejo global de errores y composición de dependencias en el punto de entrada de Web, manteniendo las implementaciones técnicas en Infrastructure.

### `SGH.Tests` — verificación automatizada

- Coloca pruebas en el proyecto de pruebas, organizadas por capa o comportamiento bajo prueba.
- Prueba reglas del dominio y resultados de casos de uso; para dependencias externas usa dobles de prueba apropiados. No requieras una base de datos real para pruebas unitarias.
- Agrega o actualiza pruebas cuando cambie un comportamiento observable o una regla de negocio.

### `SGH.Demo` — prototipo visual

- Es una aplicación independiente de demostración con datos de muestra para visualizar pantallas. No representa lógica de negocio implementada ni debe conectarse a SQL Server o convertirse en un bypass de la arquitectura.
- Los filtros del demo se presentan en paneles laterales, siguiendo el patrón visual de los formularios laterales.
- El estado de los filtros es independiente por solapa: los filtros aplicados en una solapa se conservan al cambiar a otra y volver, sin afectar sus filtros. Se mantienen hasta cerrar sesión o actualizar la página.

## Acceso a datos

- Toda interacción con SQL Server se realiza mediante Stored Procedures; no escribas SQL inline en C#.
- Usa EF Core 10 para comandos de escritura (crear, actualizar y eliminar), mediante `ExecuteSqlInterpolatedAsync` o el mapeo correspondiente.
- Usa Dapper para lecturas e indica `CommandType.StoredProcedure`.
- Nombra los procedimientos `sp_[Entidad]_[Accion]`, por ejemplo `sp_Usuario_ActualizarPassword`.
- Mantén interfaces de lectura y escritura pequeñas y separadas cuando corresponda.

## Seguridad y autenticación

- Las contraseñas nuevas o actualizadas se almacenan con BCrypt; nunca uses MD5 para nuevos hashes.
- Para hashes heredados MD5, verifica la contraseña y, si es válida, reemplaza el hash por BCrypt mediante el procedimiento almacenado definido para ello.
- No registres credenciales ni secretos en el repositorio. Usa configuración segura, como User Secrets en desarrollo.

## Código C# y calidad

- Usa namespaces file-scoped, nombres PascalCase para tipos/métodos e interfaces `IPascalCase`; usa `_camelCase` para campos privados.
- Usa `record` para DTOs/InputModels inmutables. Usa `var` cuando el tipo resulte evidente.
- Los métodos de I/O deben ser asíncronos, usar `async`/`await` y terminar en `Async`. No uses `.Result` ni `.Wait()`.
- Aplica SOLID y reutiliza patrones existentes. Evita capturas vacías o silenciosas; registra las excepciones manejadas con `ILogger`.
- Mantén los cambios acotados, actualiza la documentación relacionada y agrega o adapta pruebas para el comportamiento modificado.

## Verificación

Desde la raíz de la solución, ejecuta:

```shell
dotnet build SGH.sln
dotnet test SGH.sln
```

Al finalizar, informa los cambios y los comandos de validación ejecutados, indicando claramente cualquier prueba que no se haya podido correr.
