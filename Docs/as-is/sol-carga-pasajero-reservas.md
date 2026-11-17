# HOTELDB — Carga masiva de pasajeros y solapamiento

`SOL_GuardaPasajeroSolicitudAlojamientoMasivo` (as-is, nov 2026) antes de insertar en `PasajerosSolicitudAlojamiento` busca en `Reservas`:

- `Estado = 'A'`
- Misma **minera**: `Reservas.CampamentoId` → `Campamentos.EmpresaMandanteId` → `Empresas.MineraId` → `Mineras` (no existe `Campamentos.MineraId`)
- Mismo RUT (`PasajeroId` vía `Usuarios.Rut`)
- Solapamiento con `FechaCheckIn` / `FechaCheckOut` de la solicitud usando **`FechaLlegada` y `FechaSalida` de la reserva**

**No usa** `Reservas.CheckIn` ni `Reservas.CheckOut`.

Por eso, tras un check-out operativo con `CheckOut` informado pero `FechaSalida` NULL, siguen aplicando ramas como:

- `(r.FechaSalida IS NULL AND r.FechaLlegada <= @fechaCheckIn)`
- `(@fechaCheckOut IS NULL AND r.FechaSalida IS NULL)`

Mensaje: `Pasajero ya cuenta con reservas para el periodo. Reserva #…`.

## Ajuste NewRest (sin reescribir el legado)

1. **`sp_Reserva_FinalizarCheckout`**: si hay `CheckOut`, completar `FechaSalida` y pasar la reserva a **`Estado = 'F'`** (el masivo solo bloquea con `Estado = 'A'`).
2. Se invoca al registrar check-out (`sp_Reserva_RegistrarMovimiento`), antes de importar pasajeros (`sp_Solicitud_CargarPasajero`, por RUT y toda la minera) y al generar reservas.
3. **`sp_Solicitud_AsegurarPeriodo`**: si `FechaCheckOut` de la solicitud viene NULL, derivarla del turno o igualar `FechaCheckIn` (evita la rama `(@fechaCheckOut IS NULL AND r.FechaSalida IS NULL)` del masivo).

En pantalla una reserva puede figurar con check-out y aún **Asignada** (`A`); sin finalizarla, el import masivo sigue viendo solapamiento aunque `salida` esté informada.