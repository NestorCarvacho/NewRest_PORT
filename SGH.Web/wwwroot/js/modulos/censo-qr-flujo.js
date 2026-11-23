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
    var overlay = null;

    function cerrar() {
        if (!overlay) return;
        var visor = overlay.querySelector(".lector-qr-envoltorio");
        if (visor) detenerEscaner(visor);
        overlay.remove();
        overlay = null;
        document.body.classList.remove("censo-qr-wizard-abierto");
    }

    function crearOverlay() {
        cerrar();
        overlay = document.createElement("div");
        overlay.className = "censo-qr-wizard";
        overlay.setAttribute("role", "dialog");
        overlay.setAttribute("aria-modal", "true");
        overlay.innerHTML =
            '<div class="censo-qr-wizard-paso" data-pasos></div>' +
            '<div class="censo-qr-wizard-contenido" data-contenido></div>' +
            '<div class="censo-qr-wizard-pie" data-pie></div>';
        var btnCerrar = document.createElement("button");
        btnCerrar.type = "button";
        btnCerrar.className = "censo-qr-wizard-cerrar";
        btnCerrar.setAttribute("aria-label", "Cerrar");
        btnCerrar.textContent = "×";
        btnCerrar.addEventListener("click", cerrar);
        overlay.append(btnCerrar);
        document.body.append(overlay);
        document.body.classList.add("censo-qr-wizard-abierto");
        return overlay;
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

    function aplicarHabitacionDesdeQr(estado, textoQr) {
        var qr;
        try {
            qr = JSON.parse(textoQr);
        } catch (_) {
            dependencias.mostrarAviso("El código QR no es un JSON válido.", "error");
            return false;
        }
        if (!qr || qr.h == null) {
            dependencias.mostrarAviso("El código QR no contiene una habitación.", "error");
            return false;
        }
        estado.habitacionId = String(qr.h);
        estado.qrJson = textoQr;
        estado.capacidad = capacidadEfectiva(qr.c != null ? qr.c : (qr.nc != null ? qr.nc : 4));
        estado.habitacion = qr.n || qr.habitacion || ("Habitación " + qr.h);
        return true;
    }

    function guardar(estado, alTerminar) {
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
        return dependencias.enviar("/Censo/Mid/MidCenso.cs", payload).then(function () {
            dependencias.mostrarAviso("Censo QR guardado correctamente.", "success");
            cerrar();
            if (alTerminar) alTerminar();
        }).catch(function (error) {
            encolarCensoQr(payload, meta);
            actualizarBannerOffline();
            dependencias.mostrarAviso(error.message + " El censo quedó pendiente para sincronizar.", "warning");
            cerrar();
            if (alTerminar) alTerminar();
        });
    }

    function pasoEscanear(estado, navegar) {
        var cont = overlay.querySelector("[data-contenido]");
        var pie = overlay.querySelector("[data-pie]");
        cont.replaceChildren();
        pie.replaceChildren();
        var titulo = document.createElement("h2");
        titulo.className = "censo-qr-wizard-titulo";
        titulo.textContent = "Seleccione Habitación";
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
        iniciarEscaner(visorAncla, function (texto) {
            if (aplicarHabitacionDesdeQr(estado, texto)) navegar("censo");
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
        var cont = overlay.querySelector("[data-contenido]");
        var pie = overlay.querySelector("[data-pie]");
        cont.replaceChildren();
        pie.replaceChildren();
        var titulo = document.createElement("h2");
        titulo.className = "censo-qr-wizard-titulo";
        titulo.textContent = "Seleccione Habitación";
        var form = document.createElement("form");
        form.className = "censo-qr-habitacion-selector";
        form.id = "habitacionSelector";
        var selModulo = document.createElement("select");
        selModulo.name = "modulo";
        selModulo.className = "selector-buscable";
        var selPiso = document.createElement("select");
        selPiso.name = "piso";
        var selHab = document.createElement("select");
        selHab.name = "habitacion";
        form.append(
            dependencias.etiquetaCampo("Módulo", selModulo),
            dependencias.etiquetaCampo("Piso", selPiso),
            dependencias.etiquetaCampo("Habitación", selHab)
        );
        cont.append(titulo, form);

        var catalogo = { modulos: [], pisos: [], habitaciones: [] };
        dependencias.obtenerCatalogo().then(function (cat) {
            catalogo = cat || catalogo;
            llenarSelect(selModulo, catalogo.modulos, "Seleccione Módulo");
        });

        function pisosFiltrados(idModulo) {
            return (catalogo.pisos || []).filter(function (p) { return String(p.padreId) === String(idModulo); });
        }
        function habitacionesFiltradas(idPiso) {
            return (catalogo.habitaciones || []).filter(function (h) { return String(h.padreId) === String(idPiso); });
        }

        selModulo.addEventListener("change", function () {
            llenarSelect(selPiso, pisosFiltrados(selModulo.value), "Seleccione Piso");
            llenarSelect(selHab, [], "Seleccione Habitación");
        });
        selPiso.addEventListener("change", function () {
            llenarSelect(selHab, habitacionesFiltradas(selPiso.value), "Seleccione Habitación");
        });

        var btnAtras = dependencias.boton("Regresar", "boton-secundario", function () { navegar("escanear"); });
        var btnOk = dependencias.boton("Seleccionar Habitación", "boton-primario", function () {
            if (!selHab.value) {
                dependencias.mostrarAviso("Seleccione una habitación.", "error");
                return;
            }
            var hab = (catalogo.habitaciones || []).find(function (h) { return String(h.id) === selHab.value; });
            var mod = (catalogo.modulos || []).find(function (m) { return String(m.id) === selModulo.value; });
            var piso = (catalogo.pisos || []).find(function (p) { return String(p.id) === selPiso.value; });
            estado.habitacionId = selHab.value;
            estado.habitacion = hab ? hab.etiqueta : selHab.value;
            estado.moduloEtiqueta = mod ? mod.etiqueta : "";
            estado.pisoEtiqueta = piso ? piso.etiqueta : "";
            estado.capacidad = 4;
            estado.qrJson = JSON.stringify({ h: parseInt(selHab.value, 10), c: 4 });
            navegar("censo");
        });
        pie.append(btnAtras, btnOk);
    }

    function pasoCenso(estado, navegar) {
        var cont = overlay.querySelector("[data-contenido]");
        var pie = overlay.querySelector("[data-pie]");
        cont.replaceChildren();
        pie.replaceChildren();
        var titulo = document.createElement("h2");
        titulo.className = "censo-qr-wizard-titulo";
        titulo.textContent = estado.habitacion || "Censo";
        var detalle = document.createElement("p");
        detalle.className = "censo-qr-wizard-detalle";
        detalle.textContent = [estado.moduloEtiqueta, estado.pisoEtiqueta].filter(Boolean).join(" | ");
        var form = document.createElement("div");
        form.className = "censo-qr-form-censo";
        var inputFecha = document.createElement("input");
        inputFecha.type = "date";
        inputFecha.name = "fecha";
        inputFecha.value = estado.fecha || dependencias.fechaActual();
        var selectTurno = document.createElement("select");
        selectTurno.name = "turno";
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

        var btnAtras = dependencias.boton("Regresar", "boton-secundario", function () { navegar("escanear"); });
        var btnSig = dependencias.boton("Continuar", "boton-primario", function () {
            if (!selector.esValido()) {
                dependencias.mostrarAviso("Revise la ocupación de camas.", "error");
                return;
            }
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
        var cont = overlay.querySelector("[data-contenido]");
        var pie = overlay.querySelector("[data-pie]");
        cont.replaceChildren();
        pie.replaceChildren();
        var titulo = document.createElement("h2");
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
        var btnAtras = dependencias.boton("Regresar", "boton-secundario", function () { navegarInterno("censo"); });
        var btnGuardar = dependencias.boton("Guardar", "boton-primario", function () {
            guardar(estado, alTerminar);
        });
        pie.append(btnAtras, btnGuardar);
    }

    var navegarInterno = null;

    function abrir(opciones) {
        opciones = opciones || {};
        var estado = {
            fecha: dependencias.fechaActual(),
            turno: "D",
            camasDia: 0,
            camasNoche: 0,
            capacidad: 4
        };
        crearOverlay();
        var pasosEl = overlay.querySelector("[data-pasos]");
        var indicePaso = { escanear: 1, manual: 1, censo: 2, confirmar: 3 };

        function navegar(destino) {
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