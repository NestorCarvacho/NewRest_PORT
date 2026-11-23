# Reversar check-in / check-out

La acción **Reversar** en el listado de reservas (`movimientos-reserva.js`) no anula la reserva: quita el último movimiento operativo.

1. Si hay **check-out**, lo elimina y la fila vuelve a quedar con check-in.
2. Si solo hay **check-in**, lo elimina y la reserva queda asignada sin ingreso.

Mid: opción `reversar` → `he_sp_reserva_reversar_movimiento` (solo reservas en estado **A**).

**Anular** sigue siendo anulación de la reserva (estado **N**), independiente de reversar movimientos.