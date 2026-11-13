export function crearNavegacion(dependencias) {
    var orden = [];
    var arrastrada = null;
    var solapas = dependencias.solapas;
    var paneles = dependencias.paneles;
    var vista = dependencias.vista;

    function abrir(id) {
        var modulo = dependencias.modulos[id];
        if (!modulo) return;
        if (orden.indexOf(id) < 0) {
            orden.push(id);
            solapas.append(crearSolapa(id, modulo.titulo));
            paneles.append(dependencias.crearPanel(id, modulo));
        }
        activar(id);
    }

    function crearSolapa(id, titulo) {
        var solapa = document.createElement("button");
        solapa.type = "button";
        solapa.className = "solapa";
        solapa.draggable = true;
        solapa.dataset.id = id;
        var texto = document.createElement("span");
        texto.textContent = titulo;
        var cerrar = document.createElement("span");
        cerrar.className = "solapa-cerrar";
        cerrar.setAttribute("role", "button");
        cerrar.setAttribute("aria-label", "Cerrar " + titulo);
        cerrar.textContent = "×";
        cerrar.addEventListener("click", function (evento) {
            evento.stopPropagation();
            cerrarSolapa(id);
        });
        solapa.append(texto, cerrar);
        solapa.addEventListener("click", function () { activar(id); });
        return solapa;
    }

    function activar(id) {
        solapas.hidden = false;
        vista.hidden = true;
        solapas.querySelectorAll(".solapa").forEach(function (solapa) {
            solapa.classList.toggle("activa", solapa.dataset.id === id);
        });
        paneles.querySelectorAll(".modulo").forEach(function (nodo) {
            var visible = nodo.dataset.id === id;
            nodo.hidden = !visible;
            if (!visible) return;
            if (!window.DataTable) {
                if (dependencias.mostrarEstado) dependencias.mostrarEstado(nodo, "No se cargó la tabla (DataTables). Compruebe la conexión e recargue la página.");
                return;
            }
            if (!nodo.dataset.lista && !nodo.dataset.iniciando) {
                nodo.dataset.iniciando = "1";
                dependencias.mostrarEstado(nodo, "Cargando datos...");
                Promise.resolve().then(function () {
                    if (!nodo.isConnected) return;
                    if (!nodo._tabla)
                        nodo._tabla = dependencias.inicializarTabla(nodo, dependencias.columnas(dependencias.modulos[id].modo), dependencias.modulos[id].ordenInicial);
                    return dependencias.prepararCatalogo(dependencias.modulos[id].modo);
                }).then(function () {
                    if (!nodo.isConnected) return;
                    nodo.dataset.lista = "1";
                    return dependencias.cargar(id);
                }).catch(function (error) {
                    if (nodo.isConnected) dependencias.mostrarEstado(nodo, error.message);
                }).finally(function () {
                    delete nodo.dataset.iniciando;
                });
            }
        });
    }

    function cerrarSolapa(id) {
        var indice = orden.indexOf(id);
        if (indice < 0) return;
        var estabaActiva = solapas.querySelector('[data-id="' + id + '"]').classList.contains("activa");
        orden.splice(indice, 1);
        solapas.querySelector('[data-id="' + id + '"]').remove();
        var nodo = paneles.querySelector('[data-id="' + id + '"]');
        dependencias.mostrarEstado(nodo, "");
        dependencias.destruirTabla(nodo);
        nodo.remove();
        if (orden.length === 0) {
            solapas.hidden = true;
            vista.hidden = false;
            return;
        }
        if (estabaActiva) activar(orden[Math.max(0, indice - 1)]);
    }

    solapas.addEventListener("dragstart", function (evento) {
        var solapa = evento.target.closest(".solapa");
        if (!solapa) return;
        arrastrada = solapa;
        solapa.classList.add("arrastrando");
        evento.dataTransfer.effectAllowed = "move";
    });
    solapas.addEventListener("dragend", function () {
        if (arrastrada) arrastrada.classList.remove("arrastrando");
        arrastrada = null;
    });
    solapas.addEventListener("dragover", function (evento) {
        evento.preventDefault();
        var destino = evento.target.closest(".solapa");
        if (!arrastrada || !destino || destino === arrastrada) return;
        var rect = destino.getBoundingClientRect();
        var despues = evento.clientX > rect.left + rect.width / 2;
        solapas.insertBefore(arrastrada, despues ? destino.nextSibling : destino);
        orden = Array.from(solapas.querySelectorAll(".solapa"), function (solapa) { return solapa.dataset.id; });
    });

    return { abrir: abrir, activar: activar, cerrar: cerrarSolapa, paneles: paneles };
}
