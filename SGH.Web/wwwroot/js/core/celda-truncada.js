/** Texto truncado en tabla; tooltip flotante alineado al UI del escritorio. */
var capaTooltip = null;

export function escaparHtml(texto) {
    return String(texto ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");
}

export function renderCeldaTruncada(valor) {
    var texto = valor == null ? "" : String(valor);
    if (!texto) return "";
    var esc = escaparHtml(texto);
    return '<span class="celda-truncada" data-tooltip="' + esc + '">' + esc + "</span>";
}

export function renderizadorCeldaTruncada() {
    return function (dato, tipo) {
        if (tipo !== "display" && tipo !== "filter") return dato;
        return renderCeldaTruncada(dato);
    };
}

function obtenerCapaTooltip() {
    if (capaTooltip && capaTooltip.isConnected) return capaTooltip;
    capaTooltip = document.getElementById("tooltip-celda");
    if (!capaTooltip) {
        capaTooltip = document.createElement("div");
        capaTooltip.id = "tooltip-celda";
        capaTooltip.className = "tooltip-celda";
        capaTooltip.hidden = true;
        capaTooltip.setAttribute("role", "tooltip");
        document.body.append(capaTooltip);
    }
    return capaTooltip;
}

function posicionarTooltip(celda, capa) {
    var rect = celda.getBoundingClientRect();
    var margen = 8;
    capa.style.left = Math.max(margen, rect.left) + "px";
    var arriba = rect.top - capa.offsetHeight - 6;
    if (arriba < margen) {
        capa.style.top = rect.bottom + 6 + "px";
    } else {
        capa.style.top = arriba + "px";
    }
    var maxIzq = window.innerWidth - capa.offsetWidth - margen;
    if (parseFloat(capa.style.left) > maxIzq) capa.style.left = Math.max(margen, maxIzq) + "px";
}

export function enlazarTooltipsCelda(contenedor) {
    if (!contenedor) return;
    var capa = obtenerCapaTooltip();
    contenedor.querySelectorAll(".celda-truncada[data-tooltip]").forEach(function (celda) {
        if (celda.dataset.tooltipEnlazado) return;
        celda.dataset.tooltipEnlazado = "1";
        celda.addEventListener("mouseenter", function () {
            var texto = celda.getAttribute("data-tooltip");
            if (!texto) return;
            capa.textContent = texto;
            capa.hidden = false;
            requestAnimationFrame(function () { posicionarTooltip(celda, capa); });
        });
        celda.addEventListener("mousemove", function () {
            if (!capa.hidden) posicionarTooltip(celda, capa);
        });
        celda.addEventListener("mouseleave", function () {
            capa.hidden = true;
        });
    });
}

export function ocultarTooltipCelda() {
    var capa = obtenerCapaTooltip();
    capa.hidden = true;
}