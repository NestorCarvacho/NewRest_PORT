import { clonarModuloShell } from "./biblioteca-ui.js";

export function crearPanelModulo(dependencias) {
    function crear(id, modulo) {
        var nodo = clonarModuloShell(id);
        if (!nodo) {
            nodo = document.createElement("section");
            nodo.className = "modulo modulo-" + id;
            nodo.dataset.id = id;
            nodo.hidden = true;
            var estadoLegacy = document.createElement("p");
            estadoLegacy.className = "modulo-estado";
            estadoLegacy.hidden = true;
            var barraLegacy = document.createElement("div");
            barraLegacy.className = "modulo-barra";
            var tablaLegacy = document.createElement("div");
            nodo.append(estadoLegacy, barraLegacy, tablaLegacy);
        }

        nodo.hidden = true;
        nodo._filtro = modulo.filtroInicial();
        var estado = nodo.querySelector(".modulo-estado, [data-sgh-modulo-estado]");
        var barra = nodo.querySelector(".modulo-barra, [data-sgh-barra]");
        var contenedorTabla = nodo.querySelector("[data-sgh-modulo-tabla]");
        if (estado) estado.hidden = true;

        barra.append(dependencias.boton("Filtros", "boton-secundario", function () {
            dependencias.prepararCatalogo(modulo.modo).then(function () {
                dependencias.abrirPanel("Filtros de " + modulo.titulo, dependencias.formulario(
                    dependencias.contenido(modulo.filtros).map(function (grupo) {
                        return { titulo: grupo.titulo, campos: grupo.campos.map(function (campo) {
                            return Object.assign({}, campo, { valor: nodo._filtro[campo.nombre] });
                        }) };
                    }),
                    "Aplicar",
                    false,
                    function (datos) {
                        nodo._filtro = datos;
                        return dependencias.cargar(id);
                    }
                ));
            }).catch(function (error) { dependencias.mostrarEstado(nodo, error.message); });
        }));
        if (modulo.modo === "solicitud") {
            barra.append(dependencias.boton("Nueva solicitud", "boton-primario", function () {
                dependencias.solicitud.abrirNueva();
            }));
        }
        if (modulo.modo === "censo") {
            barra.append(dependencias.boton("Censo QR", "boton-secundario", dependencias.censoQr.abrir));
            var guardar = dependencias.boton("Guardar", "boton-primario", function () {
                dependencias.censo.guardar(nodo);
            });
            guardar.hidden = true;
            nodo._guardar = guardar;
            barra.append(guardar);
        }

        var tabla = dependencias.crearTabla(modulo.columnas);
        if (contenedorTabla) contenedorTabla.replaceWith(tabla);
        else nodo.append(tabla);

        return nodo;
    }

    return crear;
}