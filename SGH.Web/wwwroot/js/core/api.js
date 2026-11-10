export function crearApi() {
    function token() {
        var campo = document.querySelector("#antifalsificacion input[name='__RequestVerificationToken']");
        return campo ? campo.value : "";
    }

    function pedir(url, opciones) {
        return fetch(url, opciones).then(function (respuesta) {
            return respuesta.text().then(function (texto) {
                var cuerpo = {};
                if (texto) {
                    try { cuerpo = JSON.parse(texto); } catch (error) { cuerpo = {}; }
                }
                if (!respuesta.ok) throw new Error(cuerpo.mensaje || "No se pudo completar la operación.");
                return cuerpo;
            });
        });
    }

    function enviar(url, cuerpo) {
        return pedir(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token()
            },
            body: JSON.stringify(cuerpo)
        });
    }

    function enviarArchivo(url, archivo) {
        var cuerpo = new FormData();
        cuerpo.append("archivo", archivo);
        return pedir(url, {
            method: "POST",
            headers: { "RequestVerificationToken": token() },
            body: cuerpo
        });
    }

    function consulta(parametros) {
        var partes = [];
        Object.keys(parametros).forEach(function (clave) {
            if (parametros[clave] === undefined || parametros[clave] === null || parametros[clave] === "") return;
            partes.push(encodeURIComponent(clave) + "=" + encodeURIComponent(parametros[clave]));
        });
        return partes.length ? "&" + partes.join("&") : "";
    }

    return { pedir: pedir, enviar: enviar, enviarArchivo: enviarArchivo, consulta: consulta };
}
