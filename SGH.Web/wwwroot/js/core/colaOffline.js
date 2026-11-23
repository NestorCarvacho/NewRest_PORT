var CLAVE_COLA = "sgh.offline.cola";
var CLAVE_CENSO = "sgh.censos.pendientes";
var enviarSync = null;

function leerJson(clave) {
    try {
        return JSON.parse(localStorage.getItem(clave) || "[]");
    } catch (_) {
        return [];
    }
}

function contarPendientes() {
    return leerJson(CLAVE_COLA).length + leerJson(CLAVE_CENSO).length;
}

export function encolarOperacion(entrada) {
    var pendientes = leerJson(CLAVE_COLA);
    pendientes.push(Object.assign({ id: Date.now() + "-" + Math.random().toString(16).slice(2) }, entrada));
    localStorage.setItem(CLAVE_COLA, JSON.stringify(pendientes));
    actualizarBannerOffline();
}

export async function sincronizarCola(enviar) {
    if (!navigator.onLine) return;
    var pendientes = leerJson(CLAVE_COLA);
    for (var i = 0; i < pendientes.length; i++) {
        var item = pendientes[i];
        try {
            await enviar(item.url, item.cuerpo);
            pendientes = leerJson(CLAVE_COLA).filter(function (x) { return x.id !== item.id; });
            localStorage.setItem(CLAVE_COLA, JSON.stringify(pendientes));
        } catch (_) {
            break;
        }
    }
    actualizarBannerOffline();
}

export async function sincronizarCensoLegado(enviar) {
    if (!navigator.onLine) return;
    var pendientes = leerJson(CLAVE_CENSO);
    for (var i = 0; i < pendientes.length; i++) {
        var item = pendientes[i];
        try {
            await enviar("/Censo/Mid/MidCenso.cs", Object.assign({ opcion: "guardarqr" }, item.payload));
            pendientes = leerJson(CLAVE_CENSO).filter(function (x) { return x.id !== item.id; });
            localStorage.setItem(CLAVE_CENSO, JSON.stringify(pendientes));
        } catch (_) {
            break;
        }
    }
    actualizarBannerOffline();
}

export async function sincronizarTodo(enviar) {
    await sincronizarCola(enviar);
    await sincronizarCensoLegado(enviar);
}

function dispararSincronizacion() {
    if (!enviarSync || !navigator.onLine) return;
    sincronizarTodo(enviarSync).catch(function () { });
}

export function actualizarBannerOffline() {
    var existente = document.getElementById("banner-offline");
    var sinRed = !navigator.onLine;
    var pendientes = contarPendientes();
    if (!sinRed && pendientes === 0) {
        if (existente) existente.remove();
        return;
    }
    if (!existente) {
        existente = document.createElement("div");
        existente.id = "banner-offline";
        existente.className = "banner-offline";
        existente.setAttribute("role", "status");
        document.body.prepend(existente);
    }
    existente.replaceChildren();
    var texto = document.createElement("span");
    texto.className = "banner-offline-texto";
    var partes = [];
    if (sinRed) partes.push("Sin conexión. Los cambios se guardarán localmente cuando aplique.");
    if (pendientes > 0) partes.push("Pendientes de sincronizar: " + pendientes + ".");
    texto.textContent = partes.join(" ");
    existente.append(texto);
    if (pendientes > 0 && !sinRed) {
        var boton = document.createElement("button");
        boton.type = "button";
        boton.className = "boton-secundario banner-offline-sync";
        boton.textContent = "Sincronizar datos";
        boton.addEventListener("click", dispararSincronizacion);
        existente.append(boton);
    }
}

export function instalarColaOffline(enviar) {
    enviarSync = enviar;
    actualizarBannerOffline();
    window.addEventListener("online", function () {
        sincronizarTodo(enviar).catch(function () { });
    });
    window.addEventListener("offline", actualizarBannerOffline);
    return {
        encolarOperacion: encolarOperacion,
        sincronizar: function () { return sincronizarTodo(enviar); }
    };
}