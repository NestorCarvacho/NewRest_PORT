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