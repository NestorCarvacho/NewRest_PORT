# Reserva: anulación (referencia legacy)

Pendiente de paridad en `MidReservas`: el módulo web actual expone check-in/check-out (`he_sp_reserva_registrar_movimiento`), no una opción explícita **anular** como en la pantalla legacy.

## Próximo paso técnico

1. Confirmar en HOTELDB el procedimiento o flujo legacy (estado `N`, restricciones de habitación y pasajero).
2. Agregar opción Mid `anular` con el mismo mensaje de negocio que el legado.
3. UI en listado de reservas con confirmación y estados permitidos.

Hasta entonces, la anulación sigue fuera del recorrido MVC+Mid de NewRest_PORT.