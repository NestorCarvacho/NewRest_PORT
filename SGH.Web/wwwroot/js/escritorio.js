import { inicializarCuenta } from "./modulos/cuenta.js";
import { crearNavegacion } from "./core/navegacion.js";
import { inicializarMenuModulos } from "./core/menu-modulos.js";
import { crearEntornoEscritorio } from "./core/fabrica-escritorio.js";

(function () {
    var solapas = document.getElementById("solapas");
    var paneles = document.getElementById("paneles");
    if (!solapas || !paneles) return;

    inicializarCuenta();
    var entorno = crearEntornoEscritorio();
    var vista = document.getElementById("vista-inicial");

    var navegacion = crearNavegacion({
        solapas: solapas,
        paneles: paneles,
        vista: vista,
        modulos: entorno.modulos,
        crearPanel: entorno.crearPanel,
        columnas: entorno.renderizador.columnas,
        prepararCatalogo: entorno.prepararCatalogo,
        inicializarTabla: entorno.tablas.inicializar,
        destruirTabla: entorno.tablas.destruir,
        cargar: entorno.cargar,
        mostrarEstado: entorno.mostrarEstado
    });

    entorno.enlazarCargador(function (id) {
        return navegacion.paneles.querySelector('[data-id="' + id + '"]');
    });

    var menuModulos = inicializarMenuModulos({
        abrirModulo: navegacion.abrir,
        abrirCensoQr: entorno.censoQr ? entorno.censoQr.abrir : function () { }
    });

    entorno.enlazarPanelesLaterales();
    if (entorno.censoQr) {
        window.addEventListener("online", entorno.censoQr.sincronizarPendientes);
        entorno.censoQr.sincronizarPendientes();
    }

    var inicial = document.getElementById("modulo-inicial");
    if (inicial) navegacion.abrir(inicial.dataset.moduloInicial);

    document.addEventListener("keydown", function (evento) {
        if (evento.key === "Escape") {
            entorno.cerrarPanel();
            menuModulos.cerrarMenus();
        }
    });
})();
