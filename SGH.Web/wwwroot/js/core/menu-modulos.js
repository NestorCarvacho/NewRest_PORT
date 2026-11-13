export function inicializarMenuModulos(dependencias) {
    var resultados = document.getElementById("buscar-resultados");

    function normalizar(texto) {
        return texto.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    }

    function cerrarMenu(item) {
        item.classList.remove("abierto");
        item.querySelector(".menu-boton").setAttribute("aria-expanded", "false");
    }

    function cerrarMenus() {
        document.querySelectorAll(".menu-item.abierto").forEach(cerrarMenu);
        var menuUsuario = document.querySelector(".usuario-menu");
        var usuario = document.querySelector(".usuario-boton");
        if (menuUsuario) menuUsuario.hidden = true;
        if (usuario) usuario.setAttribute("aria-expanded", "false");
        if (resultados) resultados.hidden = true;
    }

    function abrirMenu(item) {
        cerrarMenus();
        item.classList.add("abierto");
        item.querySelector(".menu-boton").setAttribute("aria-expanded", "true");
    }

    document.querySelectorAll(".menu-columna [data-modulo]").forEach(function (boton) {
        boton.addEventListener("click", function (evento) {
            if (boton.tagName === "A" && (evento.ctrlKey || evento.metaKey || evento.shiftKey || evento.altKey)) return;
            evento.preventDefault();
            var modulo = boton.getAttribute("data-modulo");
            if (modulo === "censoQr") dependencias.abrirCensoQr();
            else dependencias.abrirModulo(modulo);
            cerrarMenus();
        });
    });

    document.querySelectorAll(".menu-item").forEach(function (item) {
        var temporizador;
        item.addEventListener("mouseenter", function () {
            window.clearTimeout(temporizador);
            abrirMenu(item);
        });
        item.addEventListener("mouseleave", function () {
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
                boton.addEventListener("click", function () {
                    buscador.value = "";
                    resultados.hidden = true;
                    var id = origen.getAttribute("data-modulo");
                    if (id === "censoQr") dependencias.abrirCensoQr();
                    else if (id) dependencias.abrirModulo(id);
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
