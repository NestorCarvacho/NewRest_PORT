(function () {
    var barra = document.getElementById("aviso-cookies");
    if (!barra) return;

    var categorias = document.getElementById("cookies-categorias");
    var configurar = document.getElementById("cookies-configurar");
    var analiticas = document.getElementById("cookies-analiticas");
    var publicidad = document.getElementById("cookies-publicidad");
    var nombre = "sgh.cookies=";

    function leer() {
        var item = document.cookie.split("; ").filter(function (parte) { return parte.indexOf(nombre) === 0; })[0];
        if (!item) return null;
        var valor = decodeURIComponent(item.substring(nombre.length));
        if (valor === "aceptado") return { analiticas: true, publicidad: true };
        return {
            analiticas: valor.indexOf("analiticas") >= 0,
            publicidad: valor.indexOf("publicidad") >= 0
        };
    }

    function guardar(opcion) {
        var partes = ["necesarias"];
        if (opcion.analiticas) partes.push("analiticas");
        if (opcion.publicidad) partes.push("publicidad");
        document.cookie = nombre + partes.join(".") + "; path=/; max-age=31536000; samesite=lax";
        cerrar();
    }

    function abrir() {
        var actual = leer();
        analiticas.checked = !!(actual && actual.analiticas);
        publicidad.checked = !!(actual && actual.publicidad);
        barra.hidden = false;
        document.body.classList.add("con-aviso-cookies");
    }

    function cerrar() {
        barra.hidden = true;
        categorias.hidden = true;
        configurar.setAttribute("aria-expanded", "false");
        document.body.classList.remove("con-aviso-cookies");
    }

    document.getElementById("cookies-aceptar").addEventListener("click", function () {
        guardar({ analiticas: true, publicidad: true });
    });
    document.getElementById("cookies-rechazar").addEventListener("click", function () {
        guardar({ analiticas: false, publicidad: false });
    });
    document.getElementById("cookies-guardar").addEventListener("click", function () {
        guardar({ analiticas: analiticas.checked, publicidad: publicidad.checked });
    });
    configurar.addEventListener("click", function () {
        var abierto = categorias.hidden;
        categorias.hidden = !abierto;
        configurar.setAttribute("aria-expanded", abierto ? "true" : "false");
    });
    document.querySelectorAll("[data-preferencias-cookies]").forEach(function (boton) {
        boton.addEventListener("click", function (evento) {
            evento.preventDefault();
            abrir();
        });
    });

    if (!leer()) abrir();
})();
