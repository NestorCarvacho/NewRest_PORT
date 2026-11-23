import { actualizarBannerOffline } from "./colaOffline.js";

var CLAVE_COLA = "sgh.offline.cola";
var EVENTO = "sgh-censo-qr-pendientes";

function leerCola() {
    try {
        return JSON.parse(localStorage.getItem(CLAVE_COLA) || "[]");
    } catch (_) {
        return [];
    }
}

function esPendienteCenso(item) {
    return item && item.cuerpo && item.cuerpo.opcion === "guardarqr";
}

export function listarPendientesCensoQr() {
    return leerCola().filter(esPendienteCenso);
}

export function encolarCensoQr(cuerpo, meta) {
    var pendientes = leerCola();
    var entrada = {
        id: Date.now() + "-" + Math.random().toString(16).slice(2),
        url: "/Censo/Mid/MidCenso.cs",
        cuerpo: Object.assign({}, cuerpo),
        meta: Object.assign({
            creadoEn: new Date().toISOString()
        }, meta || {})
    };
    pendientes.push(entrada);
    localStorage.setItem(CLAVE_COLA, JSON.stringify(pendientes));
    actualizarBannerOffline();
    window.dispatchEvent(new CustomEvent(EVENTO));
    return entrada.id;
}

export function nombreEventoPendientesCensoQr() {
    return EVENTO;
}