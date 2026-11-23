# Anular check-in / check-out (= reversar movimiento)

En el listado, el botón **Anular** (con check-in o check-out) quita el último movimiento operativo; en Mid es la opción `reversar`.

1. Si hay **check-out**, lo elimina y la fila vuelve a quedar con check-in.
2. Si solo hay **check-in**, lo elimina y la reserva queda asignada sin ingreso.

Mid: `reversar` → `he_sp_reserva_reversar_movimiento` (solo reservas en estado **A**).

**Anular reserva** (sin movimientos) sigue en opción `anular` y deja la reserva en estado **N**.