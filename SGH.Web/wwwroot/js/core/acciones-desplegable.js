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
    document.addEventListener("click", function (evento) {
        if (!envoltorio.classList.contains("abierto")) return;
        if (!envoltorio.contains(evento.target)) {
            envoltorio.classList.remove("abierto");
            dispara.setAttribute("aria-expanded", "false");
        }
    });
    return envoltorio;
}