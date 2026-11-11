export function crearCatalogos(enviar) {
    var reserva = null;
    var reservaPendiente = null;
    var censo = null;
    var solicitud = null;

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

    function preparar(modo) {
        if (modo === "solicitud") {
            return enviar("/Solicitud/Mid/MidSolicitudes.cs", { opcion: "catalogo" }).then(function (datos) {
                solicitud = datos;
                return datos;
            });
        }
        if (modo === "misCensos") return Promise.resolve(null);
        if (modo === "censo") return cargarCenso();
        return cargarReserva();
    }

    return {
        preparar: preparar,
        obtenerReserva: function () { return reserva; },
        obtenerSolicitud: function () { return solicitud; },
        obtenerCenso: function () { return censo; }
    };
}
