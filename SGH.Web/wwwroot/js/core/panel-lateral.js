export function crearPanelLateral() {
    var panel = document.getElementById("panel-lateral");
    var velo = document.getElementById("velo");

    function abrir(titulo, contenido) {
        document.getElementById("panel-titulo").textContent = titulo;
        var cuerpo = document.getElementById("panel-cuerpo");
        cuerpo.replaceChildren(contenido);
        panel.hidden = false;
        velo.hidden = false;
    }

    function cerrar() {
        if (document.activeElement && panel.contains(document.activeElement)) {
            document.activeElement.blur();
        }
        panel.hidden = true;
        velo.hidden = true;
    }

    return { abrir: abrir, cerrar: cerrar, velo: velo };
}
