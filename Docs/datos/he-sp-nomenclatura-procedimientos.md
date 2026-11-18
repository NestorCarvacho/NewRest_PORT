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

### Comparar QA vs local

Ejecutá en **cada** servidor (misma consulta, distinta conexión) el script  
`Docs/datos/migracion/he_sp_inventario_comparar.sql` y guardá el primer resultado (`servidor`, `legado`, `nuevo`, `estado_newrest`).

- **SSMS:** resultado a Excel/CSV por entorno.
- **sqlcmd:**

```bash
sqlcmd -S <QA> -d HOTELDB -E -i Docs/datos/migracion/he_sp_inventario_comparar.sql -s ";" -W -h-1 -o inventario_qa.csv
sqlcmd -S "(localdb)\MSSQLLocalDB" -d HOTELDB -E -i Docs/datos/migracion/he_sp_inventario_comparar.sql -s ";" -W -h-1 -o inventario_local.csv
```

En Excel: pegar ambas tablas y comparar por columna `nuevo` y `estado_newrest`. Lo que la app necesita es `estado_newrest = OK app (procedimiento)` en QA.

### Error «Could not find stored procedure 'he_sp_…'»

La **aplicación** ya llama `he_sp_*`, pero **HOTELDB** aún solo tiene `sp_*` (u otro nombre). No es un fallo de código: falta alinear la base.

1. **Renombrar rápido (recomendado en SSMS):** `Docs/datos/migracion/he_sp_renombrar_rapido.sql` — un solo archivo con **PREVIEW** + bucle `sp_rename` (52 pares). Alternativa expandida: `he_sp_renombrar_desde_sp_legacy.sql`.  
   Luego **re-ejecutar** los `.sql` de `SGH.Infrastructure/**/Persistence/` (orden en `hoteldb-scripts-orden-ejecucion.md`) para actualizar `EXEC` internos que aún citen `sp_*`.
2. **O crear desde cero:** ejecutar los scripts del repo con `CREATE OR ALTER` (login primero: `Cuenta/Persistence/001_sp_login.sql` y `002_sp_login_passwordv2_dual.sql` si aplica).
3. **Solo emergencia:** sinónimos (`he_sp_cuenta_login_puente_qa.sql` o `he_sp_sinonimos_desde_sp_legacy.sql`); no sustituye el rename ni el despliegue real.

Si ya creaste sinónimos `he_sp_*`, el script de **rename** los elimina antes de renombrar el procedimiento legado.

### Puente temporal en QA (sinónimos)

Solo si no podés renombrar ni ejecutar scripts aún: `Docs/datos/migracion/he_sp_sinonimos_desde_sp_legacy.sql`. Al migrar en serio, borrá los sinónimos y usá **rename** o `CREATE OR ALTER`.