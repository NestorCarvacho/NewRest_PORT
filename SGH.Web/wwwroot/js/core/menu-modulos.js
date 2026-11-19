export function inicializarMenuModulos(dependencias) {
    var resultados = document.getElementById("buscar-resultados");

    function esSolapaEnEscritorio(modulo) {
        if (!modulo) return false;
        if (typeof dependencias.esModuloSolapa === "function") return dependencias.esModuloSolapa(modulo);
        return false;
    }

    function activarEnlace(evento, boton, modulo) {
        if (boton.tagName === "A" && (evento.ctrlKey || evento.metaKey || evento.shiftKey || evento.altKey)) return;
        if (modulo === "censoQr") {
            evento.preventDefault();
            dependencias.abrirCensoQr();
            return;
        }
        if (boton.tagName === "A" && !esSolapaEnEscritorio(modulo)) {
            /* Páginas Razor dedicadas (p. ej. Habitaciones, Baños, Usuarios) */
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

    function cerrarMenus() {
        cerrarMenusModulo();
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

    document.querySelectorAll(".menu-item > .menu-boton").forEach(function (boton) {
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
        if (evento.target.closest(".menu-item")) return;
        if (evento.target.closest(".usuario")) return;
        if (evento.target.closest(".menu-desplegable")) return;
        cerrarMenus();
    });

    document.querySelectorAll(".menu-columna [data-modulo]").forEach(function (boton) {
        boton.addEventListener("click", function (evento) {
            var modulo = boton.getAttribute("data-modulo");
            activarEnlace(evento, boton, modulo);
            cerrarMenus();
        });
    });

    document.querySelectorAll(".menu-item").forEach(function (item) {
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
            document.querySelectorAll(".menu-columna [data-modulo]").forEach(function (origen) {
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

    return { cerrarMenus: cerrarMenus };
}
