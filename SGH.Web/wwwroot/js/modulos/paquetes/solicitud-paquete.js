import { crearSolicitud } from "../solicitud.js";
import { definirPantallasSolicitud } from "../definicion-pantallas.js";

export function instalarSolicitud(base) {
    var solicitud = crearSolicitud({
        pedir: base.pedir,
        enviar: base.enviar,
        enviarArchivo: base.api.enviarArchivo,
        consulta: base.consulta,
        campo: base.campo,
        inicializarSelectores: base.inicializarSelectores,
        actualizarSelector: base.actualizarSelector,
        obtenerCatalogo: base.catalogos.obtenerSolicitud,
        prepararCatalogo: base.prepararCatalogo,
        abrirPanel: base.abrirPanel,
        cerrarPanel: base.cerrarPanel,
        cargar: base.cargar,
        mostrarAviso: base.mostrarAviso,
        fechaActual: base.fechaActual,
        rangoInicial: base.rangoReservaInicial,
        presetsPeriodo: base.presetsPeriodo,
        aplicarPresetPeriodo: base.aplicarPresetPeriodo,
        normalizarFiltroPeriodo: base.normalizarFiltroPeriodo
    });
    return {
        solicitud: solicitud,
        definiciones: definirPantallasSolicitud(solicitud)
    };
}
