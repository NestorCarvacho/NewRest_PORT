export function bloquearBotonSegundos(boton, segundos) {
    if (!boton || !segundos || segundos < 1) return;

    var etiqueta = boton.getAttribute("data-etiqueta");
    if (!etiqueta) {
        etiqueta = (boton.textContent || "").trim();
        if (etiqueta) boton.setAttribute("data-etiqueta", etiqueta);
        else etiqueta = "…";
    }

    if (boton._relojEspera != null) {
        window.clearInterval(boton._relojEspera);
        boton._relojEspera = null;
    }

    boton.disabled = true;
    var hasta = Date.now() + segundos * 1000;

    function pintar() {
        var resta = Math.max(0, Math.ceil((hasta - Date.now()) / 1000));
        if (resta <= 0) {
            if (boton._relojEspera != null) {
                window.clearInterval(boton._relojEspera);
                boton._relojEspera = null;
            }
            boton.disabled = false;
            boton.textContent = etiqueta;
            return;
        }
        var minutos = Math.floor(resta / 60);
        var sobra = resta % 60;
        boton.textContent = "Espere " + minutos + ":" + String(sobra).padStart(2, "0");
    }

    boton._relojEspera = window.setInterval(pintar, 1000);
    pintar();
}