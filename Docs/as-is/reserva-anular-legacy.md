# Reserva: anulación (referencia legacy)

NewRest_PORT expone **Anular** en el listado de reservas (estados `P`/`A`) vía `MidReservas` opción `anular` y `dbo.he_sp_reserva_anular` (estado `N`).

Validar en HOTELDB si el legado aplicaba reglas adicionales (p. ej. con check-in registrado) y ajustar `ReglasReserva.ValidarAnular` si corresponde.