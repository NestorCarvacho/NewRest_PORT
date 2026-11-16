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
        barra.append(dependencias.boton("Actualizar", "boton-secundario", function () {
            dependencias.cargar(id).catch(function (error) {
                dependencias.mostrarEstado(nodo, error.message);
            });
        }));
        barra.append(dependencias.boton("Exportar Excel", "boton-secundario", function () {
            if (!nodo._tabla) {
                dependencias.mostrarAviso("No hay datos para exportar.", "warning");
                return;
            }
            var avisoVacio = function () { dependencias.mostrarAviso("No hay datos para exportar.", "warning"); };
            dependencias.exportarTabla.exportarExcel(nodo, modulo.titulo, avisoVacio);
        }));
        barra.append(dependencias.boton("Exportar CSV", "boton-secundario", function () {
            if (!nodo._tabla) {
                dependencias.mostrarAviso("No hay datos para exportar.", "warning");
                return;
            }
            var avisoVacio = function () { dependencias.mostrarAviso("No hay datos para exportar.", "warning"); };
            dependencias.exportarTabla.exportarCsv(nodo, modulo.titulo, avisoVacio);
        }));
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
        if (modulo.modo === "habitaciones" && dependencias.habitacion) {
            barra.append(dependencias.boton("Nueva habitación", "boton-primario", function () {
                dependencias.habitacion.abrirNuevaHabitacion();
            }));
        }
        if (modulo.modo === "banos" && dependencias.habitacion) {
            barra.append(dependencias.boton("Nuevo baño", "boton-primario", function () {
                dependencias.habitacion.abrirNuevoBano();
            }));
        }
        nodo.append(estado, barra, dependencias.crearTabla(modulo.columnas));
        return nodo;
    }

    return crear;
}
