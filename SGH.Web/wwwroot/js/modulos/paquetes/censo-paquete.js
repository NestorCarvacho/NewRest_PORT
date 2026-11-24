import { sincronizarTodo } from "../../core/colaOffline.js";
import { numero } from "../../core/numeros.js";
import { crearCenso } from "../censo.js";
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
    var censoQrFlujo = crearCensoQrFlujo({
        fechaActual: base.fechaActual,
        enviar: base.enviar,
        mostrarAviso: base.mostrarAviso,
        boton: base.boton,
        etiquetaCampo: etiquetaCampoSimple,
        abrirPanel: base.abrirPanel,
        cerrarPanel: base.cerrarPanel,
        obtenerCatalogo: function () { return base.catalogos.preparar("censo"); }
    });

    function fechaCensoDesdeFiltro(filtro) {
        return censo.fechaUnicaCenso(filtro) || base.fechaActual();
    }

    function refrescarTrasQr(fechaGuardada, nodoOrigen) {
        if (nodoOrigen && nodoOrigen.dataset.id === "censo" && base.cargar) {
            base.cargar("censo", { bloquearPantalla: false, avisarConteo: false }).catch(function () { });
        }
        var mis = document.querySelector('.modulo-anclado[data-id="misCensos"]');
        if (mis && mis._refrescarVistaCensoQr) mis._refrescarVistaCensoQr(fechaGuardada);
    }

    function abrirFlujoCensoQr(opciones) {
        opciones = opciones || {};
        var alCerrarUsuario = opciones.alCerrar;
        var nodoOrigen = opciones.nodoOrigen;
        censoQrFlujo.abrir({
            fechaCenso: opciones.fechaCenso || base.fechaActual(),
            alCerrar: function (fechaGuardada) {
                refrescarTrasQr(fechaGuardada, nodoOrigen);
                if (typeof alCerrarUsuario === "function") alCerrarUsuario(fechaGuardada);
            }
        });
    }

    function abrirFlujoCensoQrDesdeNodo(nodo) {
        var filtro = (nodo && nodo._filtro) || {};
        abrirFlujoCensoQr({
            fechaCenso: fechaCensoDesdeFiltro(filtro),
            nodoOrigen: nodo
        });
    }

    function enlazarVistaMisCensosQr(nodo, extra) {
        extra = extra || {};
        montarVistaMisCensosQr({
            enviar: base.enviar,
            fechaActual: base.fechaActual,
            sincronizar: function () { return sincronizarTodo(base.enviar); },
            recargarModulo: extra.recargarModulo,
            abrirFlujoCensoQr: function (opts) {
                abrirFlujoCensoQr(Object.assign({ nodoOrigen: nodo }, opts || {}));
            }
        }, nodo);
        var fecha = nodo.querySelector("[name='fechaCenso']");
        if (fecha && !fecha.value) fecha.value = base.fechaActual();
    }

    return {
        censo: censo,
        censoQrFlujo: censoQrFlujo,
        abrirFlujoCensoQr: abrirFlujoCensoQr,
        abrirFlujoCensoQrDesdeNodo: abrirFlujoCensoQrDesdeNodo,
        enlazarVistaMisCensosQr: enlazarVistaMisCensosQr,
        definiciones: definirPantallasCenso(censo)
    };
}