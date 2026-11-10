(function () {
    "use strict";

    var clave = "sgh.tema";
    var tema = "oscuro";
    try {
        var guardado = localStorage.getItem(clave);
        if (guardado === "claro" || guardado === "oscuro") tema = guardado;
    } catch (error) {
        console.warn("No se pudo leer la preferencia de tema.", error);
    }

    document.documentElement.dataset.tema = tema;

    document.addEventListener("DOMContentLoaded", function () {
        var selectores = document.querySelectorAll("[data-selector-tema]");
        function actualizar() {
            document.documentElement.dataset.tema = tema;
            selectores.forEach(function (selector) {
                selector.setAttribute("aria-checked", tema === "claro" ? "true" : "false");
            });
        }

        actualizar();
        selectores.forEach(function (selector) {
            selector.addEventListener("click", function () {
                tema = tema === "oscuro" ? "claro" : "oscuro";
                actualizar();
                try {
                    localStorage.setItem(clave, tema);
                } catch (error) {
                    console.warn("No se pudo guardar la preferencia de tema.", error);
                }
            });
        });
    });
})();
