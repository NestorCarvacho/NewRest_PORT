export function inicializarCuenta() {
    var usuario = document.querySelector(".usuario-boton");
    var menuUsuario = document.querySelector(".usuario-menu");
    if (!usuario || !menuUsuario) return;

    usuario.addEventListener("click", function (evento) {
        evento.stopPropagation();
        var abierto = menuUsuario.hidden;
        if (abierto) {
            document.querySelectorAll(".menu-item.abierto").forEach(function (item) {
                item.classList.remove("abierto");
                var boton = item.querySelector(".menu-boton");
                if (boton) boton.setAttribute("aria-expanded", "false");
            });
        }
        menuUsuario.hidden = !abierto;
        usuario.setAttribute("aria-expanded", abierto ? "true" : "false");
    });

    document.addEventListener("click", function (evento) {
        if (menuUsuario.hidden) return;
        if (evento.target.closest(".usuario")) return;
        menuUsuario.hidden = true;
        usuario.setAttribute("aria-expanded", "false");
    });
}
