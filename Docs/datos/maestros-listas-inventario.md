# Inventario maestros — Listas (SGH 2.1.0)

**Estado:** **BORRADOR / PENDIENTE** — completar contra HOTELDB y código legado.

## Propósito

Matriz de los **41 catálogos `Listas`** para planificar lotes SGH 2.1.1a…n. Una fila por código de lista o pantalla legado equivalente.

## Plantilla de fila

| # | Código Lista / pantalla | SP lectura | SP alta/edición/baja | Mensajes as-is | Menú legado | Lote propuesto | Estado NewRest |
|---|-------------------------|------------|----------------------|----------------|-------------|----------------|----------------|
| 1 | `usrpasshist` | (política contraseña) | — | `Docs/login-…` | — | N/A (parámetro) | Solo lectura reglas |
| 2 | *(completar)* | | | | | | Pendiente |

## Consultas SQL (catálogo de SP)

Usar [`diccionario-hoteldb-procedimientos-almacenados.md`](diccionario-hoteldb-procedimientos-almacenados.md):

- **§1** — todos los procedimientos de HOTELDB.
- **§2** — filtro por prefijos maestros (`UNG_`, `DIV_`, `DEP_`, `EMP_`, `LIS_`, …).
- **§3** — parámetros por SP.
- **§4** — SP que referencian tablas `Listas`, `Empresas`, etc.

Exportar resultados a CSV y completar la matriz de abajo (41 ítems).

## Siguiente paso

Cerrar ticket **SGH 2.1.0** cuando las 41 filas tengan SP y lote asignado; recién entonces abrir **SGH 2.1.1** (primer lote de implementación).