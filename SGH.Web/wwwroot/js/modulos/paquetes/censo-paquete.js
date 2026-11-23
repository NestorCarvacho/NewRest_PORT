import { numero } from "../../core/numeros.js";
import { crearCenso } from "../censo.js";
import { crearCensoQr } from "../censo-qr.js";
import { crearCensoQrFlujo } from "../censo-qr-flujo.js";
import { montarVistaMisCensosQr } from "../mis-censos-qr-vista.js";
import { definirPantallasCenso } from "../definicion-pantallas.js";

function etiquetaCampoSimple(etiqueta, control) {
    var label = document.createElement("label");
    label.className = "campo";
    var texto = document.createElement("span");
    texto.textContent = etiqueta;
    label.append(texto, control);
    return label;
}

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
    var censoQrFlujo = crearCensoQrFlujo({
        fechaActual: base.fechaActual,
        enviar: base.enviar,
        mostrarAviso: base.mostrarAviso,
        boton: base.boton,
        etiquetaCampo: etiquetaCampoSimple,
        obtenerCatalogo: function () { return base.catalogos.preparar("censo"); }
    });
    function enlazarVistaMisCensosQr(nodo) {
        montarVistaMisCensosQr({
            enviar: base.enviar,
            fechaActual: base.fechaActual,
            abrirFlujoCensoQr: function (opts) { censoQrFlujo.abrir(opts); }
        }, nodo);
        var fecha = nodo.querySelector("[name='fechaCenso']");
        if (fecha && !fecha.value) fecha.value = base.fechaActual();
    }
    return {
        censo: censo,
        censoQr: censoQr,
        censoQrFlujo: censoQrFlujo,
        enlazarVistaMisCensosQr: enlazarVistaMisCensosQr,
        definiciones: definirPantallasCenso(censo)
    };
}
