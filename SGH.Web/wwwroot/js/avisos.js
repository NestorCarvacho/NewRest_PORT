(function () {
    document.querySelectorAll(".aviso").forEach(function (aviso) {
        var temporizador;

        function ocultar() {
            if (aviso.classList.contains("aviso-sale")) return;
            aviso.classList.add("aviso-sale");
            aviso.addEventListener("animationend", function () { aviso.remove(); }, { once: true });
        }

        function programar() {
            window.clearTimeout(temporizador);
            temporizador = window.setTimeout(ocultar, 3000);
        }

        var cerrar = aviso.querySelector(".aviso-cerrar");
        if (cerrar) {
            cerrar.addEventListener("click", function () {
                window.clearTimeout(temporizador);
                ocultar();
            });
        }

        aviso.addEventListener("mouseenter", function () { window.clearTimeout(temporizador); });
        aviso.addEventListener("mouseleave", programar);
        programar();
    });
})();
