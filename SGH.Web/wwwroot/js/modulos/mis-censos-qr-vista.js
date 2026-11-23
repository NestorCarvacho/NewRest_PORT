import { listarPendientesCensoQr, nombreEventoPendientesCensoQr } from "../core/colaCensoQr.js";

function turnoLegible(valor) {
    if (!valor) return "";
    var t = String(valor).trim();
    if (t === "D" || /d[ií]a/i.test(t)) return "Día";
    if (t === "N" || /noche/i.test(t)) return "Noche";
    if (t === "A" || /ambos/i.test(t)) return "Ambos";
    return t;
}

function formatearFechaHora(iso) {
    if (!iso) return "";
    var d = new Date(iso);
    if (Number.isNaN(d.getTime())) return "";
    var dia = String(d.getDate()).padStart(2, "0");
    var mes = String(d.getMonth() + 1).padStart(2, "0");
    var anio = d.getFullYear();
    var hora = String(d.getHours()).padStart(2, "0");
    var min = String(d.getMinutes()).padStart(2, "0");
    return dia + "/" + mes + "/" + anio + " " + hora + ":" + min;
}

function crearTarjeta(opciones) {
    var li = document.createElement("li");
    li.className = "censo-qr-tarjeta" + (opciones.pendiente ? " censo-qr-tarjeta-pendiente" : "");
    var badge = document.createElement("div");
    badge.className = "censo-qr-tarjeta-badge";
    badge.textContent = String(opciones.camas != null ? opciones.camas : "0");
    var cuerpo = document.createElement("div");
    cuerpo.className = "censo-qr-tarjeta-cuerpo";
    var titulo = document.createElement("span");
    titulo.className = "censo-qr-tarjeta-titulo";
    titulo.textContent = opciones.titulo;
    var subt = document.createElement("p");
    subt.className = "censo-qr-tarjeta-subtitulo";
    subt.textContent = opciones.subtitulo || "";
    cuerpo.append(titulo, subt);
    li.append(badge, cuerpo);
    return li;
}

function normalizarFilas(resp) {
    if (!resp) return [];
    if (Array.isArray(resp)) return resp;
    if (Array.isArray(resp.filas)) return resp.filas;
    if (resp.registros && Array.isArray(resp.registros.filas)) return resp.registros.filas;
    return [];
}

export function montarVistaMisCensosQr(dependencias, nodo) {
    var raiz = nodo.querySelector(".censo-qr-vista");
    if (!raiz || raiz._enlazado) return;
    raiz._enlazado = true;

    var listaPendientes = raiz.querySelector("[data-lista='pendientes']");
    var listaRegistrados = raiz.querySelector("[data-lista='registrados']");
    var inputFecha = raiz.querySelector("[name='fechaCenso']");
    var fab = raiz.querySelector("[data-accion='nuevo-censo-qr']");
    var btnSync = raiz.querySelector("[data-accion='sincronizar-censo-qr']");

    function tituloBase(meta, cuerpo) {
        var tituloPartes = [];
        if (meta.creadoEn) tituloPartes.push(formatearFechaHora(meta.creadoEn));
        if (meta.habitacion) tituloPartes.push(meta.habitacion);
        return tituloPartes;
    }

    function pintarPendientes() {
        if (!listaPendientes) return;
        listaPendientes.replaceChildren();
        var items = listarPendientesCensoQr();
        if (btnSync) {
            btnSync.hidden = items.length === 0;
        }
        if (!items.length) {
            var vacio = document.createElement("div");
            vacio.className = "censo-qr-sin-resultados";
            vacio.textContent = "Sin resultados";
            listaPendientes.append(vacio);
            return;
        }
        items.forEach(function (item) {
            var meta = item.meta || {};
            var cuerpo = item.cuerpo || {};
            var subt = [meta.modulo, meta.piso].filter(Boolean).join(" | ");
            var turno = cuerpo.turno || meta.turno || "D";

            if (turno === "A") {
                var camasDia = meta.camasOcupadasDia != null ? meta.camasOcupadasDia : (cuerpo.cantidad != null ? cuerpo.cantidad : "0");
                var camasNoche = meta.camasOcupadasNoche != null ? meta.camasOcupadasNoche : (cuerpo.cantidadNoche != null ? cuerpo.cantidadNoche : "0");
                ["Día", "Noche"].forEach(function (etiqueta, indice) {
                    var partes = tituloBase(meta, cuerpo).slice();
                    partes.push("Turno: " + etiqueta);
                    listaPendientes.append(crearTarjeta({
                        pendiente: true,
                        camas: indice === 0 ? camasDia : camasNoche,
                        titulo: partes.join(" | "),
                        subtitulo: subt
                    }));
                });
                return;
            }

            var camas = cuerpo.turno === "N" ? cuerpo.cantidad : cuerpo.cantidad;
            if (meta.camasOcupadas != null) camas = meta.camasOcupadas;
            var partes = tituloBase(meta, cuerpo);
            partes.push("Turno: " + turnoLegible(meta.turnoNombre || turno));
            listaPendientes.append(crearTarjeta({
                pendiente: true,
                camas: camas,
                titulo: partes.join(" | "),
                subtitulo: subt
            }));
        });
    }

    function pintarRegistrados(filas) {
        if (!listaRegistrados) return;
        listaRegistrados.replaceChildren();
        if (!filas || !filas.length) {
            var vacio = document.createElement("div");
            vacio.className = "censo-qr-sin-resultados";
            vacio.textContent = "Sin resultados";
            listaRegistrados.append(vacio);
            return;
        }
        filas.forEach(function (fila) {
            var titulo = (fila.habitacion || "") + " | Turno: " + turnoLegible(fila.turnoNombre || fila.turno);
            var subt = [fila.modulo, fila.piso].filter(Boolean).join(" | ");
            listaRegistrados.append(crearTarjeta({
                pendiente: false,
                camas: fila.camasOcupadas,
                titulo: titulo,
                subtitulo: subt
            }));
        });
    }

    function cargarRegistrados() {
        if (!inputFecha || !dependencias.enviar) return Promise.resolve();
        var fecha = inputFecha.value;
        return dependencias.enviar("/Censo/Mid/MidCenso.cs", { opcion: "miscensos", fecha: fecha })
            .then(function (resp) {
                var filas = normalizarFilas(resp);
                pintarRegistrados(filas);
                nodo._filasMisCensosQr = filas;
            })
            .catch(function () {
                pintarRegistrados([]);
            });
    }

    function refrescar() {
        pintarPendientes();
        return cargarRegistrados();
    }

    if (inputFecha) {
        inputFecha.addEventListener("change", function () { cargarRegistrados(); });
    }
    if (fab && dependencias.abrirFlujoCensoQr) {
        fab.addEventListener("click", function () {
            dependencias.abrirFlujoCensoQr({ alCerrar: refrescar });
        });
    }
    if (btnSync && dependencias.sincronizar) {
        btnSync.addEventListener("click", function () {
            btnSync.disabled = true;
            dependencias.sincronizar()
                .then(function () { return refrescar(); })
                .catch(function () { })
                .finally(function () { btnSync.disabled = false; });
        });
    }

    window.addEventListener(nombreEventoPendientesCensoQr(), function () { refrescar(); });
    window.addEventListener("online", function () { setTimeout(refrescar, 500); });

    nodo._refrescarVistaCensoQr = refrescar;
    refrescar();
}