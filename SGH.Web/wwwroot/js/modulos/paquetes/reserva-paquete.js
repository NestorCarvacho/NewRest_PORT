import { numero } from "../../core/numeros.js";
import { crearReserva } from "../reserva.js";
import { crearMovimientosReserva } from "../movimientos-reserva.js";
import { definirPantallasReserva } from "../definicion-pantallas.js";

export function instalarReserva(base) {
    var reserva = crearReserva({
        obtenerCatalogo: base.catalogos.obtenerReserva,
        campo: base.campo,
        aOpcion: base.aOpcion,
        seccion: base.seccion,
        momentoLocal: base.momentoLocal,
        rangoReservaInicial: base.rangoReservaInicial,
        formulario: base.formulario,
        numero: numero,
        enviar: base.enviar,
        abrirPanel: base.abrirPanel,
        cargar: base.cargar,
        mostrarAviso: base.mostrarAviso,
        consulta: base.consulta
    });
    var movimientosReserva = crearMovimientosReserva({
        boton: base.boton,
        prepararCatalogo: base.prepararCatalogo,
        abrirEdicion: reserva.abrirEdicion,
        mostrarAviso: base.mostrarAviso,
        abrirPanel: base.abrirPanel,
        formulario: base.formulario,
        seccion: base.seccion,
        campo: base.campo,
        momentoLocal: base.momentoLocal,
        enviar: base.enviar,
        cargar: base.cargar
    });
    return {
        reserva: reserva,
        movimientosReserva: movimientosReserva,
        definiciones: definirPantallasReserva(reserva)
    };
}
