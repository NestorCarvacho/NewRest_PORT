(function () {
    function ocultar(aviso) {
        if (!aviso || !aviso.isConnected || aviso.classList.contains("aviso-sale")) return;
        aviso.classList.add("aviso-sale");
        var quitado = false;
        function quitar() {
            if (quitado) return;
            quitado = true;
            aviso.remove();
        }
        aviso.addEventListener("animationend", quitar, { once: true });
        window.setTimeout(quitar, 500);
    }

    document.querySelectorAll(".aviso-sale").forEach(function (aviso) {
        window.setTimeout(function () { aviso.remove(); }, 500);
    });

    document.querySelectorAll(".aviso[data-espera]").forEach(function (aviso) {
        var espera = parseInt(aviso.getAttribute("data-espera"), 10);
        if (isNaN(espera) || espera < 0) espera = 3000;
        window.setTimeout(function () { ocultar(aviso); }, espera);
    });

    document.querySelectorAll(".aviso-cerrar").forEach(function (cerrar) {
        cerrar.addEventListener("click", function () {
            ocultar(cerrar.closest(".aviso"));
        });
    });
})();
