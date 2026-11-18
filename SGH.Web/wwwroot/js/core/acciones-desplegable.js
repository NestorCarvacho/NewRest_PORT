var cierreDesplegableEnlazado = false;

/** Agrupa botones de fila en un desplegable para no invadir columnas vecinas. */
export function crearAccionesDesplegable(botonFactory, etiquetas) {
    var envoltorio = document.createElement("div");
    envoltorio.className = "acciones-desplegable";
    var dispara = botonFactory("Acciones", "accion-fila acciones-desplegable-dispara", function (evento) {
        evento.preventDefault();
        evento.stopPropagation();
        var abierto = envoltorio.classList.toggle("abierto");
        dispara.setAttribute("aria-expanded", abierto ? "true" : "false");
    });
    dispara.setAttribute("aria-haspopup", "menu");
    dispara.setAttribute("aria-expanded", "false");
    var panel = document.createElement("div");
    panel.className = "acciones-desplegable-panel";
    panel.setAttribute("role", "menu");
    etiquetas.forEach(function (par) {
        var item = botonFactory(par[0], "acciones-desplegable-opcion " + (par[2] || ""), par[1]);
        item.setAttribute("role", "menuitem");
        item.type = "button";
        panel.append(item);
    });
    envoltorio.append(dispara, panel);
    if (!cierreDesplegableEnlazado) {
        cierreDesplegableEnlazado = true;
        document.addEventListener("click", function (evento) {
            document.querySelectorAll(".acciones-desplegable.abierto").forEach(function (menu) {
                if (menu.contains(evento.target)) return;
                menu.classList.remove("abierto");
                var boton = menu.querySelector(".acciones-desplegable-dispara");
                if (boton) boton.setAttribute("aria-expanded", "false");
            });
        });
    }
    return envoltorio;
}