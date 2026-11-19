import { numero } from "../../core/numeros.js";
import { crearCenso } from "../censo.js";
import { crearCensoQr } from "../censo-qr.js";
import { definirPantallasCenso } from "../definicion-pantallas.js";

export function instalarCenso(base) {
    var censo = crearCenso({
        obtenerCatalogo: base.catalogos.obtenerCenso,
        campo: base.campo,
        aOpcion: base.aOpcion,
        fechaActual: base.fechaActual,
        pedir: base.pedir,
        consulta: base.consulta,
        numero: numero,
        mostrarEstado: base.mostrarEstado,
        enviar: base.enviar,
        cargar: base.cargar,
        camposPeriodoFiltro: base.camposPeriodoFiltro,
        filtroInicialPeriodo: base.filtroInicialPeriodo,
        normalizarFiltroPeriodo: base.normalizarFiltroPeriodo,
        fechasEntre: base.fechasEntre,
        actualizarSelector: base.actualizarSelector,
        refrescarPeriodoFiltro: base.refrescarPeriodoFiltro
    });
    base.setEnlazarFormulario(function (form) { censo.enlazar(form); });
    var censoQr = crearCensoQr({
        formulario: base.formulario,
        seccion: base.seccion,
        campo: base.campo,
        fechaActual: base.fechaActual,
        enviar: base.enviar,
        mostrarAviso: base.mostrarAviso,
        abrirPanel: base.abrirPanel,
        boton: base.boton
    });
    return {
        censo: censo,
        censoQr: censoQr,
        definiciones: definirPantallasCenso(censo)
    };
}
