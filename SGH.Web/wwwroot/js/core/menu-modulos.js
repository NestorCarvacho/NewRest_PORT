export function inicializarMenuModulos(dependencias) {
    var resultados = document.getElementById("buscar-resultados");
    var lateral = document.getElementById("menu-modulos-lateral");
    var veloMenu = document.getElementById("velo-menu-modulos");
    var hamburguesa = document.getElementById("menu-hamburguesa");
    var cerrarLateralBtn = document.getElementById("menu-modulos-lateral-cerrar");

    function esSolapaEnEscritorio(modulo) {
        if (!modulo) return false;
        if (typeof dependencias.esModuloSolapa === "function") return dependencias.esModuloSolapa(modulo);
        return false;
    }

    function activarEnlace(evento, boton, modulo) {
        if (boton.tagName === "A" && (evento.ctrlKey || evento.metaKey || evento.shiftKey || evento.altKey)) return;
        if (modulo === "censoQr") {
            evento.preventDefault();
            if (typeof dependencias.abrirModulo === "function") dependencias.abrirModulo("misCensos");
            dependencias.abrirCensoQr();
            return;
        }
        if (boton.tagName === "A" && !esSolapaEnEscritorio(modulo)) {
            return;
        }
        evento.preventDefault();
        dependencias.abrirModulo(modulo);
    }

    function normalizar(texto) {
        return texto.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    }

    function cerrarMenu(item) {
        item.classList.remove("abierto");
        item.querySelector(".menu-boton").setAttribute("aria-expanded", "false");
    }

    function cerrarMenuLateral() {
        if (!lateral) return;
        lateral.classList.remove("abierto");
        lateral.hidden = true;
        if (veloMenu) veloMenu.hidden = true;
        if (hamburguesa) hamburguesa.setAttribute("aria-expanded", "false");
        document.body.classList.remove("menu-lateral-abierto");
    }

    function abrirMenuLateral() {
        if (!lateral || !esMenuCompacto()) return;
        cerrarMenusModulo();
        var menuUsuario = document.querySelector(".usuario-menu");
        var usuario = document.querySelector(".usuario-boton");
        if (menuUsuario) menuUsuario.hidden = true;
        if (usuario) usuario.setAttribute("aria-expanded", "false");
        lateral.hidden = false;
        window.requestAnimationFrame(function () {
            lateral.classList.add("abierto");
        });
        if (veloMenu) veloMenu.hidden = false;
        if (hamburguesa) hamburguesa.setAttribute("aria-expanded", "true");
        document.body.classList.add("menu-lateral-abierto");
    }

    function cerrarMenus() {
        cerrarMenusModulo();
        cerrarMenuLateral();
        var menuUsuario = document.querySelector(".usuario-menu");
        var usuario = document.querySelector(".usuario-boton");
        if (menuUsuario) menuUsuario.hidden = true;
        if (usuario) usuario.setAttribute("aria-expanded", "false");
    }

    function cerrarMenusModulo() {
        document.querySelectorAll(".menu-item.abierto").forEach(cerrarMenu);
        if (resultados) resultados.hidden = true;
    }

    function abrirMenu(item) {
        cerrarMenusModulo();
        var menuUsuario = document.querySelector(".usuario-menu");
        var usuario = document.querySelector(".usuario-boton");
        if (menuUsuario) menuUsuario.hidden = true;
        if (usuario) usuario.setAttribute("aria-expanded", "false");
        item.classList.add("abierto");
        item.querySelector(".menu-boton").setAttribute("aria-expanded", "true");
    }

    function esMenuCompacto() {
        return window.matchMedia("(max-width: 768px)").matches;
    }

    if (hamburguesa) {
        hamburguesa.addEventListener("click", function (evento) {
            if (!esMenuCompacto()) return;
            evento.preventDefault();
            evento.stopPropagation();
            if (lateral && lateral.classList.contains("abierto")) cerrarMenuLateral();
            else abrirMenuLateral();
        });
    }

    if (cerrarLateralBtn) {
        cerrarLateralBtn.addEventListener("click", function (evento) {
            evento.preventDefault();
            cerrarMenuLateral();
        });
    }

    if (veloMenu) {
        veloMenu.addEventListener("click", function () {
            cerrarMenuLateral();
        });
    }

    window.matchMedia("(max-width: 768px)").addEventListener("change", function (consulta) {
        if (!consulta.matches) cerrarMenuLateral();
    });

    document.querySelectorAll(".menu-app-escritorio .menu-item > .menu-boton").forEach(function (boton) {
        boton.addEventListener("click", function (evento) {
            if (!esMenuCompacto()) return;
            evento.preventDefault();
            evento.stopPropagation();
            var item = boton.closest(".menu-item");
            if (!item) return;
            if (item.classList.contains("abierto")) cerrarMenu(item);
            else abrirMenu(item);
        });
    });

    document.addEventListener("click", function (evento) {
        if (!esMenuCompacto()) return;
        if (evento.target.closest(".menu-hamburguesa")) return;
        if (evento.target.closest("#menu-modulos-lateral")) return;
        if (evento.target.closest(".menu-app-escritorio .menu-item")) return;
        if (evento.target.closest(".usuario")) return;
        if (evento.target.closest(".menu-desplegable")) return;
        cerrarMenusModulo();
    });

    var enlacesModulo = ".menu-app-escritorio .menu-columna [data-modulo], #menu-modulos-lateral [data-modulo]";
    document.querySelectorAll(enlacesModulo).forEach(function (boton) {
        boton.addEventListener("click", function (evento) {
            var modulo = boton.getAttribute("data-modulo");
            activarEnlace(evento, boton, modulo);
            cerrarMenus();
        });
    });

    document.querySelectorAll(".menu-app-escritorio .menu-item").forEach(function (item) {
        var temporizador;
        item.addEventListener("mouseenter", function () {
            if (esMenuCompacto()) return;
            window.clearTimeout(temporizador);
            abrirMenu(item);
        });
        item.addEventListener("mouseleave", function () {
            if (esMenuCompacto()) return;
            temporizador = window.setTimeout(function () { cerrarMenu(item); }, 180);
        });
    });

    var buscador = document.getElementById("buscar-modulo");
    if (buscador) {
        buscador.addEventListener("input", function () {
            var texto = buscador.value.trim().toLowerCase();
            resultados.replaceChildren();
            if (!texto) {
                resultados.hidden = true;
                return;
            }
            var consulta = normalizar(texto);
            document.querySelectorAll(enlacesModulo).forEach(function (origen) {
                if (normalizar(origen.textContent).indexOf(consulta) < 0) return;
                var item = document.createElement("li");
                var boton = document.createElement("button");
                boton.type = "button";
                boton.textContent = origen.textContent;
                boton.addEventListener("click", function (evento) {
                    buscador.value = "";
                    resultados.hidden = true;
                    var id = origen.getAttribute("data-modulo");
                    if (origen.tagName === "A" && !esSolapaEnEscritorio(id)) {
                        window.location.href = origen.href;
                    } else if (id) {
                        var eventoFalso = { preventDefault: function () {} };
                        activarEnlace(eventoFalso, origen, id);
                    }
                    cerrarMenus();
                });
                item.append(boton);
                resultados.append(item);
            });
            resultados.hidden = resultados.childElementCount === 0;
        });
    }

    return { cerrarMenus: cerrarMenus, cerrarMenuLateral: cerrarMenuLateral };
}