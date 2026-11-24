import { inicializarCuenta } from "./modulos/cuenta.js";
import { crearNavegacion } from "./core/navegacion.js";
import { inicializarMenuModulos } from "./core/menu-modulos.js";
import { crearEntornoEscritorio } from "./core/fabrica-escritorio.js";
import { instalarColaOffline } from "./core/colaOffline.js";
import { crearApi } from "./core/api.js";

(function () {
    var api = crearApi();
    var offline = instalarColaOffline(api.enviar.bind(api));
    inicializarCuenta();
    var entorno = crearEntornoEscritorio();
    var solapas = document.getElementById("solapas");
    var paneles = document.getElementById("paneles");
    var vista = document.getElementById("vista-inicial");
    var navegacion = null;
    var menuModulos;

    if (!solapas || !paneles) {
        menuModulos = inicializarMenuModulos({
            abrirModulo: function (modulo) {
                if (!modulo) return;
                window.location.assign("/Index?modulo=" + encodeURIComponent(modulo));
            },
            abrirCensoQr: entorno.abrirFlujoCensoQr ? function () {
                var mis = document.querySelector('.modulo-anclado[data-id="misCensos"]');
                var censo = document.querySelector('.modulo-anclado[data-id="censo"]');
                var fechaInp = mis && mis.querySelector("[name='fechaCenso']");
                var fechaCenso = (fechaInp && fechaInp.value) || (entorno.fechaActual && entorno.fechaActual());
                entorno.abrirFlujoCensoQr({
                    fechaCenso: fechaCenso,
                    nodoOrigen: censo || mis
                });
            } : function () { },
            esModuloSolapa: function (id) { return !!entorno.modulos[id]; }
        });
        entorno.enlazarPanelesLaterales();
        document.addEventListener("keydown", function (evento) {
            if (evento.key === "Escape") {
                entorno.cerrarPanel();
                menuModulos.cerrarMenus();
            }
        });
        return;
    }

    navegacion = crearNavegacion({
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
        cancelarCargaNodo: entorno.cancelarCargaNodo,
        mostrarEstado: entorno.mostrarEstado
    });

    entorno.enlazarCargador(function (id) {
        return navegacion.paneles.querySelector('[data-id="' + id + '"]');
    });

    menuModulos = inicializarMenuModulos({
        abrirModulo: navegacion.abrir,
        abrirCensoQr: entorno.abrirFlujoCensoQr ? function () {
            navegacion.abrir("misCensos");
            var mis = navegacion.paneles.querySelector('[data-id="misCensos"]');
            var fechaInp = mis && mis.querySelector("[name='fechaCenso']");
            var fechaCenso = (fechaInp && fechaInp.value) || (entorno.fechaActual && entorno.fechaActual());
            entorno.abrirFlujoCensoQr({
                fechaCenso: fechaCenso,
                nodoOrigen: mis
            });
        } : function () { },
        esModuloSolapa: function (id) { return !!entorno.modulos[id]; }
    });

    entorno.enlazarPanelesLaterales();
    if (entorno.censoQrFlujo) {
        window.addEventListener("online", function () { offline.sincronizar().catch(function () { }); });
        offline.sincronizar().catch(function () { });
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
