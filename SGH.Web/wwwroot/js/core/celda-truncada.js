/** Texto truncado en tabla; el nombre completo se muestra en tooltip al hover. */
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
    return '<span class="celda-truncada" title="' + esc + '">' + esc + "</span>";
}

export function renderizadorCeldaTruncada() {
    return function (dato, tipo) {
        if (tipo !== "display" && tipo !== "filter") return dato;
        return renderCeldaTruncada(dato);
    };
}