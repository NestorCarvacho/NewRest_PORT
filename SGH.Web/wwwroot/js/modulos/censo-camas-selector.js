import {
    MAX_CAMAS_ABSOLUTO,
    capacidadEfectiva,
    radioDiaDeshabilitado,
    radioNocheDeshabilitado,
    ajustarCamasNoche,
    validarOcupacionCamas
} from "./censo-camas-logica.js";

function enterosHastaMax() {
    var lista = [];
    for (var i = 0; i <= MAX_CAMAS_ABSOLUTO; i++) lista.push(i);
    return lista;
}

function crearGrupoRadios(nombre, etiqueta, valores, seleccionado, esDeshabilitado) {
    var fieldset = document.createElement("fieldset");
    fieldset.className = "censo-camas-grupo";
    fieldset.dataset.grupo = nombre;
    var leyenda = document.createElement("legend");
    leyenda.className = "censo-camas-leyenda";
    leyenda.textContent = etiqueta;
    fieldset.append(leyenda);
    var fila = document.createElement("div");
    fila.className = "censo-camas-radios";
    valores.forEach(function (valor) {
        var id = nombre + "-" + valor;
        var etiquetaNodo = document.createElement("label");
        etiquetaNodo.className = "censo-camas-radio";
        etiquetaNodo.htmlFor = id;
        var input = document.createElement("input");
        input.type = "radio";
        input.name = nombre;
        input.id = id;
        input.value = String(valor);
        input.checked = seleccionado === valor;
        if (esDeshabilitado(valor)) {
            input.disabled = true;
            etiquetaNodo.classList.add("censo-camas-radio-deshabilitado");
        }
        etiquetaNodo.append(input, document.createTextNode(String(valor)));
        fila.append(etiquetaNodo);
    });
    fieldset.append(fila);
    return fieldset;
}

function leerRadioSeleccionado(fieldset, nombre) {
    if (!fieldset) return 0;
    var marcado = fieldset.querySelector('input[name="' + nombre + '"]:checked');
    if (!marcado || marcado.disabled) return 0;
    return parseInt(marcado.value, 10) || 0;
}

/**
 * Selector de camas con validación dinámica (HostEat / legacy «Ambos»).
 * @param {{ capacidadTotal: number, contenedor: HTMLElement, selectTurno: HTMLSelectElement, inputCantidad: HTMLInputElement, inputCantidadNoche: HTMLInputElement, onCambio?: function }} opciones
 */
export function montarSelectorCamasCenso(opciones) {
    var capacidad = capacidadEfectiva(opciones.capacidadTotal);
    var estado = { dia: 0, noche: 0, turno: opciones.selectTurno ? opciones.selectTurno.value : "D" };

    var raiz = document.createElement("div");
    raiz.className = "censo-camas-selector";
    raiz.setAttribute("data-capacidad", String(capacidad));
    opciones.contenedor.append(raiz);

    function notificar() {
        if (opciones.onCambio) opciones.onCambio();
    }

    function sincronizarInputsOcultos() {
        var turno = opciones.selectTurno ? opciones.selectTurno.value : "D";
        if (turno === "D") {
            opciones.inputCantidad.value = String(estado.dia);
            opciones.inputCantidadNoche.value = "0";
        } else if (turno === "N") {
            opciones.inputCantidad.value = String(estado.noche);
            opciones.inputCantidadNoche.value = "0";
        } else {
            opciones.inputCantidad.value = String(estado.dia);
            opciones.inputCantidadNoche.value = String(estado.noche);
        }
    }

    function pintar() {
        var turno = opciones.selectTurno ? opciones.selectTurno.value : "D";
        estado.turno = turno;
        raiz.replaceChildren();

        if (turno === "D") {
            var soloDia = crearGrupoRadios("censo-camas-dia", "Camas ocupadas (día)", enterosHastaMax(), estado.dia, function (i) {
                return radioDiaDeshabilitado(i, capacidad);
            });
            raiz.append(soloDia);
            soloDia.addEventListener("change", function (evento) {
                var objetivo = evento.target;
                if (!(objetivo instanceof HTMLInputElement) || objetivo.type !== "radio") return;
                estado.dia = parseInt(objetivo.value, 10) || 0;
                sincronizarInputsOcultos();
                notificar();
            });
        } else if (turno === "N") {
            var soloNoche = crearGrupoRadios("censo-camas-noche", "Camas ocupadas (noche)", enterosHastaMax(), estado.noche, function (i) {
                return radioDiaDeshabilitado(i, capacidad);
            });
            raiz.append(soloNoche);
            soloNoche.addEventListener("change", function (evento) {
                var objetivo = evento.target;
                if (!(objetivo instanceof HTMLInputElement) || objetivo.type !== "radio") return;
                estado.noche = parseInt(objetivo.value, 10) || 0;
                sincronizarInputsOcultos();
                notificar();
            });
        } else {
            var grupoDia = crearGrupoRadios("censo-camas-dia", "Camas ocupadas (día)", enterosHastaMax(), estado.dia, function (i) {
                return radioDiaDeshabilitado(i, capacidad);
            });
            var grupoNoche = crearGrupoRadios("censo-camas-noche", "Camas ocupadas (noche)", enterosHastaMax(), estado.noche, function (i) {
                return radioNocheDeshabilitado(i, capacidad, estado.dia);
            });
            raiz.append(grupoDia, grupoNoche);

            grupoDia.addEventListener("change", function (evento) {
                var objetivo = evento.target;
                if (!(objetivo instanceof HTMLInputElement) || objetivo.type !== "radio") return;
                estado.dia = parseInt(objetivo.value, 10) || 0;
                estado.noche = ajustarCamasNoche(estado.noche, capacidad, estado.dia);
                sincronizarInputsOcultos();
                pintar();
                notificar();
            });

            grupoNoche.addEventListener("change", function (evento) {
                var objetivo = evento.target;
                if (!(objetivo instanceof HTMLInputElement) || objetivo.type !== "radio") return;
                estado.noche = parseInt(objetivo.value, 10) || 0;
                sincronizarInputsOcultos();
                notificar();
            });
        }
        sincronizarInputsOcultos();
    }

    if (opciones.selectTurno) {
        opciones.selectTurno.addEventListener("change", function () {
            pintar();
            notificar();
        });
    }

    pintar();

    return {
        setCapacidad: function (nueva) {
            capacidad = capacidadEfectiva(nueva);
            raiz.setAttribute("data-capacidad", String(capacidad));
            estado.dia = Math.min(estado.dia, capacidad);
            estado.noche = ajustarCamasNoche(estado.noche, capacidad, estado.dia);
            pintar();
        },
        esValido: function () {
            var turno = opciones.selectTurno ? opciones.selectTurno.value : "D";
            var dia = turno === "N" ? 0 : estado.dia;
            var noche = turno === "D" ? 0 : (turno === "N" ? estado.noche : estado.noche);
            return validarOcupacionCamas(capacidad, turno, dia, noche) === null;
        },
        obtenerValores: function () {
            var turno = opciones.selectTurno ? opciones.selectTurno.value : "D";
            return { turno: turno, camasDia: estado.dia, camasNoche: estado.noche, capacidadTotal: capacidad };
        }
    };
}