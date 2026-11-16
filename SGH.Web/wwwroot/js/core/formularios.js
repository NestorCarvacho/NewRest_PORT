export function crearFormularios(dependencias) {
    function seccion(titulo, campos) {
        return { titulo: titulo, campos: campos };
    }

    function campo(nombre, etiqueta, tipo, opciones, extra) {
        extra = extra || {};
        return {
            nombre: nombre,
            etiqueta: etiqueta,
            tipo: tipo || "text",
            opciones: opciones || [],
            ancho: tipo === "textarea" || extra.ancho,
            sinVacio: extra.sinVacio,
            soloLectura: extra.soloLectura,
            deshabilitado: extra.deshabilitado,
            requerido: extra.requerido,
            valor: extra.valor,
            controlesCantidad: extra.controlesCantidad
        };
    }

    function boton(texto, clase, alClick) {
        var elemento = document.createElement("button");
        elemento.type = alClick ? "button" : "submit";
        elemento.className = clase;
        elemento.textContent = texto;
        if (alClick) elemento.addEventListener("click", alClick);
        return elemento;
    }

    function opcionesDe(definicion) {
        return (definicion.opciones || []).map(function (opcion) {
            if (typeof opcion === "string") return { value: opcion, text: opcion };
            return { value: String(opcion.value), text: opcion.text, padre: opcion.padre == null ? "" : String(opcion.padre) };
        });
    }

    function aOpcion(lista) {
        return (lista || []).map(function (opcion) {
            return { value: opcion.id, text: opcion.etiqueta, padre: opcion.padreId };
        });
    }

    function leer(form) {
        var datos = {};
        form.querySelectorAll("input, select, textarea").forEach(function (control) {
            if (!control.name || control.disabled) return;
            datos[control.name] = control.value;
        });
        return datos;
    }

    function campoVisual(definicion, vacio) {
        var etiqueta = document.createElement("label");
        etiqueta.className = "campo" + (definicion.ancho ? " campo-ancho" : "");
        var texto = document.createElement("span");
        texto.textContent = definicion.etiqueta;
        if (definicion.soloLectura || definicion.deshabilitado) {
            var estado = document.createElement("span");
            estado.className = "campo-estado";
            estado.textContent = definicion.deshabilitado ? "Bloqueado" : "Solo lectura";
            texto.append(" ", estado);
        }
        var control;
        if (definicion.tipo === "select") {
            control = document.createElement("select");
            control.className = "selector-buscable";
            control._opciones = opcionesDe(definicion);
            if (!definicion.sinVacio) {
                var todos = document.createElement("option");
                todos.value = "";
                todos.textContent = vacio;
                control.append(todos);
            }
            control._opciones.forEach(function (opcion) {
                var item = document.createElement("option");
                item.value = opcion.value;
                item.textContent = opcion.text;
                item.disabled = opcion.deshabilitado === true;
                if (item.disabled) item.setAttribute("aria-disabled", "true");
                control.append(item);
            });
            if (definicion.valor !== undefined && definicion.valor !== null) control.value = definicion.valor;
        } else if (definicion.tipo === "textarea") {
            control = document.createElement("textarea");
        } else {
            control = document.createElement("input");
            control.type = definicion.tipo || "text";
        }
        if (definicion.controlesCantidad) {
            var inputCantidad = control;
            inputCantidad.min = "0";
            var controlCantidad = document.createElement("span");
            controlCantidad.className = "campo-cantidad";
            var disminuir = boton("-", "campo-cantidad-boton", function () {
                var valor = Number.parseInt(inputCantidad.value, 10) || 0;
                inputCantidad.value = String(Math.max(0, valor - 1));
                inputCantidad.dispatchEvent(new Event("input", { bubbles: true }));
            });
            disminuir.setAttribute("aria-label", "Disminuir " + definicion.etiqueta);
            var aumentar = boton("+", "campo-cantidad-boton", function () {
                var valor = Number.parseInt(inputCantidad.value, 10) || 0;
                inputCantidad.value = String(valor + 1);
                inputCantidad.dispatchEvent(new Event("input", { bubbles: true }));
            });
            aumentar.setAttribute("aria-label", "Aumentar " + definicion.etiqueta);
            controlCantidad.append(disminuir, control, aumentar);
            control = controlCantidad;
        }
        var elemento = definicion.controlesCantidad ? control.querySelector("input") : control;
        if (definicion.soloLectura) elemento.readOnly = true;
        if (definicion.deshabilitado) elemento.disabled = true;
        if (definicion.nombre) elemento.name = definicion.nombre;
        if (definicion.valor) elemento.value = definicion.valor;
        if ((definicion.tipo === "date" || definicion.tipo === "datetime-local") && !definicion.soloLectura && !definicion.deshabilitado) {
            var linea = document.createElement("span");
            linea.className = "campo-fecha";
            var ahora = document.createElement("button");
            ahora.type = "button";
            ahora.className = "campo-ahora";
            var conHora = definicion.tipo === "datetime-local";
            ahora.textContent = conHora ? "Ahora" : "Hoy";
            ahora.addEventListener("click", function () {
                elemento.value = dependencias.momentoLocal(conHora);
                elemento.dispatchEvent(new Event("input", { bubbles: true }));
            });
            var validacionFecha = document.createElement("span");
            validacionFecha.className = "sgh-ui-validacion";
            validacionFecha.dataset.sghValidacion = "";
            validacionFecha.hidden = true;
            validacionFecha.setAttribute("role", "alert");
            linea.append(elemento, ahora);
            etiqueta.append(texto, linea, validacionFecha);
            return etiqueta;
        }
        var validacion = document.createElement("span");
        validacion.className = "sgh-ui-validacion";
        validacion.dataset.sghValidacion = "";
        validacion.hidden = true;
        validacion.setAttribute("role", "alert");
        etiqueta.append(texto, control, validacion);
        return etiqueta;
    }

    function inicializarSelectores(form) {
        if (!window.jQuery || !window.jQuery.fn.select2) return;
        function t(texto) {
            return window.SGHI18n ? window.SGHI18n.traducir(texto) : texto;
        }

        window.jQuery(form).find("select.selector-buscable").select2({
            width: "100%",
            minimumResultsForSearch: 0,
            language: {
                noResults: function () { return t("Sin resultados"); },
                searching: function () { return t("Buscando..."); }
            },
            dropdownParent: window.jQuery("#panel-lateral")
        }).on("select2:select select2:unselect", function () {
            this.dispatchEvent(new Event("change", { bubbles: true }));
        });
    }

    function actualizarSelector(select) {
        if (!window.jQuery || !window.jQuery.fn.select2) return;
        var elemento = window.jQuery(select);
        if (elemento.hasClass("select2-hidden-accessible")) elemento.trigger("change.select2");
    }

    function crear(secciones, accion, esAlta, alEnviar) {
        var form = document.createElement("form");
        form.className = "formulario";
        var error = document.createElement("p");
        error.className = "formulario-error";
        error.hidden = true;
        form.append(error);
        secciones.forEach(function (grupo) {
            if (grupo.titulo) {
                var titulo = document.createElement("h3");
                titulo.className = "formulario-titulo";
                titulo.textContent = grupo.titulo;
                form.append(titulo);
            }
            var grilla = document.createElement("div");
            grilla.className = "formulario-grilla";
            grupo.campos.forEach(function (definicion) {
                grilla.append(campoVisual(definicion, esAlta ? "Seleccione" : "Todos"));
            });
            form.append(grilla);
        });
        dependencias.enlazar(form);
        inicializarSelectores(form);
        var pie = document.createElement("div");
        pie.className = "formulario-pie";
        var guardar = boton(accion, "boton-primario", null);
        pie.append(boton("Cancelar", "boton-secundario", dependencias.cerrarPanel), guardar);
        form.append(pie);
        var actualizarEstado = function () {
            var valido = true;
            secciones.forEach(function (grupo) {
                grupo.campos.forEach(function (definicion) {
                    if (!definicion.requerido) return;
                    var control = form.querySelector("[name='" + definicion.nombre + "']");
                    var completo = control && String(control.value || "").trim() !== "";
                    var campoEtiqueta = control ? control.closest(".campo") : null;
                    if (campoEtiqueta) campoEtiqueta.classList.toggle("campo-error", !completo);
                    if (control) control.setAttribute("aria-invalid", completo ? "false" : "true");
                    var aviso = campoEtiqueta ? campoEtiqueta.querySelector("[data-sgh-validacion]") : null;
                    if (aviso) {
                        aviso.textContent = completo ? "" : "Este campo es obligatorio.";
                        aviso.hidden = completo;
                    }
                    if (!completo) valido = false;
                });
            });
            guardar.disabled = !valido;
        };
        form.addEventListener("input", actualizarEstado);
        form.addEventListener("change", actualizarEstado);
        actualizarEstado();
        window.setTimeout(actualizarEstado, 0);
        form.addEventListener("submit", function (evento) {
            evento.preventDefault();
            form.classList.add("validar");
            actualizarEstado();
            if (guardar.disabled) return;
            var promesa = alEnviar ? alEnviar(leer(form)) : null;
            if (!promesa || !promesa.then) {
                dependencias.cerrarPanel();
                return;
            }
            promesa.then(function () { dependencias.cerrarPanel(); }).catch(function (fallo) {
                dependencias.mostrarAviso(fallo.message, "error");
            });
        });
        return form;
    }

    function contenido(origen) {
        var valor = typeof origen === "function" ? origen() : origen;
        if (!valor) return [];
        if (valor.length && valor[0].campos) return valor;
        return [seccion("Criterios", valor)];
    }

    return {
        formulario: crear,
        campo: campo,
        boton: boton,
        seccion: seccion,
        contenido: contenido,
        aOpcion: aOpcion,
        inicializarSelectores: inicializarSelectores,
        actualizarSelector: actualizarSelector
    };
}
