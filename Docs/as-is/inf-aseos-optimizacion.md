# HOTELDB — `INF_Aseos` (optimización)

Procedimiento legado de informe de aseos. El script canónico optimizado está en [`sql/INF_Aseos_optimizado.sql`](sql/INF_Aseos_optimizado.sql). Contrastar en SSMS con el `INF_Aseos` actual antes de reemplazar en producción.

## Problemas del texto as-is

| Tema | Impacto |
|------|---------|
| **Filtro de fechas con `OR`** sobre `FechaCreacion` y `FechaInicioAseo` | Suele impedir un seek limpio en índices de fecha; conviene filtrar `Aseo` antes de explotar filas por checklist. |
| **Subconsulta agregada** `MAX(FechaCreacion)` sobre todo `SupervisionAseo` | Escaneo global; mejor `OUTER APPLY TOP (1) … ORDER BY FechaCreacion DESC` por `AseoId`. |
| **`#campamentos` siempre** y luego `DELETE` si `@campamento > 0` | Trabajo extra; con campamento fijo basta un filtro directo `c.Id = @campamento`. |
| **`FORMAT()` en el SELECT** | Más caro que `CONVERT`; mismo resultado visible con `103` + `108`. |
| **`@empresaMandante` no filtra** en el legado | El script optimizado **no** lo añade (mismo alcance que hoy). Valorar `AND (@empresaMandante = 0 OR e.Id = @empresaMandante)` si el negocio lo exige. |
| **Cadena de `LEFT JOIN`** antes de filtros de módulo/piso/habitación | Mejor unir `Habitacion → … → Campamento` con `INNER` y predicados de ubicación ahí, luego checklist. |

## Índices sugeridos (si no existen)

Revisar en el diccionario antes de crear duplicados:

- `Aseo`: `(HabitacionId, FechaCreacion)` y/o `(UsuarioCreacionId, FechaCreacion)`; incluir `GrupoServicioId`, `FechaInicioAseo` según uso.
- `SupervisionAseo`: `(AseoId, FechaCreacion DESC)` INCLUDE (`Id`).
- `AseoCheckList`: `(AseoId)` INCLUDE (`ItemCheckListAseoId`, `value`, `Cantidad`).
- `CampamentosPerfilGestionAdminMinera`: `(MineraId, PerfilGestionId)` INCLUDE (`CampamentoId`).

## Despliegue

1. Comparar planes de ejecución (actual vs optimizado) con los mismos parámetros de prueba.
2. Si el resultado coincide fila a fila, `CREATE OR ALTER PROCEDURE dbo.INF_Aseos` desde el script optimizado.
3. No cambia contrato de columnas ni parámetros.

## Sin filas al depurar con datos reales

Si el informe no devuelve nada en SSMS, casi siempre es un predicado que deja el conjunto en cero (no un fallo del `OUTER APPLY` ni del checklist). Ejecutá [`sql/INF_Aseos_diagnostico.sql`](sql/INF_Aseos_diagnostico.sql) con tus valores.

Checklist habitual:

| Causa | Qué hacer |
|-------|-----------|
| **`@usuarioCreacion`, `@piso` o `@habitacion` sin inicializar** | En un `DECLARE` suelto quedan `NULL`; el legado usa `(@x = 0 OR …)` y `NULL = 0` es desconocido → **cero filas**. Poné `0` explícito en los que no filtrás. |
| **`@modulo` (id de `Pabellones`) + `@tipoModuloId` a la vez** | Si el pabellón 3407 no tiene `TipoModuloId = 8`, el informe es vacío aunque haya aseos en el campamento. El script de diagnóstico lo valida en el paso 2. |
| **Rango de fechas** | El filtro es `FechaCreacion` **o** `FechaInicioAseo` en `[@fechaDesde, @fechaHasta]` (hasta exclusiva = día siguiente a `@fechaHasta`). Si en ese mes no hay aseos en ese módulo, el resultado es correctamente vacío; el paso 4 del diagnóstico muestra los aseos más recientes del campamento/módulo. |
| **`@campamento > 0`** | El legado reemplaza `#campamentos` solo por ese id; `@minera` no acota en ese caso. |
| **`@empresaMandante`** | En el legado **no** hay `WHERE` por empresa; solo aparece en el `SELECT`. No esperes que filtre por 6955. |

Prueba mínima recomendada (aislar el módulo vs el tipo):

```sql
SET @modulo = 3407;
SET @tipoModuloId = 0;  -- quitar cruce con tipo hasta confirmar filas
SET @usuarioCreacion = 0;
SET @piso = 0;
SET @habitacion = 0;
```

Si con `tipoModuloId = 0` aparecen filas y con `8` no, el dato inconsistente está en `Pabellones.TipoModuloId`, no en la query.