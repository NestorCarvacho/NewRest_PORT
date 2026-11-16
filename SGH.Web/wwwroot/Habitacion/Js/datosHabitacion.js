import { crearApi } from "../../js/core/api.js";

(function () {
    var api = crearApi();
    var mid = "/Habitacion/Mid/MidHabitaciones.cs";
    var catalogo = null;
    var filas = [];
    var banos = [];
    var tabActiva = "habitaciones";

    function aviso(texto, ok) {
        var n = document.getElementById("habitacionesAviso");
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
        var lista = filtroPadre
            ? items.filter(function (i) { return i.padreId === filtroPadre; })
            : items;
        lista.forEach(function (i) {
            var o = document.createElement("option");
            o.value = valor(i);
            o.textContent = etiqueta(i);
            sel.appendChild(o);
        });
    }

    function cambiarTab(tab) {
        tabActiva = tab;
        document.getElementById("panelHabitaciones").hidden = tab !== "habitaciones";
        document.getElementById("panelBanos").hidden = tab !== "banos";
        document.getElementById("btnNuevaHabitacion").hidden = tab !== "habitaciones";
        document.getElementById("btnNuevoBano").hidden = tab !== "banos";
        document.querySelectorAll(".pestanas-modulo button").forEach(function (b) {
            b.classList.toggle("pestanas-modulo__activa", b.getAttribute("data-tab") === tab);
        });
        if (tab === "banos") cargarBanos();
    }

    function cargarCamas(habitacionId) {
        var fs = document.getElementById("fsCamas");
        var ul = document.getElementById("listaCamas");
        ul.innerHTML = "";
        if (!habitacionId) {
            fs.hidden = true;
            return;
        }
        api.enviar(mid, { opcion: "listarcamas", habitacionId: habitacionId }).then(function (datos) {
            var camas = datos.filas || [];
            fs.hidden = camas.length === 0;
            camas.forEach(function (c) {
                var li = document.createElement("li");
                var chk = document.createElement("input");
                chk.type = "checkbox";
                chk.checked = c.habilitada;
                chk.dataset.camaId = String(c.id);
                chk.addEventListener("change", function () {
                    api.enviar(mid, {
                        opcion: "guardarcama",
                        id: c.id,
                        habitacionId: habitacionId,
                        codigoCama: c.codigoCama,
                        habilitada: chk.checked,
                        orden: c.orden
                    }).catch(function (e) { aviso(e.message, false); chk.checked = !chk.checked; });
                });
                li.appendChild(chk);
                li.appendChild(document.createTextNode(" " + c.codigoCama));
                ul.appendChild(li);
            });
        }).catch(function () { fs.hidden = true; });
    }

    function abrirModal(opcion, fila) {
        document.getElementById("hdnOpcion").value = opcion;
        var hid = fila ? fila.id : 0;
        document.getElementById("hdnHabitacionId").value = String(hid);
        document.getElementById("tituloFormHabitacion").textContent = opcion === "crear" ? "Nueva habitación" : "Editar habitación";
        document.getElementById("txtNombre").value = fila ? fila.nombre : "";
        document.getElementById("txtCantidadCamas").value = fila ? fila.cantidadCamas : 1;
        document.getElementById("chkReservable").checked = fila ? fila.isReservable : true;
        document.getElementById("chkUsoPorTurnos").checked = fila ? fila.permiteUsoPorTurnos : false;
        document.getElementById("chkUsoExclusivo").checked = fila ? fila.permiteUsoExclusivo : false;
        if (catalogo) {
            llenarSelect(document.getElementById("selBano"), catalogo.banos || [], function (x) { return x.id; }, function (x) { return x.etiqueta; }, null, "— Sin asignar —");
            if (fila && fila.banoId) document.getElementById("selBano").value = String(fila.banoId);
            else document.getElementById("selBano").value = "";
        }
        if (fila && catalogo) {
            var pabellonId = fila.pabellonId;
            llenarSelect(document.getElementById("selPabellon"), catalogo.pabellones, function (x) { return x.id; }, function (x) { return x.etiqueta; });
            document.getElementById("selPabellon").value = String(pabellonId);
            llenarSelect(document.getElementById("selModulo"), catalogo.modulos, function (x) { return x.id; }, function (x) { return x.etiqueta; }, pabellonId);
            document.getElementById("selModulo").value = String(fila.moduloId);
        }
        if (fila && fila.sexo) document.getElementById("selSexo").value = fila.sexo;
        if (fila && fila.estado) document.getElementById("selEstado").value = fila.estado;
        if (opcion === "editar" && hid) {
            api.enviar(mid, { opcion: "obtener", id: hid }).then(function (det) {
                document.getElementById("txtMinutosMin").value = det.tiempoHigienizacionMin || det.minutosMin || 0;
                if (det.banoId) document.getElementById("selBano").value = String(det.banoId);
                cargarCamas(hid);
            });
        } else {
            document.getElementById("txtMinutosMin").value = 0;
            document.getElementById("fsCamas").hidden = true;
        }
        document.getElementById("modalHabitacion").hidden = false;
    }

    function cerrarModal() {
        document.getElementById("modalHabitacion").hidden = true;
    }

    function pintarTabla() {
        var cuerpo = document.querySelector("#tablaHabitaciones tbody");
        cuerpo.innerHTML = "";
        filas.forEach(function (f) {
            var tr = document.createElement("tr");
            tr.innerHTML =
                '<td><button type="button" class="btn-tabla btn-editar" data-id="' + f.id + '">Editar</button> ' +
                (f.isActive ? '<button type="button" class="btn-tabla btn-inactivar" data-id="' + f.id + '">Inactivar</button>' : "") + '</td>' +
                "<td>" + (f.nombre || "") + "</td>" +
                "<td>" + (f.sexo || "") + "</td>" +
                "<td>" + (f.banoNombre || "—") + "</td>" +
                "<td>" + (f.pabellonNombre || "") + "</td>" +
                "<td>" + (f.moduloNombre || "") + "</td>" +
                "<td>" + f.cantidadCamas + "</td>" +
                "<td>" + f.camasHabilitadas + " / " + f.cantidadCamas + "</td>" +
                "<td>" + (f.permiteUsoPorTurnos ? "Sí" : "No") + "</td>" +
                "<td>" + (f.permiteUsoExclusivo ? "Sí" : "No") + "</td>" +
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
                var id = Number(b.getAttribute("data-id"));
                api.enviar(mid, { opcion: "inactivar", id: id }).then(function () {
                    aviso("Habitación inactivada.", true);
                    cargarListado();
                }).catch(function (e) { aviso(e.message, false); });
            });
        });
    }

    function pintarTablaBanos() {
        var cuerpo = document.querySelector("#tablaBanos tbody");
        cuerpo.innerHTML = "";
        banos.forEach(function (b) {
            var tr = document.createElement("tr");
            tr.innerHTML =
                '<td><button type="button" class="btn-tabla btn-editar-bano" data-id="' + b.id + '">Editar</button> ' +
                (b.isActive ? '<button type="button" class="btn-tabla btn-inactivar-bano" data-id="' + b.id + '">Inactivar</button>' : "") + '</td>' +
                "<td>" + (b.codigoNombre || "") + "</td>" +
                "<td>" + (b.ubicacion || "") + "</td>" +
                "<td>" + (b.tipoSexo || "") + "</td>" +
                "<td>" + (b.estado || "") + "</td>" +
                "<td>" + (b.habitacionesVinculadas || 0) + "</td>";
            cuerpo.appendChild(tr);
        });
        cuerpo.querySelectorAll(".btn-editar-bano").forEach(function (btn) {
            btn.addEventListener("click", function () {
                var id = Number(btn.getAttribute("data-id"));
                var b = banos.find(function (x) { return x.id === id; });
                if (b) abrirModalBano("editarbano", b);
            });
        });
        cuerpo.querySelectorAll(".btn-inactivar-bano").forEach(function (btn) {
            btn.addEventListener("click", function () {
                var id = Number(btn.getAttribute("data-id"));
                api.enviar(mid, { opcion: "inactivarbano", id: id }).then(function () {
                    aviso("Baño inactivado.", true);
                    cargarBanos();
                    cargarCatalogo();
                }).catch(function (e) { aviso(e.message, false); });
            });
        });
    }

    function abrirModalBano(opcion, fila) {
        document.getElementById("hdnOpcionBano").value = opcion;
        document.getElementById("hdnBanoId").value = fila ? String(fila.id) : "0";
        document.getElementById("tituloFormBano").textContent = opcion === "crearbano" ? "Nuevo baño" : "Editar baño";
        document.getElementById("txtBanoCodigo").value = fila ? fila.codigoNombre : "";
        document.getElementById("txtBanoUbicacion").value = fila ? fila.ubicacion : "";
        document.getElementById("selBanoSexo").value = fila ? fila.tipoSexo : "MIXTO";
        document.getElementById("selBanoEstado").value = fila ? fila.estado : "ACTIVO";
        document.getElementById("modalBano").hidden = false;
    }

    function cargarCatalogo() {
        return api.enviar(mid, { opcion: "catalogo" }).then(function (datos) {
            catalogo = datos;
            llenarSelect(document.getElementById("selPabellon"), datos.pabellones, function (x) { return x.id; }, function (x) { return x.etiqueta; });
            llenarSelect(document.getElementById("selSexo"), datos.sexos, function (x) { return x.codigo; }, function (x) { return x.etiqueta; });
            llenarSelect(document.getElementById("selEstado"), datos.estadosHabitacion, function (x) { return x.codigo; }, function (x) { return x.etiqueta; });
            llenarSelect(document.getElementById("selBano"), datos.banos || [], function (x) { return x.id; }, function (x) { return x.etiqueta; }, null, "— Sin asignar —");
            document.getElementById("selPabellon").addEventListener("change", function () {
                var pid = Number(document.getElementById("selPabellon").value);
                llenarSelect(document.getElementById("selModulo"), datos.modulos, function (x) { return x.id; }, function (x) { return x.etiqueta; }, pid);
            });
            var pid = Number(document.getElementById("selPabellon").value);
            if (pid) llenarSelect(document.getElementById("selModulo"), datos.modulos, function (x) { return x.id; }, function (x) { return x.etiqueta; }, pid);
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

    document.querySelectorAll(".pestanas-modulo button").forEach(function (b) {
        b.addEventListener("click", function () { cambiarTab(b.getAttribute("data-tab")); });
    });
    document.getElementById("btnNuevaHabitacion").addEventListener("click", function () {
        abrirModal("crear", null);
    });
    document.getElementById("btnNuevoBano").addEventListener("click", function () {
        abrirModalBano("crearbano", null);
    });
    document.getElementById("btnCerrarModal").addEventListener("click", cerrarModal);
    document.getElementById("btnCerrarModalBano").addEventListener("click", function () {
        document.getElementById("modalBano").hidden = true;
    });
    document.getElementById("formHabitacion").addEventListener("submit", function (ev) {
        ev.preventDefault();
        var opcion = document.getElementById("hdnOpcion").value;
        var banoVal = document.getElementById("selBano").value;
        var cuerpo = {
            opcion: opcion,
            id: Number(document.getElementById("hdnHabitacionId").value),
            moduloId: Number(document.getElementById("selModulo").value),
            nombre: document.getElementById("txtNombre").value.trim(),
            sexo: document.getElementById("selSexo").value,
            banoId: banoVal ? Number(banoVal) : null,
            cantidadCamas: Number(document.getElementById("txtCantidadCamas").value),
            estado: document.getElementById("selEstado").value,
            isReservable: document.getElementById("chkReservable").checked,
            usoPorTurnos: document.getElementById("chkUsoPorTurnos").checked,
            usoExclusivo: document.getElementById("chkUsoExclusivo").checked,
            minutosMin: Number(document.getElementById("txtMinutosMin").value) || 0,
            tiempoHigienizacionMin: Number(document.getElementById("txtMinutosMin").value) || 0
        };
        api.enviar(mid, cuerpo).then(function () {
            aviso("Guardado correctamente.", true);
            cerrarModal();
            cargarListado();
            cargarCatalogo();
        }).catch(function (e) { aviso(e.message, false); });
    });
    document.getElementById("formBano").addEventListener("submit", function (ev) {
        ev.preventDefault();
        var cuerpo = {
            opcion: document.getElementById("hdnOpcionBano").value,
            id: Number(document.getElementById("hdnBanoId").value),
            codigoNombre: document.getElementById("txtBanoCodigo").value.trim(),
            ubicacion: document.getElementById("txtBanoUbicacion").value.trim(),
            tipoSexo: document.getElementById("selBanoSexo").value,
            estado: document.getElementById("selBanoEstado").value
        };
        api.enviar(mid, cuerpo).then(function () {
            aviso("Baño guardado.", true);
            document.getElementById("modalBano").hidden = true;
            cargarBanos();
            cargarCatalogo();
        }).catch(function (e) { aviso(e.message, false); });
    });

    var app = document.getElementById("habitaciones-app");
    var tabInicial = app && app.getAttribute("data-tab-inicial");
    if (tabInicial === "banos") {
        cambiarTab("banos");
    }

    cargarCatalogo().then(cargarListado).catch(function (e) { aviso(e.message, false); });
})();