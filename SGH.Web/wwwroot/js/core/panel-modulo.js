export function crearPanelModulo(dependencias) {
    function crear(id, modulo) {
        var nodo = document.createElement("section");
        nodo.className = "modulo modulo-" + id;
        nodo.dataset.id = id;
        nodo.hidden = true;
        nodo._filtro = modulo.filtroInicial();
        var estado = document.createElement("p");
        estado.className = "modulo-estado";
        estado.hidden = true;
        var barra = document.createElement("div");
        barra.className = "modulo-barra";
        barra.append(dependencias.boton("Filtros", "boton-secundario", function () {
            dependencias.prepararCatalogo(modulo.modo).then(function () {
                dependencias.abrirPanel("Filtros de " + modulo.titulo, dependencias.formulario(
                    dependencias.contenido(modulo.filtros),
                    "Aplicar",
                    false,
                    function (datos) {
                        nodo._filtro = datos;
                        return dependencias.cargar(id);
                    }
                ));
            }).catch(function (error) { dependencias.mostrarEstado(nodo, error.message); });
        }));
        if (modulo.accion && modulo.modo === "reserva") {
            barra.append(dependencias.boton(modulo.accion.boton, "boton-primario", function () {
                dependencias.prepararCatalogo(modulo.modo).then(function () {
                    dependencias.abrirPanel(modulo.accion.titulo, dependencias.formulario(
                        dependencias.contenido(modulo.secciones),
                        modulo.accion.confirmar,
                        true,
                        function (datos) { return dependencias.reserva.crear(datos, id); }
                    ));
                }).catch(function (error) { dependencias.mostrarEstado(nodo, error.message); });
            }));
            barra.append(dependencias.boton("Importar Excel", "boton-secundario", function () {
                dependencias.reserva.abrirImportacion(id);
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
        nodo.append(estado, barra, dependencias.crearTabla(modulo.columnas));
        return nodo;
    }

    return crear;
}
