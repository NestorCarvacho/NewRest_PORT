# Finalizar reserva (liberar habitación)

Comportamiento documentado en `Docs/as-is/sol-carga-pasajero-reservas.md`: tras check-out, el SP `he_sp_reserva_finalizar_checkout` deja la reserva en estado **F** y libera la habitación para nuevas asignaciones.

En NewRest_PORT, parte de esa lógica ya corre dentro de `he_sp_solicitud_generar_reservas` al cerrar reservas previas en conflicto. Un botón explícito **Finalizar** en la UI de reservas queda pendiente de sign-off HostEast (ver plan en `Docs/actividades/HostEast-plan-requisitos.md`, ítem 11).