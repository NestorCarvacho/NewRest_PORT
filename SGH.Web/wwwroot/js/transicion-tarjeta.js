(function () {
    var raiz = document.documentElement;
    var tarjeta = document.querySelector(".login-card");

    if (raiz.classList.contains("tarjeta-crece") && tarjeta) {
        var anchoDesde = raiz.style.getPropertyValue("--tarjeta-desde-ancho").trim();
        var altoDesde = raiz.style.getPropertyValue("--tarjeta-desde-alto").trim();
        raiz.classList.remove("tarjeta-crece");
        var ancho = tarjeta.offsetWidth;
        var alto = tarjeta.offsetHeight;
        tarjeta.style.overflow = "hidden";
        tarjeta.style.maxHeight = "none";
        var animacion = tarjeta.animate([
            { height: altoDesde, width: anchoDesde },
            { height: alto + "px", width: ancho + "px" }
        ], {
            duration: 520,
            easing: "cubic-bezier(0.16, 1, 0.3, 1)"
        });
        animacion.onfinish = function () {
            tarjeta.style.overflow = "";
            tarjeta.style.maxHeight = "";
        };
    }

    function ruta(url) {
        try {
            return new URL(url, location.origin).pathname.replace(/\/+$/, "").toLowerCase();
        } catch (error) {
            return "";
        }
    }

    function esPar(origen, destino) {
        var desde = ruta(origen);
        var hacia = ruta(destino);
        var login = "/cuenta/login";
        var clave = "/cuenta/cambiarclave";
        return (desde.endsWith(login) && hacia.endsWith(clave))
            || (desde.endsWith(clave) && hacia.endsWith(login));
    }

    document.addEventListener("click", function (evento) {
        var enlace = evento.target.closest("a[href]");
        if (!enlace || !tarjeta || !esPar(location.href, enlace.href)) return;
        var rect = tarjeta.getBoundingClientRect();
        sessionStorage.setItem("sgh.tarjeta", JSON.stringify({
            ruta: location.pathname,
            ancho: Math.round(rect.width),
            alto: Math.round(rect.height)
        }));
    });
})();
