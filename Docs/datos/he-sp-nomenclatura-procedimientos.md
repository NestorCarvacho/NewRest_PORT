# Nomenclatura de procedimientos NewRest (HOTELDB)

## Convención

Los procedimientos **propios del proyecto** (capa `SGH.Infrastructure`, scripts versionados en el repositorio) se nombran:

```text
he_sp_{tabla}_{accion}
```

- **`he_`**: prefijo del proyecto (Hotel / NewRest en HOTELDB).
- **`sp_`**: procedimiento almacenado.
- **`{tabla}`** y **`{accion}`**: en minúsculas y **snake_case**, separados por guión bajo.

Ejemplos:

| Antes (legado en repo) | Ahora |
|------------------------|--------|
| `dbo.sp_Habitacion_Listar` | `dbo.he_sp_habitacion_listar` |
| `dbo.sp_Reserva_RegistrarMovimiento` | `dbo.he_sp_reserva_registrar_movimiento` |
| `dbo.sp_Usuario_ObtenerParaLogin` | `dbo.he_sp_usuario_obtener_para_login` |

En C# el repositorio usa el nombre completo en `NameProcedure`, por ejemplo `dbo.he_sp_habitacion_listar`. En el Mid, el campo `Procedimiento` de las opciones sigue esa misma cadena cuando documenta el SP asociado.

## Qué no cambia

Los procedimientos **legados** que ya existían en HOTELDB antes del wrapper NewRest conservan su nombre (`HAB_*`, `SOL_*`, `USU_*`, `RES_*`, etc.). Los scripts del repositorio pueden seguir invocándolos con `EXEC` desde dentro de un `he_sp_*`.

## Despliegue en base de datos

Los archivos `.sql` bajo `SGH.Infrastructure/**/Persistence/` declaran los objetos con `CREATE OR ALTER PROCEDURE dbo.he_sp_...`. Tras actualizar el código de la aplicación hay que **volver a ejecutar** esos scripts (orden en `Docs/datos/hoteldb-scripts-orden-ejecucion.md` o copias en `scripts-ejecutar/` vía `./tools/sincronizar-scripts-ejecutar.sh`).

Los procedimientos antiguos `dbo.sp_*` del mismo módulo pueden quedar huérfanos en la base hasta que un DBA los elimine; la aplicación ya no los referencia.

### Error «Could not find stored procedure 'he_sp_…'»

La **aplicación** ya llama `he_sp_*`, pero **HOTELDB** aún solo tiene `sp_*` (u otro nombre). No es un fallo de código: falta alinear la base.

1. **Recomendado:** ejecutar los scripts del repo en QA (login primero: `SGH.Infrastructure/Cuenta/Persistence/001_sp_login.sql` y, si aplica, `002_sp_login_passwordv2_dual.sql`).
2. **Puente rápido solo login:** `Docs/datos/migracion/he_sp_cuenta_login_puente_qa.sql` (sinónimos `he_sp_usuario_*` → `sp_Usuario_*` existentes).
3. **Puente completo módulos SGH:** `Docs/datos/migracion/he_sp_sinonimos_desde_sp_legacy.sql`.

Cuando ejecutes los `CREATE OR ALTER` del repo, elimina los sinónimos `he_sp_*` que apunten al legado antes de crear el procedimiento con el mismo nombre.

### Puente temporal en QA

Si en un ambiente aún existen solo los nombres `sp_*` y no se puede redesplegar de inmediato, usar `Docs/datos/migracion/he_sp_sinonimos_desde_sp_legacy.sql`: crea sinónimos `he_sp_*` que apuntan al `sp_*` homónimo existente. Cuando se ejecuten los scripts del repo, conviene **eliminar esos sinónimos** y crear los procedimientos reales con `CREATE OR ALTER` (los sinónimos bloquean un objeto con el mismo nombre).