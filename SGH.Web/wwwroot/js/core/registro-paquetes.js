import { instalarReserva } from "../modulos/paquetes/reserva-paquete.js";
import { instalarCenso } from "../modulos/paquetes/censo-paquete.js";
import { instalarSolicitud } from "../modulos/paquetes/solicitud-paquete.js";

/**
 * Pantallas del escritorio (solapas en Index).
 * Al agregar módulo: registrar en PANTALLA_A_PAQUETE e INSTALADORES.
 */
export var PANTALLAS_ESCRITORIO = ["reserva", "checkin", "checkout", "censo", "misCensos", "solicitud"];

export var PANTALLA_A_PAQUETE = {
    reserva: "reserva",
    checkin: "reserva",
    checkout: "reserva",
    censo: "censo",
    misCensos: "censo",
    censoQr: "censo",
    solicitud: "solicitud"
};

/** Nombre de paquete → función instalar(base) que devuelve APIs y definiciones. */
export var INSTALADORES = {
    reserva: instalarReserva,
    censo: instalarCenso,
    solicitud: instalarSolicitud
};

export var PROPIEDADES_PAQUETE = ["reserva", "movimientosReserva", "censo", "censoQr", "solicitud"];

export function paquetesParaPantallas(idsPantalla) {
    var paquetes = new Set();
    idsPantalla.forEach(function (id) {
        var paquete = PANTALLA_A_PAQUETE[id];
        if (paquete) paquetes.add(paquete);
    });
    return Array.from(paquetes);
}
