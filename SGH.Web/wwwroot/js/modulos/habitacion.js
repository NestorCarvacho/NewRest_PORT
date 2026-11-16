import { crearApi } from "../core/api.js";

var mid = "/Habitacion/Mid/MidHabitaciones.cs";

function plantilla() {
    return (
        '<div class="panel-modulo habitacion-solapa" data-hab="raiz">' +
        '<header class="panel-modulo__cabecera">' +
        '<nav class="pestanas-modulo" role="tablist">' +
        '<button type="button" class="pestanas-modulo__activa" data-hab="tab-habitaciones" data-tab="habitaciones" role="tab">Habitaciones</button>' +
        '<button type="button" data-hab="tab-banos" data-tab="banos" role="tab">Baños</button>' +
        "</nav>" +
        '<button type="button" data-hab="btn-nueva-habitacion" class="btn primario">Nueva habitación</button>' +
        '<button type="button" data-hab="btn-nuevo-bano" class="btn primario" hidden>Nuevo baño</button>' +
        "</header>" +
        '<div data-hab="aviso" class="aviso" hidden></div>' +
        '<div data-hab="panel-habitaciones" class="panel-tab">' +
        '<table data-hab="tabla-habitaciones" class="tabla-modulo"><thead><tr>' +
        "<th>Acciones</th><th>Código</th><th>Sexo</th><th>Baño</th><th>Pabellón</th><th>Módulo</th>" +
        "<th>Camas</th><th>Hab./total</th><th>Por turnos</th><th>Exclusivo</th><th>Estado</th>" +
        "</tr></thead><tbody></tbody></table></div>" +
        '<div data-hab="panel-banos" class="panel-tab" hidden>' +
        '<table data-hab="tabla-banos" class="tabla-modulo"><thead><tr>' +
        "<th>Acciones</th><th>Código</th><th>Ubicación</th><th>Sexo</th><th>Estado</th><th>Habitaciones</th>" +
        "</tr></thead><tbody></tbody></table></div>" +
        '<div data-hab="modal-habitacion" class="modal" hidden aria-hidden="true">' +
        '<form data-hab="form-habitacion" class="formulario-modulo">' +
        '<h2 data-hab="titulo-form-habitacion">Habitación</h2>' +
        '<input type="hidden" data-hab="hdn-habitacion-id" value="0" />' +
        '<input type="hidden" data-hab="hdn-opcion" value="crear" />' +
        '<label>Pabellón<select data-hab="sel-pabellon" required></select></label>' +
        '<label>Módulo<select data-hab="sel-modulo" required></select></label>' +
        '<label>Código / nombre<input data-hab="txt-nombre" required maxlength="100" /></label>' +
        '<label>Sexo<select data-hab="sel-sexo" required></select></label>' +
        '<label>Baño<select data-hab="sel-bano"><option value="">— Sin asignar —</option></select></label>' +
        '<label>N° camas<input data-hab="txt-camas" type="number" min="1" value="1" required /></label>' +
        '<fieldset data-hab="fs-camas" hidden><legend>Camas</legend><ul data-hab="lista-camas"></ul></fieldset>' +
        '<label>Estado<select data-hab="sel-estado"></select></label>' +
        '<label class="fila-check"><input data-hab="chk-reservable" type="checkbox" checked /> Reservable</label>' +
        '<label class="fila-check"><input data-hab="chk-turnos" type="checkbox" /> Uso por turnos</label>' +
        '<label class="fila-check"><input data-hab="chk-exclusivo" type="checkbox" /> Uso exclusivo</label>' +
        '<label>Minutos higienización<input data-hab="txt-minutos" type="number" min="0" value="0" /></label>' +
        '<div class="acciones-formulario">' +
        '<button type="submit" class="btn primario">Guardar</button>' +
        '<button type="button" data-hab="btn-cerrar-modal" class="btn">Cancelar</button></div></form></div>' +
        '<div data-hab="modal-bano" class="modal" hidden aria-hidden="true">' +
        '<form data-hab="form-bano" class="formulario-modulo">' +
        '<h2 data-hab="titulo-form-bano">Baño</h2>' +
        '<input type="hidden" data-hab="hdn-bano-id" value="0" />' +
        '<input type="hidden" data-hab="hdn-opcion-bano" value="crearbano" />' +
        '<label>Código<input data-hab="txt-bano-codigo" required maxlength="100" /></label>' +
        '<label>Ubicación<input data-hab="txt-bano-ubicacion" required maxlength="200" /></label>' +
        '<label>Tipo sexo<select data-hab="sel-bano-sexo" required>' +
        '<option value="MIXTO">Mixto</option><option value="HOMBRE">Hombre</option><option value="MUJER">Mujer</option>' +
        '<option value="M">M</option><option value="F">F</option></select></label>' +
        '<label>Estado<select data-hab="sel-bano-estado"><option value="ACTIVO">Activo</option><option value="INACTIVO">Inactivo</option></select></label>' +
        '<div class="acciones-formulario"><button type="submit" class="btn primario">Guardar</button>' +
        '<button type="button" data-hab="btn-cerrar-modal-bano" class="btn">Cancelar</button></div></form></div>' +
        "</div>"
    );
}

