import { inicializarCuenta } from "../modulos/cuenta.js";
import { crearEntornoEscritorio } from "./fabrica-escritorio.js";

/**
 * Monta un módulo en #modulo-pagina: catálogo (filtros), tabla y listado inicial (pageLoad).
 */
export function iniciarPaginaModulo(idModulo) {
    var ancla = document.getElementById("modulo-pagina");
    if (!ancla) return Promise.reject(new Error("No se encontró el contenedor del módulo."));

    var id = idModulo || ancla.dataset.modulo;
    if (!id) return Promise.reject(new Error("Falta el identificador del módulo."));

    inicializarCuenta();
    var entorno = crearEntornoEscritorio({ pantallas: [id] });
    var modulo = entorno.modulos[id];
    if (!modulo) return Promise.reject(new Error("Módulo no configurado: " + id));

    entorno.enlazarCargador(function (clave) {
        return document.querySelector('.modulo[data-id="' + clave + '"]');
    });
    entorno.enlazarPanelesLaterales();

    var nodo = entorno.crearPanel(id, modulo);
    nodo.hidden = false;
    nodo.classList.add("modulo-en-pagina");
    ancla.replaceChildren(nodo);

    var vista = document.getElementById("vista-inicial");
    if (vista) vista.hidden = true;

    return entorno.prepararCatalogo(modulo.modo).then(function () {
        if (!window.DataTable) {
            throw new Error("No se cargó DataTables. Compruebe la conexión e recargue la página.");
        }
        nodo._tabla = entorno.tablas.inicializar(nodo, entorno.renderizador.columnas(modulo.modo));
        nodo.dataset.lista = "1";
        return entorno.cargar(id);
    }).catch(function (error) {
        entorno.mostrarEstado(nodo, error.message);
        throw error;
    });
}
