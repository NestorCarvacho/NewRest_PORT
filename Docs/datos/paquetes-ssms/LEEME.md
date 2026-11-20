# Paquetes SQL para SSMS (sin usar `.sh`)

Para quien **no usa terminal**: un solo archivo por módulo, listo para **SQL Server Management Studio** o **Azure Data Studio**.

## Habitación (CU-HAB) — qué ejecutar

| Archivo | Base de datos | Cuándo |
|---------|---------------|--------|
| `Habitacion_ejecutar_HOTELDB.sql` | **HOTELDB** | Un solo F5: tablas, columnas y todos los `he_sp_*` del módulo |

Fuente en el repo: `SGH.Infrastructure/Habitacion/Persistence/001_habitacion_hotldb.sql` (único archivo del módulo).

Tras editar Persistence, copiar al paquete:

```bash
python3 tools/empaquetar-habitacion-ssms.py
```

## Navegación / menú — qué ejecutar

| Archivo | Base de datos | Cuándo |
|---------|---------------|--------|
| `Navegacion_ejecutar_HOTELDB.sql` | **HOTELDB** | Menú por perfil + ítems Config habitaciones/baños y `MenusPerfil` |

Fuente: `SGH.Infrastructure/Navegacion/Persistence/001_navegacion_hotldb.sql`.

```bash
python3 tools/empaquetar-navegacion-ssms.py
```

## Login / Cuenta — qué ejecutar

| Archivo | Base de datos | Cuándo |
|---------|---------------|--------|
| `Login_ejecutar_HOTELDB.sql` | **HOTELDB** | Instalación o actualización login: columnas, BCrypt (`PasswordV2`), bloqueo e `he_sp_usuario_*` en un solo F5 |

Origen: `SGH.Infrastructure/Cuenta/Persistence/001_sp_login.sql` (reemplaza el antiguo parche `002_sp_login_passwordv2_dual.sql`).

Estructura del archivo:

1. **Sección 1** — `ALTER TABLE` en `Usuarios` / `HistoricoPassword`
2. **Sección 2** — procedimientos con cabecera `author` / `date` / `description`

Requisitos: procedimientos legados `USU_ValidaEstadoUsuario`, `USU_ObtieneCampamentosYPerfilesUsuario`, tablas `Usuarios`, `HistoricoPassword`, `Listas`.

## Censo — qué ejecutar

| Archivo | Base de datos | Cuándo |
|---------|---------------|--------|
| `Censo_ejecutar_HOTELDB.sql` | **HOTELDB** (la misma de `ConnectionStrings:Hoteldb`) | Después de desplegar la app con cambios de censo, o si fallan listado/catálogo/guardar por SP inexistente |

**No hace falta** correr `tools/sincronizar-scripts-ejecutar.sh`. Este `.sql` es copia del script maestro del repo:

`SGH.Infrastructure/Censo/Persistence/001_sp_censo.sql`

### Pasos en SSMS

1. Conectar al servidor de QA (ej. instancia donde está HOTELDB).
2. En el selector de base, elegir **HOTELDB**.
3. **Archivo → Abrir** → `Censo_ejecutar_HOTELDB.sql`.
4. Menú **Consulta → UTF-8** si los comentarios con tildes se ven mal.
5. **Ejecutar** (F5). Debe terminar sin errores en rojo.
6. Opcional comprobar:

```sql
SELECT name FROM sys.procedures
WHERE name LIKE 'he_sp_censo%'
ORDER BY name;
```

Deberían existir: `he_sp_censo_buscar`, `he_sp_censo_buscar_por_usuario`, `he_sp_censo_listar_catalogo`, `he_sp_censo_filtro_pertenece`, `he_sp_censo_guardar`.

### Requisitos previos en HOTELDB

- Procedimientos **legados** ya instalados: `HAB_ObtieneCensos`, `CSO_GuardaCenso`, `CSO_ObtieneCensoByUser`.
- Tablas habituales del legado: `Pabellones`, `Modulos`, `Habitaciones`.

### Mantenimiento

Si se edita `001_sp_censo.sql` en el código, **regenerar** este paquete copiando el contenido (o pedir al dev que actualice el archivo en el mismo commit).

En SQL solo quedan comentarios de **auditoría** por procedimiento (`author`, `date`, `description`). Para limpiar otros comentarios: `python3 tools/strip_sql_comments.py <archivos.sql>`.