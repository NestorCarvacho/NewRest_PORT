# HOTELDB — Catálogo de procedimientos almacenados (consultas)

Complemento del [`Diccionario_de_datos_HotelDB.xlsx`](Diccionario_de_datos_HotelDB.xlsx) (hojas **`Tablas`** y **`Procedimientos`**, export SQL Server 2025 / HOTELDB). Ejecutar las consultas en **SSMS**, **Azure Data Studio** o `sqlcmd` contra la misma instancia que `ConnectionStrings:Hoteldb`.

**Inventario maestros SGH 2.1:** resultados ya volcados en [`maestros-listas-inventario.md`](maestros-listas-inventario.md) y [`extractos/`](extractos/).

**Objetivo SGH 2.1:** inventariar SP legado de **Listas**, **Empresas**, **Áreas**, **Divisiones**, **UN** y **Departamentos** antes de migrar CRUD.

**Scripts versionados en NewRest** (envoltorios `sp_*`): ver `SGH.Infrastructure/**/Persistence/*.sql`. No sustituyen el inventario completo de la base productiva.

---

## 1. Catálogo completo (procedimientos usuario)

Lista todos los SP y funciones con esquema, fechas y si dependen de otro objeto.

```sql
USE HOTELDB;
GO

SELECT
    s.name AS Esquema,
    o.name AS NombreProcedimiento,
    o.type_desc AS Tipo,
    o.create_date AS Creado,
    o.modify_date AS Modificado
FROM sys.objects AS o
INNER JOIN sys.schemas AS s ON s.schema_id = o.schema_id
WHERE o.type IN ('P', 'FN', 'IF', 'TF')  /* P = procedure; FN/IF/TF = funciones */
  AND o.is_ms_shipped = 0
ORDER BY s.name, o.name;
```

---

## 2. Filtro — prefijos típicos maestros y operación

Ajusta los `LIKE` si en tu HOTELDB hay otros prefijos legado.

```sql
USE HOTELDB;
GO

SELECT
    s.name AS Esquema,
    o.name AS NombreProcedimiento,
    o.modify_date AS Modificado
FROM sys.objects AS o
INNER JOIN sys.schemas AS s ON s.schema_id = o.schema_id
WHERE o.type = 'P'
  AND o.is_ms_shipped = 0
  AND (
        o.name LIKE 'sp_%'
     OR o.name LIKE 'INF_%'
     OR o.name LIKE 'SOL_%'
     OR o.name LIKE 'USU_%'
     OR o.name LIKE 'HAB_%'
     OR o.name LIKE 'CSO_%'
     OR o.name LIKE 'UNG_%'
     OR o.name LIKE 'DIV_%'
     OR o.name LIKE 'DEP_%'
     OR o.name LIKE 'LIS_%'
     OR o.name LIKE 'EMP_%'
     OR o.name LIKE 'ARE_%'
     OR o.name LIKE '%Lista%'
     OR o.name LIKE '%Empresa%'
     OR o.name LIKE '%Division%'
     OR o.name LIKE '%Departamento%'
     OR o.name LIKE '%Unidad%Negocio%'
  )
ORDER BY o.name;
```

**Referencias ya usadas en NewRest (solicitud / catálogo organización):**

| Procedimiento | Uso en repo |
|---------------|-------------|
| `UNG_ObtieneUnidadesNegocio` | `sp_Solicitud_ListarOrganizacion` |
| `DIV_ObtieneDivisiones` | íd. |
| `DEP_ObtieneDepartamentos` | íd. |
| `SOL_GuardaSolicitudAlojamiento` | guardar solicitud |
| `SOL_ObtieneSolicitudAlojamiento` | detalle |

---

## 3. Parámetros por procedimiento

Base para documentar contratos (nombres `@`, tipos, entrada/salida).

```sql
USE HOTELDB;
GO

SELECT
    s.name AS Esquema,
    o.name AS NombreProcedimiento,
    p.parameter_id AS Orden,
    p.name AS Parametro,
    TYPE_NAME(p.user_type_id) AS TipoSql,
    p.max_length AS LongitudMax,
    p.precision AS Precision,
    p.scale AS Escala,
    p.is_output AS EsSalida
FROM sys.objects AS o
INNER JOIN sys.schemas AS s ON s.schema_id = o.schema_id
INNER JOIN sys.parameters AS p ON p.object_id = o.object_id
WHERE o.type = 'P'
  AND o.is_ms_shipped = 0
ORDER BY s.name, o.name, p.parameter_id;
```

Filtro solo maestros (misma lista de prefijos que §2):

```sql
/* Añadir al WHERE de la consulta anterior: */
AND (
    o.name LIKE 'UNG_%' OR o.name LIKE 'DIV_%' OR o.name LIKE 'DEP_%'
    OR o.name LIKE 'LIS_%' OR o.name LIKE 'EMP_%' OR o.name LIKE 'ARE_%'
    OR o.name LIKE '%Lista%' OR o.name LIKE '%Empresa%'
)
```

---

## 4. SP que referencian tablas maestras

Encuentra procedimientos que mencionen tablas objetivo en su definición (heurística por texto).

