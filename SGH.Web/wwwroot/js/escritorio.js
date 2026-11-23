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
            abrirCensoQr: entorno.censoQrFlujo ? function () {
                var nodo = document.querySelector('.modulo-anclado[data-id="misCensos"]');
                var fechaInp = nodo && nodo.querySelector("[name='fechaCenso']");
                var fechaCenso = (fechaInp && fechaInp.value) || (entorno.fechaActual && entorno.fechaActual());
                entorno.censoQrFlujo.abrir({
                    fechaCenso: fechaCenso,
                    alCerrar: function () {
                        if (nodo && nodo._refrescarVistaCensoQr) nodo._refrescarVistaCensoQr();
                    }
                });
            } : (entorno.censoQr ? entorno.censoQr.abrir : function () { }),
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
        abrirCensoQr: entorno.censoQrFlujo ? function () {
            navegacion.abrir("misCensos");
            var nodo = navegacion.paneles.querySelector('[data-id="misCensos"]');
            var fechaInp = nodo && nodo.querySelector("[name='fechaCenso']");
            var fechaCenso = (fechaInp && fechaInp.value) || (entorno.fechaActual && entorno.fechaActual());
            entorno.censoQrFlujo.abrir({
                fechaCenso: fechaCenso,
                alCerrar: function () {
                    if (nodo && nodo._refrescarVistaCensoQr) nodo._refrescarVistaCensoQr();
                    entorno.cargar("misCensos", { bloquearPantalla: false }).catch(function () { });
                }
            });
        } : (entorno.censoQr ? entorno.censoQr.abrir : function () { }),
        esModuloSolapa: function (id) { return !!entorno.modulos[id]; }
    });

    entorno.enlazarPanelesLaterales();
    if (entorno.censoQr) {
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
