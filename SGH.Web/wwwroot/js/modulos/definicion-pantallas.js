import { encabezadosModulo } from "../core/encabezados-tabla.js";

function pantalla(titulo, columnas, filtros, secciones, accion, modo) {
    return { titulo: titulo, columnas: columnas, filtros: filtros, secciones: secciones, accion: accion, modo: modo };
}

export function definirPantallasReserva(reserva) {
    var modulos = {
        reserva: pantalla(
            "Reserva",
            encabezadosModulo("reserva"),
            reserva.filtros,
            null,
            null,
            "reserva"
        )
    };
    modulos.reserva.filtroInicial = function () { return reserva.filtroInicial("reserva"); };
    return modulos;
}

export function definirPantallasCenso(censo) {
    var modulos = {
        censo: pantalla(
            "Censo",
            encabezadosModulo("censo"),
            censo.filtros,
            null,
            { titulo: "Censo", boton: "Guardar", confirmar: "Guardar" },
            "censo"
        ),
        misCensos: pantalla(
            "Mis Censos",
            encabezadosModulo("misCensos"),
            censo.filtrosMisCensos,
            null,
            null,
            "misCensos"
        )
    };
    modulos.censo.filtroInicial = function () { return censo.filtroInicial("censo"); };
    modulos.misCensos.filtroInicial = function () { return censo.filtroInicial("misCensos"); };
    return modulos;
}

export function definirPantallasSolicitud(solicitud) {
    var modulos = {
        solicitud: pantalla(
            "Solicitud alojamiento",
            encabezadosModulo("solicitud"),
            solicitud.filtros,
            null,
            { titulo: "Nueva solicitud", boton: "Nueva solicitud" },
            "solicitud"
        )
    };
    modulos.solicitud.filtroInicial = solicitud.filtroInicial;
    modulos.solicitud.ordenInicial = [[2, "desc"], [1, "desc"]];
    return modulos;
}

/** Une definiciones de varios paquetes (escritorio completo). */
export function crearDefinicionPantallas(reserva, censo, solicitud) {
    return Object.assign(
        {},
        definirPantallasReserva(reserva),
        definirPantallasCenso(censo),
        definirPantallasSolicitud(solicitud)
    );
}
