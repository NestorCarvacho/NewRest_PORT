import { crearApi } from "../../js/core/api.js";

(function () {
    var api = crearApi();
    var mid = "/Habitacion/Mid/MidHabitaciones.cs";
    var catalogo = null;
    var filas = [];

    function aviso(texto, ok) {
        var n = document.getElementById("habitacionesAviso");
        if (!n) return;
        n.hidden = !texto;
        n.textContent = texto || "";
        n.className = "aviso " + (ok ? "exito" : "error");
    }

    function llenarSelect(sel, items, valor, etiqueta, filtroPadre) {
        sel.innerHTML = "";
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

    function abrirModal(opcion, fila) {
        document.getElementById("hdnOpcion").value = opcion;
        document.getElementById("hdnHabitacionId").value = fila ? String(fila.id) : "0";
        document.getElementById("tituloFormHabitacion").textContent = opcion === "crear" ? "Nueva habitación" : "Editar habitación";
        document.getElementById("txtNombre").value = fila ? fila.nombre : "";
        document.getElementById("txtCantidadCamas").value = fila ? fila.cantidadCamas : 1;
        document.getElementById("chkReservable").checked = fila ? fila.isReservable : true;
        document.getElementById("chkUsoPorTurnos").checked = fila ? fila.permiteUsoPorTurnos : false;
        document.getElementById("chkUsoExclusivo").checked = fila ? fila.permiteUsoExclusivo : false;
        if (fila && catalogo) {
            var pabellonId = fila.pabellonId;
            llenarSelect(document.getElementById("selPabellon"), catalogo.pabellones, function (x) { return x.id; }, function (x) { return x.etiqueta; });
            document.getElementById("selPabellon").value = String(pabellonId);
            llenarSelect(document.getElementById("selModulo"), catalogo.modulos, function (x) { return x.id; }, function (x) { return x.etiqueta; }, pabellonId);
            document.getElementById("selModulo").value = String(fila.moduloId);
        }
        if (fila && fila.sexo) document.getElementById("selSexo").value = fila.sexo;
        if (fila && fila.estado) document.getElementById("selEstado").value = fila.estado;
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

    function cargarCatalogo() {
        return api.enviar(mid, { opcion: "catalogo" }).then(function (datos) {
            catalogo = datos;
            llenarSelect(document.getElementById("selPabellon"), datos.pabellones, function (x) { return x.id; }, function (x) { return x.etiqueta; });
            llenarSelect(document.getElementById("selSexo"), datos.sexos, function (x) { return x.codigo; }, function (x) { return x.etiqueta; });
            llenarSelect(document.getElementById("selEstado"), datos.estadosHabitacion, function (x) { return x.codigo; }, function (x) { return x.etiqueta; });
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

    document.getElementById("btnNuevaHabitacion").addEventListener("click", function () {
        abrirModal("crear", null);
    });
    document.getElementById("btnCerrarModal").addEventListener("click", cerrarModal);
    document.getElementById("formHabitacion").addEventListener("submit", function (ev) {
        ev.preventDefault();
        var opcion = document.getElementById("hdnOpcion").value;
        var cuerpo = {
            opcion: opcion,
            id: Number(document.getElementById("hdnHabitacionId").value),
            moduloId: Number(document.getElementById("selModulo").value),
            nombre: document.getElementById("txtNombre").value.trim(),
            sexo: document.getElementById("selSexo").value,
            cantidadCamas: Number(document.getElementById("txtCantidadCamas").value),
            estado: document.getElementById("selEstado").value,
            isReservable: document.getElementById("chkReservable").checked,
            usoPorTurnos: document.getElementById("chkUsoPorTurnos").checked,
            usoExclusivo: document.getElementById("chkUsoExclusivo").checked,
            minutosMin: Number(document.getElementById("txtMinutosMin").value) || 0
        };
        api.enviar(mid, cuerpo).then(function () {
            aviso("Guardado correctamente.", true);
            cerrarModal();
            cargarListado();
        }).catch(function (e) { aviso(e.message, false); });
    });

    cargarCatalogo().then(cargarListado).catch(function (e) { aviso(e.message, false); });
})();