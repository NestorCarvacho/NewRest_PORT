export function inicializarCuenta() {
    var usuario = document.querySelector(".usuario-boton");
    var menuUsuario = document.querySelector(".usuario-menu");
    if (!usuario || !menuUsuario) return;

    usuario.addEventListener("click", function () {
        var abierto = menuUsuario.hidden;
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