```sql
USE HOTELDB;
GO

DECLARE @Tablas TABLE (Nombre sysname);
INSERT INTO @Tablas (Nombre) VALUES
    (N'Listas'),
    (N'Empresas'),
    (N'AreasEmpresa'),
    (N'Divisiones'),
    (N'UnidadesNegocio'),
    (N'Departamentos');

SELECT DISTINCT
    OBJECT_SCHEMA_NAME(m.object_id) AS Esquema,
    OBJECT_NAME(m.object_id) AS NombreProcedimiento,
    t.Nombre AS TablaReferida
FROM sys.sql_modules AS m
CROSS JOIN @Tablas AS t
WHERE m.definition LIKE N'%' + t.Nombre + N'%'
  AND OBJECTPROPERTY(m.object_id, 'IsProcedure') = 1
ORDER BY t.Nombre, NombreProcedimiento;
```

> Los nombres físicos en HOTELDB pueden diferir (`dbo.Empresa`, plural/singular). Amplía la tabla `@Tablas` tras revisar el xlsx.

---

## 5. Dependencias entre procedimientos

Quién llama a quién (útil para no romper cadenas al migrar).

```sql
USE HOTELDB;
GO

SELECT
    OBJECT_SCHEMA_NAME(d.referencing_id) AS EsquemaLlamador,
    OBJECT_NAME(d.referencing_id) AS ProcedimientoLlamador,
    COALESCE(
        d.referenced_schema_name + '.' + d.referenced_entity_name,
        d.referenced_entity_name
    ) AS ObjetoReferenciado,
    d.referenced_class_desc AS Clase
FROM sys.sql_expression_dependencies AS d
WHERE OBJECTPROPERTY(d.referencing_id, 'IsProcedure') = 1
  AND d.referenced_id IS NOT NULL
ORDER BY ProcedimientoLlamador, ObjetoReferenciado;
```

Ejemplo: `sp_Solicitud_ListarOrganizacion` → `UNG_ObtieneUnidadesNegocio`, `DIV_ObtieneDivisiones`, `DEP_ObtieneDepartamentos`.

---

## 6. Definición T-SQL (exportar script)

**Cuidado:** resultados grandes. Exportar a archivo, no pegar todo en el repo.

```sql
USE HOTELDB;
GO

SELECT
    s.name AS Esquema,
    o.name AS NombreProcedimiento,
    m.definition AS DefinicionSql
FROM sys.objects AS o
INNER JOIN sys.schemas AS s ON s.schema_id = o.schema_id
INNER JOIN sys.sql_modules AS m ON m.object_id = o.object_id
WHERE o.type = 'P'
  AND o.is_ms_shipped = 0
  AND o.name IN (
        N'UNG_ObtieneUnidadesNegocio',
        N'DIV_ObtieneDivisiones',
        N'DEP_ObtieneDepartamentos'
        /* ampliar lista */
  )
ORDER BY o.name;
```

Alternativa GUI: clic derecho sobre el procedimiento → **Script Procedure as** → **CREATE To**.

---

## 7. Comparar SP usados en código NewRest vs base

Lista estática extraída del repositorio (regenerar con búsqueda `NameProcedure` / Dapper en `SGH.Infrastructure`). Cruzar manualmente con §1.

| Procedimiento | Módulo |
|---------------|--------|
| `sp_Menu_ObtenerPorPerfil` | Navegación |
| `sp_Usuario_*` | Cuenta |
| `sp_Censo_*`, `HAB_ObtieneCensos`, `CSO_ObtieneCensoByUser` | Censo |
| `sp_Reserva_*` | Reserva |
| `sp_Solicitud_*`, `SOL_*`, `USU_ObtieneUsuarios` | Solicitud |
| `INF_*` | Informes censo |

Cualquier SP de **§2** que no esté en esta tabla es candidato a documentar en **SGH 2.1** (mantenedor legado).

---

## 8. Flujo recomendado de documentación

1. Ejecutar **§1** → exportar CSV/Excel (`Catalogo_SP_HOTELDB.csv`).
2. Ejecutar **§2** y **§4** → pegar resultados en [`maestros-listas-inventario.md`](maestros-listas-inventario.md) / hoja del diccionario.
3. Por cada SP crítico: **§3** (parámetros) + trozo de **§6** o script en `Docs/as-is/sql/`.
4. Marcar en inventario: **lectura / CUD / obsoleto / usado NewRest**.

---

## PowerShell (ejecutar consulta 1 con sqlcmd)

Sustituye servidor y autenticación. No guardes la cadena en archivos versionados.

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d HOTELDB -E -Q "SET NOCOUNT ON; SELECT s.name, o.name, o.modify_date FROM sys.objects o JOIN sys.schemas s ON s.schema_id = o.schema_id WHERE o.type = 'P' AND o.is_ms_shipped = 0 ORDER BY 2;" -s "|" -W -o Catalogo_SP_HOTELDB.txt
```

---

## Referencias

- [`diccionario-hoteldb-perfiles-menus.md`](diccionario-hoteldb-perfiles-menus.md)
- [`maestros-listas-inventario.md`](maestros-listas-inventario.md)
- [`../SGH-2.1-maestros-base.md`](../SGH-2.1-maestros-base.md)