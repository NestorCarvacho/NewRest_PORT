# Inventario maestros — Listas (SGH 2.1.0)

**Estado:** **BORRADOR / PENDIENTE** — completar contra HOTELDB y código legado.

## Propósito

Matriz de los **41 catálogos `Listas`** para planificar lotes SGH 2.1.1a…n. Una fila por código de lista o pantalla legado equivalente.

## Plantilla de fila

| # | Código Lista / pantalla | SP lectura | SP alta/edición/baja | Mensajes as-is | Menú legado | Lote propuesto | Estado NewRest |
|---|-------------------------|------------|----------------------|----------------|-------------|----------------|----------------|
| 1 | `usrpasshist` | (política contraseña) | — | `Docs/login-…` | — | N/A (parámetro) | Solo lectura reglas |
| 2 | *(completar)* | | | | | | Pendiente |

## Consulta sugerida (ajustar al esquema real)

Contrastar en SQL Server las tablas/vistas que alimentan el mantenedor **Listas** del front legado y documentar aquí los 41 ítems. No ejecutar en este repo sin credenciales; el dueño de HOTELDB completa la matriz.

## Siguiente paso

Cerrar ticket **SGH 2.1.0** cuando las 41 filas tengan SP y lote asignado; recién entonces abrir **SGH 2.1.1** (primer lote de implementación).