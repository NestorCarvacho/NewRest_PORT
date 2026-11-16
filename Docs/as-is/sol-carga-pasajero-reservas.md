# HOTELDB — Carga masiva de pasajeros y solapamiento

`SOL_GuardaPasajeroSolicitudAlojamientoMasivo` (as-is, nov 2026) antes de insertar en `PasajerosSolicitudAlojamiento` busca en `Reservas`:

- `Estado = 'A'`
- Misma **minera** (`Campamentos` por `MineraId`)
- Mismo RUT (`PasajeroId` vía `Usuarios.Rut`)
- Solapamiento con `FechaCheckIn` / `FechaCheckOut` de la solicitud usando **`FechaLlegada` y `FechaSalida` de la reserva**

**No usa** `Reservas.CheckIn` ni `Reservas.CheckOut`.

Por eso, tras un check-out operativo con `CheckOut` informado pero `FechaSalida` NULL, siguen aplicando ramas como:

- `(r.FechaSalida IS NULL AND r.FechaLlegada <= @fechaCheckIn)`
- `(@fechaCheckOut IS NULL AND r.FechaSalida IS NULL)`

Mensaje: `Pasajero ya cuenta con reservas para el periodo. Reserva #…`.

## Ajuste NewRest (sin reescribir el legado)

1. Al registrar **check-out** (`sp_Reserva_RegistrarMovimiento`), si `FechaSalida` sigue NULL, copiar `CheckOut` (o la fecha del movimiento) a `FechaSalida`.
2. Antes de **`sp_Solicitud_CargarPasajero`** y **`sp_Solicitud_GenerarReservas`**, mismo cierre para reservas `Estado = 'A'` con `CheckOut` y sin `FechaSalida` (datos históricos).

Si el negocio exige otra regla (p. ej. anular reserva al checkout), hay que alinear `RES_RealizaCheckX` / estados en HOTELDB.