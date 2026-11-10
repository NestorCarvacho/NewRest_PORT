export function crearAvisos() {
    function mostrarAviso(mensaje, tipo) {
        var contenedor = document.getElementById("avisos-escritorio");
        if (!contenedor) return;
        var aviso = document.createElement("div");
        aviso.className = "aviso-escritorio aviso-escritorio-" + tipo + " aviso-nuevo";
        aviso.setAttribute("role", "alert");
        var texto = document.createElement("span");
        texto.textContent = mensaje;
        var cerrar = document.createElement("button");
        cerrar.type = "button";
        cerrar.className = "aviso-escritorio-cerrar";
        cerrar.setAttribute("aria-label", "Cerrar aviso");
        cerrar.textContent = "×";
        var retirar = function () { aviso.remove(); };
        cerrar.addEventListener("click", retirar);
        aviso.append(texto, cerrar);
        contenedor.append(aviso);
        window.setTimeout(retirar, 6000);
        return aviso;
    }

    function mostrarEstado(nodo, mensaje) {
        var estado = nodo.querySelector(".modulo-estado");
        if (!estado) return;
        if (mensaje === "Cargando datos...") {
            if (nodo._avisoCarga) nodo._avisoCarga.remove();
            nodo._avisoCarga = mostrarAviso(mensaje, "info");
        } else if (nodo._avisoCarga) {
            nodo._avisoCarga.remove();
            nodo._avisoCarga = null;
        }
        if (mensaje && mensaje !== "Cargando datos...") mostrarAviso(mensaje, "error");
        estado.hidden = !mensaje || mensaje === "Cargando datos...";
        estado.textContent = mensaje || "";
        estado.classList.toggle("cargando", mensaje === "Cargando datos...");
        estado.setAttribute("aria-live", "polite");
    }

    return { mostrarAviso: mostrarAviso, mostrarEstado: mostrarEstado };
}
