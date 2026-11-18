export function crearCatalogos(enviar) {
    var ttlMs = 300000;
    var reserva = null;
    var reservaObtenidaEn = 0;
    var reservaPendiente = null;
    var censo = null;
    var censoObtenidaEn = 0;
    var solicitud = null;
    var solicitudObtenidaEn = 0;
    var habitacion = null;
    var habitacionObtenidaEn = 0;

    function vigente(marca) {
        return marca > 0 && (Date.now() - marca) < ttlMs;
    }

    function cargarReserva() {
        if (reserva && vigente(reservaObtenidaEn)) return Promise.resolve(reserva);
        if (reservaPendiente) return reservaPendiente;
        reservaPendiente = enviar("/Reserva/Mid/MidReservas.cs", { opcion: "catalogo" }).then(function (datos) {
            reserva = datos;
            reservaObtenidaEn = Date.now();
            return datos;
        }).catch(function (error) {
            reservaPendiente = null;
            throw error;
        });
        return reservaPendiente;
    }

    function cargarCenso() {
        if (censo && vigente(censoObtenidaEn)) return Promise.resolve(censo);
        return enviar("/Censo/Mid/MidCenso.cs", { opcion: "catalogo" }).then(function (datos) {
            censo = datos;
            censoObtenidaEn = Date.now();
            return datos;
        });
    }

    function cargarHabitacion() {
        if (habitacion && vigente(habitacionObtenidaEn)) return Promise.resolve(habitacion);
        return enviar("/Habitacion/Mid/MidHabitaciones.cs", { opcion: "catalogo" }).then(function (datos) {
            habitacion = datos;
            habitacionObtenidaEn = Date.now();
            return datos;
        });
    }

    function preparar(modo) {
        if (modo === "solicitud") {
            if (solicitud && vigente(solicitudObtenidaEn)) return Promise.resolve(solicitud);
            return enviar("/Solicitud/Mid/MidSolicitudes.cs", { opcion: "catalogo" }).then(function (datos) {
                solicitud = datos;
                solicitudObtenidaEn = Date.now();
                return datos;
            });
        }
        if (modo === "habitaciones" || modo === "banos" || modo === "tiposcama") return cargarHabitacion();
        if (modo === "misCensos") return Promise.resolve(null);
        if (modo === "censo") return cargarCenso();
        return cargarReserva();
    }

    return {
        preparar: preparar,
        obtenerReserva: function () { return reserva; },
        obtenerSolicitud: function () { return solicitud; },
        obtenerCenso: function () { return censo; },
        obtenerHabitacion: function () { return habitacion; },
        invalidarHabitacion: function () { habitacion = null; habitacionObtenidaEn = 0; },
        invalidarReserva: function () { reserva = null; reservaObtenidaEn = 0; reservaPendiente = null; },
        invalidarCenso: function () { censo = null; censoObtenidaEn = 0; },
        invalidarSolicitud: function () { solicitud = null; solicitudObtenidaEn = 0; }
    };
}