function q(raiz, sel) {
    return raiz.querySelector('[data-hab="' + sel + '"]');
}

export function crearHabitacion() {
    var api = crearApi();

    function montar(contenedor, opciones) {
        contenedor.innerHTML = plantilla();
        var raiz = q(contenedor, "raiz");
        if (!raiz) return;

        var catalogo = null;
        var filas = [];
        var banos = [];
        var tabInicial = (opciones && opciones.tabInicial) || "habitaciones";

        function aviso(texto, ok) {
            var n = q(raiz, "aviso");
            if (!n) return;
            n.hidden = !texto;
            n.textContent = texto || "";
            n.className = "aviso " + (ok ? "exito" : "error");
        }

        function llenarSelect(sel, items, valor, etiqueta, filtroPadre, opcionVacia) {
            sel.innerHTML = "";
            if (opcionVacia) {
                var vac = document.createElement("option");
                vac.value = "";
                vac.textContent = opcionVacia;
                sel.appendChild(vac);
            }
            var lista = filtroPadre ? items.filter(function (i) { return i.padreId === filtroPadre; }) : items;
            lista.forEach(function (i) {
                var o = document.createElement("option");
                o.value = valor(i);
                o.textContent = etiqueta(i);
                sel.appendChild(o);
            });
        }

        function cambiarTab(tab) {
            q(raiz, "panel-habitaciones").hidden = tab !== "habitaciones";
            q(raiz, "panel-banos").hidden = tab !== "banos";
            q(raiz, "btn-nueva-habitacion").hidden = tab !== "habitaciones";
            q(raiz, "btn-nuevo-bano").hidden = tab !== "banos";
            raiz.querySelectorAll(".pestanas-modulo button").forEach(function (b) {
                b.classList.toggle("pestanas-modulo__activa", b.getAttribute("data-tab") === tab);
            });
            if (tab === "banos") cargarBanos();
        }

        function pintarTabla() {
            var cuerpo = q(raiz, "tabla-habitaciones").querySelector("tbody");
            cuerpo.innerHTML = "";
            filas.forEach(function (f) {
                var tr = document.createElement("tr");
                tr.innerHTML =
                    '<td><button type="button" class="btn-tabla btn-editar" data-id="' + f.id + '">Editar</button> ' +
                    (f.isActive ? '<button type="button" class="btn-tabla btn-inactivar" data-id="' + f.id + '">Inactivar</button>' : "") + "</td>" +
                    "<td>" + (f.nombre || "") + "</td><td>" + (f.sexo || "") + "</td><td>" + (f.banoNombre || "—") + "</td>" +
                    "<td>" + (f.pabellonNombre || "") + "</td><td>" + (f.moduloNombre || "") + "</td>" +
                    "<td>" + f.cantidadCamas + "</td><td>" + f.camasHabilitadas + " / " + f.cantidadCamas + "</td>" +
                    "<td>" + (f.permiteUsoPorTurnos ? "Sí" : "No") + "</td><td>" + (f.permiteUsoExclusivo ? "Sí" : "No") + "</td>" +
                    "<td>" + (f.estado || "") + "</td>";
                cuerpo.appendChild(tr);
            });
            cuerpo.querySelectorAll(".btn-editar").forEach(function (b) {
                b.addEventListener("click", function () {
                    var id = Number(b.getAttribute("data-id"));
                    var fila = filas.find(function (x) { return x.id === id; });
                    if (fila) abrirModal("editar", fila);
                });
            });
            cuerpo.querySelectorAll(".btn-inactivar").forEach(function (b) {
                b.addEventListener("click", function () {
                    api.enviar(mid, { opcion: "inactivar", id: Number(b.getAttribute("data-id")) }).then(function () {
                        aviso("Habitación inactivada.", true);
                        cargarListado();
                    }).catch(function (e) { aviso(e.message, false); });
                });
            });
        }

        function pintarTablaBanos() {
            var cuerpo = q(raiz, "tabla-banos").querySelector("tbody");
            cuerpo.innerHTML = "";
            banos.forEach(function (b) {
                var tr = document.createElement("tr");
                tr.innerHTML =
                    '<td><button type="button" class="btn-editar-bano" data-id="' + b.id + '">Editar</button> ' +
                    (b.isActive ? '<button type="button" class="btn-inactivar-bano" data-id="' + b.id + '">Inactivar</button>' : "") + "</td>" +
                    "<td>" + (b.codigoNombre || "") + "</td><td>" + (b.ubicacion || "") + "</td><td>" + (b.tipoSexo || "") + "</td>" +
                    "<td>" + (b.estado || "") + "</td><td>" + (b.habitacionesVinculadas || 0) + "</td>";
                cuerpo.appendChild(tr);
            });
            cuerpo.querySelectorAll(".btn-editar-bano").forEach(function (btn) {
                btn.addEventListener("click", function () {
                    var b = banos.find(function (x) { return x.id === Number(btn.getAttribute("data-id")); });
                    if (b) abrirModalBano("editarbano", b);
                });
            });
            cuerpo.querySelectorAll(".btn-inactivar-bano").forEach(function (btn) {
                btn.addEventListener("click", function () {
                    api.enviar(mid, { opcion: "inactivarbano", id: Number(btn.getAttribute("data-id")) }).then(function () {
                        aviso("Baño inactivado.", true);
                        cargarBanos();
                        cargarCatalogo();
                    }).catch(function (e) { aviso(e.message, false); });
                });
            });
        }

        function abrirModal(opcion, fila) {
            q(raiz, "hdn-opcion").value = opcion;
            var hid = fila ? fila.id : 0;
            q(raiz, "hdn-habitacion-id").value = String(hid);
            q(raiz, "txt-nombre").value = fila ? fila.nombre : "";
            q(raiz, "txt-camas").value = fila ? fila.cantidadCamas : 1;
            q(raiz, "chk-reservable").checked = fila ? fila.isReservable : true;
            q(raiz, "chk-turnos").checked = fila ? fila.permiteUsoPorTurnos : false;
            q(raiz, "chk-exclusivo").checked = fila ? fila.permiteUsoExclusivo : false;
            q(raiz, "modal-habitacion").hidden = false;
            if (catalogo && fila) {
                llenarSelect(q(raiz, "sel-pabellon"), catalogo.pabellones, function (x) { return x.id; }, function (x) { return x.etiqueta; });
                q(raiz, "sel-pabellon").value = String(fila.pabellonId);
                llenarSelect(q(raiz, "sel-modulo"), catalogo.modulos, function (x) { return x.id; }, function (x) { return x.etiqueta; }, fila.pabellonId);
                q(raiz, "sel-modulo").value = String(fila.moduloId);
            }
            if (fila && fila.sexo) q(raiz, "sel-sexo").value = fila.sexo;
            if (fila && fila.estado) q(raiz, "sel-estado").value = fila.estado;
            if (opcion === "editar" && hid) {
                api.enviar(mid, { opcion: "obtener", id: hid }).then(function (det) {
                    q(raiz, "txt-minutos").value = det.tiempoHigienizacionMin || det.minutosMin || 0;
                    if (det.banoId) q(raiz, "sel-bano").value = String(det.banoId);
                });
            }
        }

        function abrirModalBano(opcion, fila) {
            q(raiz, "hdn-opcion-bano").value = opcion;
            q(raiz, "hdn-bano-id").value = fila ? String(fila.id) : "0";
            q(raiz, "txt-bano-codigo").value = fila ? fila.codigoNombre : "";
            q(raiz, "txt-bano-ubicacion").value = fila ? fila.ubicacion : "";
            q(raiz, "sel-bano-sexo").value = fila ? fila.tipoSexo : "MIXTO";
            q(raiz, "sel-bano-estado").value = fila ? fila.estado : "ACTIVO";
            q(raiz, "modal-bano").hidden = false;
        }

        function cargarCatalogo() {
            return api.enviar(mid, { opcion: "catalogo" }).then(function (datos) {
                catalogo = datos;
                llenarSelect(q(raiz, "sel-pabellon"), datos.pabellones, function (x) { return x.id; }, function (x) { return x.etiqueta; });
                llenarSelect(q(raiz, "sel-sexo"), datos.sexos, function (x) { return x.codigo; }, function (x) { return x.etiqueta; });
                llenarSelect(q(raiz, "sel-estado"), datos.estadosHabitacion, function (x) { return x.codigo; }, function (x) { return x.etiqueta; });
                llenarSelect(q(raiz, "sel-bano"), datos.banos || [], function (x) { return x.id; }, function (x) { return x.etiqueta; }, null, "— Sin asignar —");
            });
        }

        function cargarListado() {
            return api.enviar(mid, { opcion: "listar", soloActivas: 0 }).then(function (datos) {
                filas = datos.filas || [];
                pintarTabla();
            });
        }

        function cargarBanos() {
            return api.enviar(mid, { opcion: "listarbanos", soloActivos: 0 }).then(function (datos) {
                banos = datos.filas || [];
                pintarTablaBanos();
            });
        }

        raiz.querySelectorAll(".pestanas-modulo button").forEach(function (b) {
            b.addEventListener("click", function () { cambiarTab(b.getAttribute("data-tab")); });
        });
        q(raiz, "btn-nueva-habitacion").addEventListener("click", function () { abrirModal("crear", null); });
        q(raiz, "btn-nuevo-bano").addEventListener("click", function () { abrirModalBano("crearbano", null); });
        q(raiz, "btn-cerrar-modal").addEventListener("click", function () { q(raiz, "modal-habitacion").hidden = true; });
        q(raiz, "btn-cerrar-modal-bano").addEventListener("click", function () { q(raiz, "modal-bano").hidden = true; });
        q(raiz, "form-habitacion").addEventListener("submit", function (ev) {
            ev.preventDefault();
            var banoVal = q(raiz, "sel-bano").value;
            api.enviar(mid, {
                opcion: q(raiz, "hdn-opcion").value,
                id: Number(q(raiz, "hdn-habitacion-id").value),
                moduloId: Number(q(raiz, "sel-modulo").value),
                nombre: q(raiz, "txt-nombre").value.trim(),
                sexo: q(raiz, "sel-sexo").value,
                banoId: banoVal ? Number(banoVal) : null,
                cantidadCamas: Number(q(raiz, "txt-camas").value),
                estado: q(raiz, "sel-estado").value,
                isReservable: q(raiz, "chk-reservable").checked,
                usoPorTurnos: q(raiz, "chk-turnos").checked,
                usoExclusivo: q(raiz, "chk-exclusivo").checked,
                minutosMin: Number(q(raiz, "txt-minutos").value) || 0,
                tiempoHigienizacionMin: Number(q(raiz, "txt-minutos").value) || 0
            }).then(function () {
                aviso("Guardado correctamente.", true);
                q(raiz, "modal-habitacion").hidden = true;
                cargarListado();
                cargarCatalogo();
            }).catch(function (e) { aviso(e.message, false); });
        });
        q(raiz, "form-bano").addEventListener("submit", function (ev) {
            ev.preventDefault();
            api.enviar(mid, {
                opcion: q(raiz, "hdn-opcion-bano").value,
                id: Number(q(raiz, "hdn-bano-id").value),
                codigoNombre: q(raiz, "txt-bano-codigo").value.trim(),
                ubicacion: q(raiz, "txt-bano-ubicacion").value.trim(),
                tipoSexo: q(raiz, "sel-bano-sexo").value,
                estado: q(raiz, "sel-bano-estado").value
            }).then(function () {
                aviso("Baño guardado.", true);
                q(raiz, "modal-bano").hidden = true;
                cargarBanos();
                cargarCatalogo();
            }).catch(function (e) { aviso(e.message, false); });
        });

        if (tabInicial === "banos") cambiarTab("banos");
        else cambiarTab("habitaciones");
        return cargarCatalogo().then(cargarListado).catch(function (e) { aviso(e.message, false); });
    }

    return { montar: montar };
}