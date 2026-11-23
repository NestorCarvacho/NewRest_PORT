import { encolarCensoQr } from "../core/colaCensoQr.js";
import { actualizarBannerOffline } from "../core/colaOffline.js";
import { capacidadEfectiva, validarOcupacionCamas } from "./censo-camas-logica.js";
import { montarSelectorCamasCenso } from "./censo-camas-selector.js";
import { iniciarEscaner, detenerEscaner } from "./censo-qr-escaner.js";

function turnoLegible(codigo) {
    if (codigo === "D") return "Día";
    if (codigo === "N") return "Noche";
    if (codigo === "A") return "Ambos";
    return codigo || "";
}

export function crearCensoQrFlujo(dependencias) {
    var raiz = null;
    var panelCierraEnlazado = false;

    function detenerEscanerEnRaiz() {
        if (!raiz) return;
        var visor = raiz.querySelector(".lector-qr-envoltorio");
        if (visor) detenerEscaner(visor);
    }

    function enlazarCierrePanel() {
        if (panelCierraEnlazado) return;
        panelCierraEnlazado = true;
        ["panel-cerrar", "velo"].forEach(function (id) {
            var nodo = document.getElementById(id);
            if (nodo) nodo.addEventListener("click", detenerEscanerEnRaiz);
        });
    }

    function cerrar() {
        detenerEscanerEnRaiz();
        raiz = null;
        if (dependencias.cerrarPanel) dependencias.cerrarPanel();
    }

    function tituloPanel(paso) {
        var titulos = {
            escanear: "Censo QR — Escanear habitación",
            manual: "Censo QR — Seleccionar habitación",
            censo: "Censo QR — Datos del censo",
            confirmar: "Censo QR — Confirmación"
        };
        var el = document.getElementById("panel-titulo");
        if (el) el.textContent = titulos[paso] || "Censo QR";
    }

    function marcarRequerido(control, completo) {
        if (!control) return;
        var campo = control.closest(".campo");
        if (campo) campo.classList.toggle("campo-error", !completo);
        control.setAttribute("aria-invalid", completo ? "false" : "true");
    }

    function valorCompleto(control) {
        return control && String(control.value || "").trim() !== "";
    }

    function prepararRaiz() {
        detenerEscanerEnRaiz();
        raiz = document.createElement("div");
        raiz.className = "censo-qr-wizard";
        raiz.innerHTML =
            '<div class="censo-qr-wizard-paso" data-pasos></div>' +
            '<div class="censo-qr-wizard-contenido" data-contenido></div>' +
            '<div class="censo-qr-wizard-pie" data-pie></div>';
        return raiz;
    }

    function pintarPasos(contenedor, activo) {
        var pasos = [
            { n: 1, etiqueta: "Escanear" },
            { n: 2, etiqueta: "Censo" },
            { n: 3, etiqueta: "Confirmación" }
        ];
        contenedor.replaceChildren();
        pasos.forEach(function (p) {
            var item = document.createElement("div");
            item.className = "censo-qr-paso" + (p.n === activo ? " censo-qr-paso-activo" : "");
            item.innerHTML = '<span class="censo-qr-paso-numero">' + p.n + '</span><span class="censo-qr-paso-etiqueta">' + p.etiqueta + "</span>";
            contenedor.append(item);
        });
    }

    function consultarHabitacionQr(habitacionId) {
        return dependencias.enviar("/Censo/Mid/MidCenso.cs", {
            opcion: "obtenerhabitacionqr",
            habitacionId: String(habitacionId)
        });
    }

    function aplicarDetalleHabitacion(estado, detalle, textoQr) {
        estado.habitacionId = String(detalle.habitacionId);
        estado.habitacion = detalle.habitacion || ("Habitación " + detalle.habitacionId);
        estado.moduloEtiqueta = detalle.modulo || "";
        estado.pisoEtiqueta = detalle.piso || "";
        estado.capacidad = capacidadEfectiva(detalle.capacidad);
        if (textoQr) estado.qrJson = textoQr;
    }

    function aplicarHabitacionDesdeQr(estado, textoQr) {
        var qr;
        try {
            qr = JSON.parse(textoQr);
        } catch (_) {
            dependencias.mostrarAviso("El código QR no es un JSON válido.", "error");
            return Promise.resolve(false);
        }
        if (!qr || qr.h == null) {
            dependencias.mostrarAviso("El código QR no contiene una habitación.", "error");
            return Promise.resolve(false);
        }
        return consultarHabitacionQr(qr.h).then(function (detalle) {
            aplicarDetalleHabitacion(estado, detalle, textoQr);
            return true;
        }).catch(function () {
            estado.habitacionId = String(qr.h);
            estado.qrJson = textoQr;
            estado.capacidad = capacidadEfectiva(qr.c != null ? qr.c : (qr.nc != null ? qr.nc : 4));
            estado.habitacion = qr.n || qr.habitacion || ("Habitación " + qr.h);
            estado.moduloEtiqueta = qr.m || qr.modulo || "";
            estado.pisoEtiqueta = qr.p || qr.piso || "";
            return true;
        });
    }

    function guardar(estado, alTerminar) {
        if (!estado.habitacionId) {
            dependencias.mostrarAviso("Seleccione una habitación.", "error");
            return Promise.resolve();
        }
        if (!estado.fecha) {
            dependencias.mostrarAviso("Debe ingresar la fecha de censo.", "error");
            return Promise.resolve();
        }
        if (!estado.turno) {
            dependencias.mostrarAviso("Seleccione un turno válido.", "error");
            return Promise.resolve();
        }
        var camasDia = estado.turno === "N" ? 0 : estado.camasDia;
        var camasNoche = estado.turno === "D" ? 0 : (estado.turno === "N" ? estado.camasNoche : estado.camasNoche);
        var errorCamas = validarOcupacionCamas(estado.capacidad, estado.turno, camasDia, camasNoche);
        if (errorCamas) {
            dependencias.mostrarAviso(errorCamas, "error");
            return Promise.resolve();
        }
        var payload = {
            opcion: "guardarqr",
            habitacionId: estado.habitacionId,
            fecha: estado.fecha,
            turno: estado.turno,
            cantidad: String(estado.turno === "N" ? camasNoche : camasDia),
            cantidadNoche: String(camasNoche),
            capacidadHabitacion: String(estado.capacidad)
        };
        var meta = {
            habitacion: estado.habitacion,
            modulo: estado.moduloEtiqueta,
            piso: estado.pisoEtiqueta,
            turnoNombre: turnoLegible(estado.turno),
            camasOcupadas: estado.turno === "N" ? camasNoche : camasDia
        };
        if (estado.turno === "A") {
            meta.camasOcupadasDia = camasDia;
            meta.camasOcupadasNoche = camasNoche;
        }
        var mensajeExito = estado.turno === "A"
            ? "Censo QR guardado: turno Día (" + camasDia + ") y turno Noche (" + camasNoche + ")."
            : "Censo QR guardado correctamente.";
        return dependencias.enviar("/Censo/Mid/MidCenso.cs", payload).then(function () {
            dependencias.mostrarAviso(mensajeExito, "success");
            cerrar();
            if (alTerminar) alTerminar(estado.fecha);
        }).catch(function (error) {
            encolarCensoQr(payload, meta);
            actualizarBannerOffline();
            dependencias.mostrarAviso(error.message + " El censo quedó pendiente para sincronizar.", "warning");
            cerrar();
            if (alTerminar) alTerminar(estado.fecha);
        });
    }

    function pasoEscanear(estado, navegar) {
        var cont = raiz.querySelector("[data-contenido]");
        var pie = raiz.querySelector("[data-pie]");
        cont.replaceChildren();
        pie.replaceChildren();
        var titulo = document.createElement("h3");
        titulo.className = "censo-qr-wizard-titulo";
        titulo.textContent = "Escanee o elija la habitación";
        var visorAncla = document.createElement("div");
        visorAncla.className = "censo-qr-wizard-visor-ancla";
        var btnManual = document.createElement("button");
        btnManual.type = "button";
        btnManual.className = "censo-qr-boton-habitacion-manual";
        btnManual.title = "Seleccionar habitación manualmente";
        btnManual.setAttribute("aria-label", "Seleccionar habitación manualmente");
        btnManual.textContent = "⌂";
        btnManual.addEventListener("click", function () { navegar("manual"); });
        visorAncla.append(btnManual);
        cont.append(titulo, visorAncla);
        var btnCerrar = dependencias.boton("Cancelar", "boton-secundario", cerrar);
        pie.append(btnCerrar);
        iniciarEscaner(visorAncla, function (texto) {
            aplicarHabitacionDesdeQr(estado, texto).then(function (ok) {
                if (ok) navegar("censo");
            });
        }, dependencias.mostrarAviso);
    }

    function llenarSelect(select, opciones, etiquetaVacia) {
        select.replaceChildren();
        var vacio = document.createElement("option");
        vacio.value = "";
        vacio.textContent = etiquetaVacia;
        select.append(vacio);
        (opciones || []).forEach(function (op) {
            var item = document.createElement("option");
            item.value = String(op.id != null ? op.id : op.value);
            item.textContent = op.etiqueta || op.text;
            if (op.padreId != null) item.dataset.padre = String(op.padreId);
            select.append(item);
        });
    }

    function pasoManual(estado, navegar) {
        var cont = raiz.querySelector("[data-contenido]");
        var pie = raiz.querySelector("[data-pie]");
        cont.replaceChildren();
        pie.replaceChildren();
        detenerEscanerEnRaiz();
        var titulo = document.createElement("h3");
        titulo.className = "censo-qr-wizard-titulo";
        titulo.textContent = "Seleccione habitación";
        var form = document.createElement("form");
        form.className = "censo-qr-habitacion-selector formulario";
        form.id = "habitacionSelector";
        form.noValidate = true;
        var selModulo = document.createElement("select");
        selModulo.name = "modulo";
        selModulo.className = "selector-buscable";
        selModulo.required = true;
        var selPiso = document.createElement("select");
        selPiso.name = "piso";
        selPiso.required = true;
        var selTipo = document.createElement("select");
        selTipo.name = "tipoInstalacion";
        selTipo.required = true;
        var selHab = document.createElement("select");
        selHab.name = "habitacion";
        selHab.required = true;
        form.append(
            dependencias.etiquetaCampo("Módulo", selModulo),
            dependencias.etiquetaCampo("Piso", selPiso),
            dependencias.etiquetaCampo("Tipo de instalación", selTipo),
            dependencias.etiquetaCampo("Habitación", selHab)
        );
        cont.append(titulo, form);

        var catalogo = { modulos: [], pisos: [], habitaciones: [], tiposInstalacion: [], habitacionesQr: [] };
        dependencias.obtenerCatalogo().then(function (cat) {
            catalogo = cat || catalogo;
            llenarSelect(selModulo, catalogo.modulos, "Seleccione Módulo");
            llenarSelect(selTipo, catalogo.tiposInstalacion, "Seleccione Tipo");
        });

        function pisosFiltrados(idModulo) {
            return (catalogo.pisos || []).filter(function (p) { return String(p.padreId) === String(idModulo); });
        }
        function habitacionesQrFiltradas(idPiso, idTipo) {
            return (catalogo.habitacionesQr || []).filter(function (h) {
                return String(h.padreId) === String(idPiso)
                    && String(h.tipoInstalacionId) === String(idTipo);
            });
        }

        function limpiarErroresManual() {
            [selModulo, selPiso, selTipo, selHab].forEach(function (sel) { marcarRequerido(sel, true); });
        }

        function validarManual() {
            form.classList.add("validar");
            var ok = true;
            [selModulo, selPiso, selTipo, selHab].forEach(function (sel) {
                var completo = valorCompleto(sel);
                marcarRequerido(sel, completo);
                if (!completo) ok = false;
            });
            if (!ok) dependencias.mostrarAviso("Complete módulo, piso, tipo y habitación.", "error");
            return ok;
        }

        [selModulo, selPiso, selTipo, selHab].forEach(function (sel) {
            sel.addEventListener("change", limpiarErroresManual);
        });

        selModulo.addEventListener("change", function () {
            llenarSelect(selPiso, pisosFiltrados(selModulo.value), "Seleccione Piso");
            llenarSelect(selHab, [], "Seleccione Habitación");
        });
        selPiso.addEventListener("change", function () {
            llenarSelect(selHab, habitacionesQrFiltradas(selPiso.value, selTipo.value), "Seleccione Habitación");
        });
        selTipo.addEventListener("change", function () {
            llenarSelect(selHab, habitacionesQrFiltradas(selPiso.value, selTipo.value), "Seleccione Habitación");
        });

        var btnAtras = dependencias.boton("Regresar", "boton-secundario", function () { navegar("escanear"); });
        var btnOk = dependencias.boton("Seleccionar Habitación", "boton-primario", function () {
            if (!validarManual()) return;
            var hab = (catalogo.habitacionesQr || []).find(function (h) { return String(h.id) === selHab.value; });
            var mod = (catalogo.modulos || []).find(function (m) { return String(m.id) === selModulo.value; });
            var piso = (catalogo.pisos || []).find(function (p) { return String(p.id) === selPiso.value; });
            var capacidadLocal = hab && hab.capacidad != null ? hab.capacidad : 4;
            consultarHabitacionQr(selHab.value).then(function (detalle) {
                aplicarDetalleHabitacion(estado, detalle, null);
                estado.qrJson = JSON.stringify({ h: parseInt(selHab.value, 10), c: detalle.capacidad });
                navegar("censo");
            }).catch(function () {
                estado.habitacionId = selHab.value;
                estado.habitacion = hab ? hab.etiqueta : selHab.value;
                estado.moduloEtiqueta = mod ? mod.etiqueta : "";
                estado.pisoEtiqueta = piso ? piso.etiqueta : "";
                estado.capacidad = capacidadEfectiva(capacidadLocal);
                estado.qrJson = JSON.stringify({ h: parseInt(selHab.value, 10), c: capacidadLocal });
                navegar("censo");
            });
        });
        pie.append(btnAtras, btnOk);
    }

    function pasoCenso(estado, navegar) {
        var cont = raiz.querySelector("[data-contenido]");
        var pie = raiz.querySelector("[data-pie]");
        cont.replaceChildren();
        pie.replaceChildren();
        if (!estado.habitacionId) {
            dependencias.mostrarAviso("Seleccione una habitación antes de continuar.", "error");
            navegar("escanear");
            return;
        }
        var titulo = document.createElement("h3");
        titulo.className = "censo-qr-wizard-titulo";
        titulo.textContent = estado.habitacion || "Censo";
        var detalle = document.createElement("p");
        detalle.className = "censo-qr-wizard-detalle";
        detalle.textContent = [estado.moduloEtiqueta, estado.pisoEtiqueta].filter(Boolean).join(" | ");
        var form = document.createElement("form");
        form.className = "censo-qr-form-censo formulario";
        form.noValidate = true;
        var inputFecha = document.createElement("input");
        inputFecha.type = "date";
        inputFecha.name = "fecha";
        inputFecha.required = true;
        inputFecha.value = estado.fecha || dependencias.fechaActual();
        var selectTurno = document.createElement("select");
        selectTurno.name = "turno";
        selectTurno.required = true;
        [["D", "Día"], ["N", "Noche"], ["A", "Ambos"]].forEach(function (par) {
            var op = document.createElement("option");
            op.value = par[0];
            op.textContent = par[1];
            if (par[0] === (estado.turno || "D")) op.selected = true;
            selectTurno.append(op);
        });
        var hiddenDia = document.createElement("input");
        hiddenDia.type = "hidden";
        hiddenDia.name = "cantidad";
        hiddenDia.value = "0";
        var hiddenNoche = document.createElement("input");
        hiddenNoche.type = "hidden";
        hiddenNoche.name = "cantidadNoche";
        hiddenNoche.value = "0";
        var anclaCamas = document.createElement("div");
        anclaCamas.className = "censo-camas-ancla";
        form.append(
            dependencias.etiquetaCampo("Fecha de censo", inputFecha),
            dependencias.etiquetaCampo("Turno", selectTurno),
            hiddenDia,
            hiddenNoche,
            anclaCamas
        );
        cont.append(titulo, detalle, form);

        var selector = montarSelectorCamasCenso({
            capacidadTotal: estado.capacidad,
            contenedor: anclaCamas,
            selectTurno: selectTurno,
            inputCantidad: hiddenDia,
            inputCantidadNoche: hiddenNoche
        });

        function validarCenso() {
            form.classList.add("validar");
            var okFecha = valorCompleto(inputFecha);
            marcarRequerido(inputFecha, okFecha);
            var okTurno = valorCompleto(selectTurno);
            marcarRequerido(selectTurno, okTurno);
            if (!okFecha) {
                dependencias.mostrarAviso("Debe ingresar la fecha de censo.", "error");
                return false;
            }
            if (!okTurno) {
                dependencias.mostrarAviso("Seleccione un turno válido.", "error");
                return false;
            }
            if (!selector.esValido()) {
                dependencias.mostrarAviso("Revise la ocupación de camas.", "error");
                return false;
            }
            return true;
        }

        inputFecha.addEventListener("change", function () { marcarRequerido(inputFecha, valorCompleto(inputFecha)); });
        selectTurno.addEventListener("change", function () { marcarRequerido(selectTurno, valorCompleto(selectTurno)); });

        var btnAtras = dependencias.boton("Regresar", "boton-secundario", function () { navegar("escanear"); });
        var btnSig = dependencias.boton("Continuar", "boton-primario", function () {
            if (!validarCenso()) return;
            var vals = selector.obtenerValores();
            estado.fecha = inputFecha.value;
            estado.turno = selectTurno.value;
            estado.camasDia = vals.camasDia;
            estado.camasNoche = vals.camasNoche;
            navegar("confirmar");
        });
        pie.append(btnAtras, btnSig);
    }

    function pasoConfirmar(estado, alTerminar) {
        var cont = raiz.querySelector("[data-contenido]");
        var pie = raiz.querySelector("[data-pie]");
        cont.replaceChildren();
        pie.replaceChildren();
        var titulo = document.createElement("h3");
        titulo.className = "censo-qr-wizard-titulo";
        titulo.textContent = "Confirmación";
        var lista = document.createElement("ul");
        lista.className = "censo-qr-resumen";
        var camasTexto = estado.turno === "A"
            ? "Día: " + estado.camasDia + " · Noche: " + estado.camasNoche
            : "Camas: " + (estado.turno === "N" ? estado.camasNoche : estado.camasDia);
        [
            ["Habitación", estado.habitacion],
            ["Fecha", estado.fecha],
            ["Turno", turnoLegible(estado.turno)],
            ["Ocupación", camasTexto]
        ].forEach(function (par) {
            var li = document.createElement("li");
            li.textContent = par[0] + ": " + par[1];
            lista.append(li);
        });
        cont.append(titulo, lista);
        if (estado.turno === "A") {
            var nota = document.createElement("p");
            nota.className = "censo-qr-nota-ambos";
            nota.textContent = "Al guardar se registran dos censos en el servidor: uno con turno Día y otro con turno Noche.";
            cont.append(nota);
        }
        var btnAtras = dependencias.boton("Regresar", "boton-secundario", function () { navegarInterno("censo"); });
        var btnGuardar = dependencias.boton("Guardar", "boton-primario", function () {
            guardar(estado, alTerminar);
        });
        pie.append(btnAtras, btnGuardar);
    }

    var navegarInterno = null;

    function abrir(opciones) {
        if (!dependencias.abrirPanel) {
            dependencias.mostrarAviso("No se pudo abrir el panel lateral.", "error");
            return;
        }
        opciones = opciones || {};
        enlazarCierrePanel();
        var estado = {
            fecha: dependencias.fechaActual(),
            turno: "D",
            camasDia: 0,
            camasNoche: 0,
            capacidad: 4
        };
        prepararRaiz();
        dependencias.abrirPanel("Censo QR", raiz);
        var pasosEl = raiz.querySelector("[data-pasos]");
        var indicePaso = { escanear: 1, manual: 1, censo: 2, confirmar: 3 };

        function navegar(destino) {
            tituloPanel(destino);
            pintarPasos(pasosEl, indicePaso[destino] || 1);
            if (destino === "escanear") pasoEscanear(estado, navegar);
            else if (destino === "manual") pasoManual(estado, navegar);
            else if (destino === "censo") pasoCenso(estado, navegar);
            else if (destino === "confirmar") pasoConfirmar(estado, opciones.alCerrar);
        }
        navegarInterno = navegar;
        navegar("escanear");
    }

    return { abrir: abrir, cerrar: cerrar };
}