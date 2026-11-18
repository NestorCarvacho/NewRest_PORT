export function crearCatalogos(enviar) {
    var reserva = null;
    var reservaPendiente = null;
    var censo = null;
    var solicitud = null;
    var habitacion = null;

    function cargarReserva() {
        if (reserva) return Promise.resolve(reserva);
        if (reservaPendiente) return reservaPendiente;
        reservaPendiente = enviar("/Reserva/Mid/MidReservas.cs", { opcion: "catalogo" }).then(function (datos) {
            reserva = datos;
            return datos;
        }).catch(function (error) {
            reservaPendiente = null;
            throw error;
        });
        return reservaPendiente;
    }

    function cargarCenso() {
        if (censo) return Promise.resolve(censo);
        return enviar("/Censo/Mid/MidCenso.cs", { opcion: "catalogo" }).then(function (datos) {
            censo = datos;
            return datos;
        });
    }

    function cargarHabitacion() {
        if (habitacion) return Promise.resolve(habitacion);
        return enviar("/Habitacion/Mid/MidHabitaciones.cs", { opcion: "catalogo" }).then(function (datos) {
            habitacion = datos;
            return datos;
        });
    }

    function preparar(modo) {
        if (modo === "solicitud") {
            return enviar("/Solicitud/Mid/MidSolicitudes.cs", { opcion: "catalogo" }).then(function (datos) {
                solicitud = datos;
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
        invalidarHabitacion: function () { habitacion = null; }
    };
}
