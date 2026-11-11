# Directrices de desarrollo (SGH / NewRest)

Complemento de [`PROMPT-PRINCIPAL.md`](PROMPT-PRINCIPAL.md). Arquitectura: [`arquitectura-base.md`](arquitectura-base.md). Pantallas: [`flujo-mvc-mid.md`](flujo-mvc-mid.md).

## Fuentes por producto

- Reglas funcionales: `Docs/as-is/` e informe consolidado del repo (no sustituyen este paquete).
- Los AS-IS describen el legado; **no** copiar su arquitectura técnica al sistema nuevo.

## Capas

Misma dirección de dependencias que en `arquitectura-base.md`.

### Domain

- Por módulo: `Entities/`, `Interfaces/`, `Reglas*`.
- Sin EF, Dapper, ASP.NET, DTOs de pantalla ni repositorios.

### Application

- `DTOs/`, `Interfaces/`, `UseCases/`, `Mid/`, `Sesion/`, `DependencyInjection/`.
- Solo depende de Domain.

### Infrastructure

- `DependencyInjection/`, `Persistence/`, por módulo `Commands/`, `Queries/`, SQL en `Persistence/`.
- SP únicamente; registra implementaciones en `AddInfraestructura`.

### Web

- `Pages/`, `Controllers/` Mid, `Mid/RespuestaDeMid.cs`, `wwwroot/`.
- PageModel de cuenta sin reglas de negocio ni repositorios directos.

### Tests

- Por capa; dobles para SQL; sin BD en unitarias.
- Actualizar pruebas cuando cambie comportamiento observable.

## Acceso a datos

- Stored procedures; sin SQL inline en C#.
- Lecturas: Dapper + `CommandType.StoredProcedure`.
- Escrituras legado: `AD_Global` + `LIB_ControlResultado`.
- Nombres nuevos: `sp_[Entidad]_[Accion]`.

## Seguridad

- BCrypt para contraseñas nuevas; MD5 legado solo para verificar y migrar.
- Secretos en User Secrets / variables de entorno.
- Ver [`autorizacion-rbac.md`](autorizacion-rbac.md).

## Código C#

- Namespaces file-scoped; `record` para DTOs; `_camelCase` campos privados.
- I/O async con sufijo `Async`; sin `.Result`/`.Wait()`.
- Cambios acotados; log en excepciones manejadas.

## Verificación

```shell
dotnet build {Nombre}.sln
dotnet test {Nombre}.sln
```
