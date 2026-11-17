import { bloquearBotonSegundos } from "./boton-espera.js";

export function crearPanelModulo(dependencias) {
    function crearMenuExportar(nodo, tituloModulo) {
        var envoltorio = document.createElement("div");
        envoltorio.className = "exportar-desplegable";
        var dispara = dependencias.boton("Exportar", "boton-secundario exportar-desplegable-dispara", function (evento) {
            evento.stopPropagation();
            var abierto = envoltorio.classList.toggle("abierto");
            dispara.setAttribute("aria-expanded", abierto ? "true" : "false");
        });
        dispara.setAttribute("aria-haspopup", "menu");
        dispara.setAttribute("aria-expanded", "false");
        var panel = document.createElement("div");
        panel.className = "exportar-desplegable-panel";
        panel.setAttribute("role", "menu");
        function alElegir(tipo) {
            return function () {
                envoltorio.classList.remove("abierto");
                dispara.setAttribute("aria-expanded", "false");
                if (!nodo._tabla) {
                    dependencias.mostrarAviso("No hay datos para exportar.", "warning");
                    return;
                }
                var avisoVacio = function () { dependencias.mostrarAviso("No hay datos para exportar.", "warning"); };
                if (tipo === "excel") dependencias.exportarTabla.exportarExcel(nodo, tituloModulo, avisoVacio);
                else dependencias.exportarTabla.exportarCsv(nodo, tituloModulo, avisoVacio);
            };
        }
        [["Exportar Excel", "excel"], ["Exportar CSV", "csv"]].forEach(function (par) {
            var item = dependencias.boton(par[0], "exportar-desplegable-opcion", alElegir(par[1]));
            item.setAttribute("role", "menuitem");
            item.type = "button";
            panel.append(item);
        });
        envoltorio.append(dispara, panel);
        document.addEventListener("click", function (evento) {
            if (!envoltorio.classList.contains("abierto")) return;
            if (!envoltorio.contains(evento.target)) {
                envoltorio.classList.remove("abierto");
                dispara.setAttribute("aria-expanded", "false");
            }
        });
        return envoltorio;
    }

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
        var ultimaActualizacion = 0;
        var intervaloMinimoMs = 5000;
        var botonActualizar = dependencias.boton("Actualizar", "boton-secundario", function () {
            if (botonActualizar.disabled) return;
            var ahora = Date.now();
            var transcurrido = ahora - ultimaActualizacion;
            if (transcurrido < intervaloMinimoMs) {
                var resta = Math.ceil((intervaloMinimoMs - transcurrido) / 1000);
                bloquearBotonSegundos(botonActualizar, resta);
                return;
            }
            ultimaActualizacion = ahora;
            bloquearBotonSegundos(botonActualizar, Math.ceil(intervaloMinimoMs / 1000));
            dependencias.cargar(id).catch(function (error) {
                dependencias.mostrarEstado(nodo, error.message);
            });
        });
        botonActualizar.setAttribute("data-etiqueta", "Actualizar");
        barra.append(botonActualizar);
        barra.append(crearMenuExportar(nodo, modulo.titulo));
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
                        nodo._filtro = dependencias.normalizarFiltroPeriodo
                            ? dependencias.normalizarFiltroPeriodo(datos)
                            : datos;
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
