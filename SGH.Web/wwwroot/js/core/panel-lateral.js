export function crearPanelLateral() {
    var panel = document.getElementById("panel-lateral");
    var velo = document.getElementById("velo");

    function abrir(titulo, contenido) {
        document.getElementById("panel-titulo").textContent = titulo;
        var cuerpo = document.getElementById("panel-cuerpo");
        if (contenido.hidden) {
            contenido.hidden = false;
            contenido.removeAttribute("aria-hidden");
        }
        cuerpo.replaceChildren(contenido);
        panel.hidden = false;
        velo.hidden = false;
    }

    function cerrar() {
        if (document.activeElement && panel.contains(document.activeElement)) {
            document.activeElement.blur();
        }
        var cuerpo = document.getElementById("panel-cuerpo");
        var form = cuerpo && cuerpo.querySelector("form.formulario-filtros-modulo");
        if (form && form._contenedorPadre) {
            form._contenedorPadre.append(form);
            form.hidden = true;
            form.setAttribute("aria-hidden", "true");
        }
        panel.hidden = true;
        velo.hidden = true;
    }

    return { abrir: abrir, cerrar: cerrar, velo: velo };
}
