# Paquetes SQL para SSMS (sin usar `.sh`)

Para quien **no usa terminal**: un solo archivo por módulo, listo para **SQL Server Management Studio** o **Azure Data Studio**.

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

Cada procedimiento lleva cabecera interna:

```text
--  author: ...
--  date: ...
--  description: ...
```

Cuando cambie la lógica o el responsable, actualizar **author**, **date** y **description** en ambos archivos.