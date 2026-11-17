export function crearSolicitud(d) {
    var ruta = "/Solicitud/Mid/MidSolicitudes.cs";
    function mid(opcion, datos) {
        return d.enviar(ruta, Object.assign({ opcion: opcion }, datos || {}));
    }
    function opciones(lista) {
        return (lista || []).map(function (x) { return { value: x.id, text: x.nombre }; });
    }
    function filtros() {
        var catalogo = d.obtenerCatalogo() || {};
        return [
            d.campo("desde", "Desde", "date", [], { requerido: true }),
            d.campo("hasta", "Hasta", "date", [], { requerido: true }),
            d.campo("grupoId", "Grupo empresa", "select", opciones(catalogo.grupos)),
            d.campo("solicitanteId", "Solicitante", "select", opciones(catalogo.solicitantes)),
            d.campo("estado", "Estado", "select", opciones(catalogo.estados)),
            d.campo("numero", "Número", "number")
        ];
    }
    function filtroInicial() {
        var rango = d.rangoInicial();
        return { desde: rango.reservaDesde, hasta: rango.reservaHasta };
    }
    function buscar(filtro) {
        return mid("listar", filtro);
    }
    function boton(texto, accion) {
        var el = document.createElement("button");
        el.type = "button";
        el.className = "boton-secundario";
        el.textContent = texto;
        el.addEventListener("click", accion);
        return el;
    }
    function campo(form, etiqueta, nombre, tipo, valor, requerido) {
        var label = document.createElement("label");
        label.className = tipo === "checkbox" && (form.classList.contains("formulario-solicitud") || form.classList.contains("formulario-pasajero"))
            ? "campo-interruptor" : "campo";
        var texto = document.createElement("span");
        texto.textContent = etiqueta;
        var input = document.createElement(tipo === "select" ? "select" : tipo === "textarea" ? "textarea" : "input");
        if (tipo === "select") input.className = "selector-buscable";
        if (input.tagName === "INPUT") input.type = tipo;
        input.name = nombre;
        if (input.type === "checkbox") {
            input.checked = !!valor;
            if (label.classList.contains("campo-interruptor")) input.setAttribute("role", "switch");
        }
        else if (valor != null) input.value = valor;
        input.required = !!requerido;
        label.append(texto, input);
        form.append(label);
        return input;
    }
    function llenar(select, lista, seleccionado) {
        select.replaceChildren(new Option("Seleccione", ""));
        (lista || []).forEach(function (item) { select.add(new Option(item.nombre, item.id)); });
        select.value = seleccionado == null || seleccionado === 0 || seleccionado === "0" ? "" : String(seleccionado);
        d.actualizarSelector(select);
    }
    function fecha(valor) { return valor ? String(valor).slice(0, 10) : ""; }
    function fechaPasajero(valor, defecto) {
        if (!valor) return fecha(defecto);
        var partes = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(valor);
        return partes ? partes[3] + "-" + partes[2] + "-" + partes[1] : fecha(valor);
    }
    function errorEn(error) {
        d.mostrarAviso(error.message, "error");
    }
    function crearFormulario(detalle, catalogo, organizacion) {
        var form = document.createElement("form");
        form.className = "formulario formulario-solicitud";
        var edicion = !!detalle;
        var numero = campo(form, "Número", "numero", "text", detalle ? detalle.id : "0");
        numero.readOnly = true;
        var estado = campo(form, "Estado", "estado", "text", detalle ? (detalle.estadoNombre || detalle.estado) : "En ingreso");
        estado.readOnly = true;
        var grupo = campo(form, "Grupo empresa", "grupoId", "select", null, true);
        llenar(grupo, catalogo.grupos, detalle && detalle.areaEmpresaSolicitanteId);
        grupo.disabled = edicion;
        var solicitante = campo(form, "Solicitante", "solicitanteId", "select");
        llenar(solicitante, catalogo.solicitantes, detalle && detalle.usuarioSolicitanteId);
        solicitante.disabled = edicion;
        var fechaSolicitud = campo(form, "Fecha solicitud", "fechaSolicitud", "date", edicion ? fecha(detalle.fechaSolicitud) : d.fechaActual(), true);
        var checkIn = campo(form, "Check-in", "fechaCheckIn", "date", edicion ? fecha(detalle.fechaCheckIn) : d.fechaActual(), true);
        var checkOut = campo(form, "Check-out", "fechaCheckOut", "date", edicion ? fecha(detalle.fechaCheckOut) : "");
        var conTurno = campo(form, "Sistema de turno", "conSistemaTurno", "checkbox", edicion && detalle.conSistemaTurno);
        var turno = campo(form, "Turno", "turnoId", "select", null, true);
        var rotacion = campo(form, "Turno rotación", "turnoRotacionId", "select");
        llenar(turno, catalogo.turnos, detalle && detalle.turno);
        llenar(rotacion, catalogo.turnos, detalle && detalle.turnoRotacion);
        [fechaSolicitud, checkIn, checkOut, conTurno, turno, rotacion].forEach(function (el) { el.disabled = edicion; });
        function actualizarTurnos() {
            turno.disabled = edicion || !conTurno.checked;
            rotacion.disabled = edicion || !conTurno.checked;
        }
        conTurno.addEventListener("change", actualizarTurnos);
        actualizarTurnos();
        var email = campo(form, "Correo de contacto", "emailContactoCampamento", "email", edicion ? detalle.emailContactoCampamento : "", true);
        var telefono = campo(form, "Teléfono", "numeroContactoCampamento", "text", detalle && detalle.numeroContactoCampamento);
        var adc = campo(form, "ADC", "adc", "text", detalle && detalle.adc);
        var unidad = campo(form, "Unidad de negocio", "unidadNegocioId", "select");
        var division = campo(form, "División", "divisionId", "select");
        var departamento = campo(form, "Departamento", "departamentoId", "select");
        llenar(unidad, organizacion.unidadesNegocio, detalle && detalle.unidadNegocioId);
        llenar(division, organizacion.divisiones, detalle && detalle.divisionId);
        llenar(departamento, organizacion.departamentos, detalle && detalle.departamentoId);
        var contrato = campo(form, "Contrato", "numeroContrato", "text", detalle && detalle.numeroContrato);
        var visita = campo(form, "Reserva visita", "reservaVisita", "checkbox", detalle && detalle.reservaVisita);
        var observacion = campo(form, "Observación", "observacion", "textarea", detalle && detalle.observacion);
        observacion.parentElement.classList.add("campo-ancho");
        var versionOrganizacion = 0;
        var versionGrupo = 0;
        function cargarOrganizacion(unidadId, divisionId, deptoId) {
            var version = ++versionOrganizacion;
            return mid("organizacion", { unidadId: unidadId || 0, divisionId: divisionId || 0 })
                .then(function (datos) {
                    if (version !== versionOrganizacion) return;
                    llenar(unidad, datos.unidadesNegocio, unidadId);
                    llenar(division, datos.divisiones, divisionId);
                    llenar(departamento, datos.departamentos, deptoId);
                }).catch(function (ex) { if (version === versionOrganizacion) errorEn(ex); });
        }
        grupo.addEventListener("change", function () {
            var version = ++versionGrupo;
            var seleccionado = catalogo.grupos.find(function (g) { return String(g.id) === grupo.value; });
            turno.value = seleccionado && seleccionado.turnoId ? String(seleccionado.turnoId) : "";
            d.actualizarSelector(turno);
            contrato.value = seleccionado && seleccionado.contrato || "";
            unidad.disabled = !!(seleccionado && seleccionado.unidadNegocioId);
            cargarOrganizacion(seleccionado && seleccionado.unidadNegocioId, 0, null);
            mid("catalogo", { grupoId: grupo.value || 0 })
                .then(function (datos) { if (version === versionGrupo) llenar(solicitante, datos.solicitantes); })
                .catch(function (ex) { if (version === versionGrupo) errorEn(ex); });
        });
        unidad.addEventListener("change", function () { cargarOrganizacion(unidad.value, 0, null); });
        division.addEventListener("change", function () { cargarOrganizacion(unidad.value, division.value, null); });
        var guardar = document.createElement("button");
        guardar.type = "submit";
        guardar.className = "boton-primario";
        guardar.textContent = "Guardar";
        form.append(guardar);
        form.addEventListener("submit", function (evento) {
            evento.preventDefault();
            if (!form.reportValidity()) return;
            guardar.disabled = true;
            var cuerpo = {
                id: edicion ? detalle.id : 0, grupoId: Number(grupo.value), solicitanteId: Number(solicitante.value) || null,
                fechaSolicitud: fechaSolicitud.value || null, fechaCheckIn: checkIn.value || null,
                fechaCheckOut: checkOut.value || null, conSistemaTurno: conTurno.checked,
                turnoId: Number(turno.value) || null, turnoRotacionId: Number(rotacion.value) || null,
                emailContactoCampamento: email.value, numeroContactoCampamento: telefono.value,
                adc: adc.value, unidadNegocioId: Number(unidad.value) || null, divisionId: Number(division.value) || null,
                departamentoId: Number(departamento.value) || null, numeroContrato: contrato.value,
                reservaVisita: visita.checked, observacion: observacion.value
            };
            mid(edicion ? "editar" : "crear", cuerpo)
                .then(function (resultado) {
                    d.mostrarAviso("Solicitud guardada N° " + resultado.id + ".", "success");
                    d.cargar("solicitud");
                    abrirDetalle(resultado.id);
                }).catch(function (ex) { errorEn(ex); }).finally(function () { guardar.disabled = false; });
        });
        d.inicializarSelectores(form);
        return form;
    }
    function abrirFormulario(detalle) {
        var grupoId = detalle ? detalle.areaEmpresaSolicitanteId : 0;
        var unidadId = detalle ? detalle.unidadNegocioId || 0 : 0;
        var divisionId = detalle ? detalle.divisionId || 0 : 0;
        Promise.all([
            mid("catalogo", { grupoId: grupoId }),
            mid("organizacion", { unidadId: unidadId, divisionId: divisionId })
        ]).then(function (resultados) {
            d.abrirPanel(detalle ? "Editar solicitud " + detalle.id : "Nueva solicitud",
                crearFormulario(detalle, resultados[0], resultados[1]));
        }).catch(function (ex) { d.mostrarAviso(ex.message, "error"); });
    }
    function ejecutar(botonAccion, url, cuerpo, alTerminar) {
        botonAccion.disabled = true;
        mid(url, cuerpo).then(alTerminar)
            .catch(errorEn)
            .finally(function () { botonAccion.disabled = false; });
    }
    function mostrarDetalle(detalle, pasajeros) {
        var panel = document.createElement("div");
        panel.className = "solicitud-detalle";
        var resumen = document.createElement("dl");
        resumen.className = "solicitud-resumen";
        [
            ["Estado", detalle.estadoNombre || detalle.estado],
            ["Grupo empresa", detalle.areaEmpresaSolicitante || "—"],
            ["Check-in", fecha(detalle.fechaCheckIn) || "—"],
            ["Check-out", fecha(detalle.fechaCheckOut) || "—"]
        ].forEach(function (item) {
            var dato = document.createElement("div");
            var etiqueta = document.createElement("dt");
            var valor = document.createElement("dd");
            etiqueta.textContent = item[0];
            valor.textContent = item[1];
            dato.append(etiqueta, valor);
            resumen.append(dato);
        });
        panel.append(resumen);
        var accionesSolicitud = document.createElement("div");
        accionesSolicitud.className = "solicitud-acciones";
        if (detalle.estado === "P") {
            accionesSolicitud.append(boton("Editar solicitud", function () { abrirFormulario(detalle); }));
            var anular = boton("Anular solicitud", function () {
                if (!window.confirm("¿Anular esta solicitud?")) return;
                ejecutar(anular, "anular", { id: detalle.id }, function () { d.cargar("solicitud"); abrirDetalle(detalle.id); });
            });
            accionesSolicitud.append(anular);
        }
        if (detalle.estado === "R") {
            var notificar = boton("Enviar correo de reservas", function () {
                ejecutar(notificar, "notificar", { id: detalle.id },
                    function () { d.mostrarAviso("Correo enviado.", "success"); });
            });
            accionesSolicitud.append(notificar);
        }
        panel.append(accionesSolicitud);
        var seccion = document.createElement("section");
        seccion.className = "solicitud-pasajeros";
        var cabeceraSeccion = document.createElement("div");
        cabeceraSeccion.className = "solicitud-pasajeros-cabecera";
        var titulo = document.createElement("h3");
        titulo.textContent = "Pasajeros y cupos (" + pasajeros.length + ")";
        cabeceraSeccion.append(titulo);
        var accionesPasajeros = document.createElement("div");
        accionesPasajeros.className = "solicitud-acciones";
        if (detalle.estado === "P") {
            var agregar = boton("Agregar pasajero", function () { abrirPasajero(detalle); });
            agregar.className = "boton-primario";
            accionesPasajeros.append(agregar, boton("Importar pasajeros", function () { abrirImportacion(detalle); }));
        }
        cabeceraSeccion.append(accionesPasajeros);
        seccion.append(cabeceraSeccion);
        if (detalle.estado === "R" || detalle.estado === "A") {
            var avisoEstado = document.createElement("p");
            avisoEstado.className = "solicitud-pasajeros-aviso";
            avisoEstado.textContent = "No se pueden modificar los pasajeros de una solicitud reservada o anulada.";
            seccion.append(avisoEstado);
        }
        var accionesSeleccion = document.createElement("div");
        accionesSeleccion.className = "solicitud-acciones solicitud-acciones-seleccion";
        if (detalle.estado === "P") {
            var generar = boton("Generar reservas", function () {
                if (!window.confirm("¿Esta seguro que desea generar las reservas para la solicitud seleccionada?")) return;
                ejecutar(generar, "generar", { id: detalle.id }, function (resultado) {
                    d.mostrarAviso("Reservas generadas. Primera reserva N° " + resultado.primeraReservaId + ".", "success");
                    if (resultado.advertenciaCorreo) d.mostrarAviso(resultado.advertenciaCorreo, "error");
                    d.cargar("solicitud");
                    abrirDetalle(detalle.id);
                });
            });
            generar.disabled = true;
        }
        var seleccion = new Set();
        var tabla = document.createElement("table");
        tabla.className = "solicitud-tabla";
        var encabezado = document.createElement("tr");
        var seleccionarTodos = document.createElement("input");
        seleccionarTodos.type = "checkbox";
        seleccionarTodos.setAttribute("aria-label", "Seleccionar todos los pasajeros");
        var celdaSeleccion = document.createElement("th");
        if (detalle.estado === "P") {
            celdaSeleccion.append(seleccionarTodos);
            encabezado.append(celdaSeleccion);
        }
        var columnasPasajeros = ["Habitación", "Identificador", "Tipo identificador", "Nombre"];
        if (detalle.estado === "P") columnasPasajeros.push("Acciones");
        columnasPasajeros.forEach(function (texto) {
            var th = document.createElement("th");
            th.textContent = texto;
            encabezado.append(th);
        });
        var cabecera = document.createElement("thead");
        cabecera.append(encabezado);
        var cuerpo = document.createElement("tbody");
        tabla.append(cabecera, cuerpo);
        var eliminarSeleccionados;
        var asignarSeleccionados;
        var desasignarSeleccionados;
        function pasajeroTieneHabitacion(p) {
            return Number(p.habitacionId) > 0 || !!(p.habitacion && String(p.habitacion).trim());
        }
        function pasajerosSeleccionadosConHabitacion() {
            return pasajeros.filter(function (p) {
                return seleccion.has(p.id) && pasajeroTieneHabitacion(p);
            });
        }
        function actualizarSeleccion() {
            if (detalle.estado !== "P") return;
            generar.disabled = seleccion.size === 0;
            eliminarSeleccionados.disabled = seleccion.size === 0;
            asignarSeleccionados.disabled = seleccion.size === 0;
            desasignarSeleccionados.disabled = pasajerosSeleccionadosConHabitacion().length === 0;
            seleccionarTodos.checked = pasajeros.length > 0 && seleccion.size === pasajeros.length;
            seleccionarTodos.indeterminate = seleccion.size > 0 && seleccion.size < pasajeros.length;
        }
        if (detalle.estado === "P") {
            eliminarSeleccionados = boton("Eliminar pasajeros", async function () {
                if (!window.confirm("¿Eliminar los pasajeros seleccionados?")) return;
                var ids = Array.from(seleccion);
                panel.querySelectorAll("button, input").forEach(function (control) { control.disabled = true; });
                var realizados = 0;
                try {
                    for (var id of ids) {
                        await mid("eliminar", { id: detalle.id, pasajeroId: id });
                        realizados++;
                    }
                    d.mostrarAviso("Pasajeros eliminados: " + realizados + ".", "success");
                } catch (ex) {
                    d.mostrarAviso("Eliminados antes del error: " + realizados + ". " + ex.message, "error");
                }
                abrirDetalle(detalle.id);
            });
            asignarSeleccionados = boton("Asignar habitaciones", function () {
                abrirHabitacionesMultiples(detalle, pasajeros.filter(function (p) { return seleccion.has(p.id); }), pasajeros);
            });
            desasignarSeleccionados = boton("Desasignar habitaciones", async function () {
                var candidatos = pasajerosSeleccionadosConHabitacion();
                if (!candidatos.length) return;
                if (!window.confirm("¿Quitar la habitación de los " + candidatos.length + " pasajero(s) seleccionado(s)?"))
                    return;
                panel.querySelectorAll("button, input").forEach(function (control) { control.disabled = true; });
                var realizados = 0;
                var omitidos = 0;
                var primerError = "";
                try {
                    for (var i = 0; i < candidatos.length; i++) {
                        var pasajero = candidatos[i];
                        try {
                            await mid("desasignar", { solicitudId: detalle.id, pasajeroId: pasajero.id });
                            realizados++;
                        } catch (ex) {
                            omitidos++;
                            if (!primerError) primerError = ex.message || "Error desconocido.";
                        }
                    }
                    var texto = "Habitaciones quitadas: " + realizados + ".";
                    if (omitidos) {
                        texto += " Sin cambio en " + omitidos + " fila(s).";
                        if (primerError) texto += " " + primerError;
                    }
                    d.mostrarAviso(texto, omitidos ? "warning" : "success");
                } catch (ex) {
                    d.mostrarAviso("Quitadas antes del error: " + realizados + ". " + ex.message, "error");
                }
                abrirDetalle(detalle.id);
            });
            accionesSeleccion.append(asignarSeleccionados, desasignarSeleccionados, eliminarSeleccionados, generar);
            seccion.append(accionesSeleccion);
            seleccionarTodos.addEventListener("change", function () {
                seleccion.clear();
                cuerpo.querySelectorAll("input[type=checkbox]").forEach(function (checkbox) {
                    checkbox.checked = seleccionarTodos.checked;
                    if (checkbox.checked) seleccion.add(Number(checkbox.value));
                });
                actualizarSeleccion();
            });
            actualizarSeleccion();
        }
        pasajeros.forEach(function (pasajero) {
            var fila = document.createElement("tr");
            var tdSeleccion = document.createElement("td");
            if (detalle.estado === "P") {
                var checkbox = document.createElement("input");
                checkbox.type = "checkbox";
                checkbox.value = pasajero.id;
                checkbox.setAttribute("aria-label", "Seleccionar " + (pasajero.nombrePasajero || "cupo " + pasajero.id));
                checkbox.addEventListener("change", function () {
                    if (checkbox.checked) seleccion.add(pasajero.id);
                    else seleccion.delete(pasajero.id);
                    actualizarSeleccion();
                });
                tdSeleccion.append(checkbox);
                fila.append(tdSeleccion);
            }
            [pasajero.habitacion || "", pasajero.rut || "", pasajero.tipoIdentificador || "",
                pasajero.nombrePasajero || "Cupo sin identificar"].forEach(function (valor) {
                var td = document.createElement("td");
                td.textContent = valor;
                fila.append(td);
            });
            var acciones = document.createElement("td");
            if (detalle.estado === "P") {
                acciones.append(boton("Editar", function () { abrirPasajero(detalle, pasajero); }));
                var eliminar = boton("Eliminar", function () {
                    if (!window.confirm("¿Eliminar este pasajero o cupo?")) return;
                    ejecutar(eliminar, "eliminar", { id: detalle.id, pasajeroId: pasajero.id }, function () { abrirDetalle(detalle.id); });
                });
                acciones.append(eliminar);
                fila.append(acciones);
            }
            cuerpo.append(fila);
        });
        if (pasajeros.length) {
            var tablaContenedor = document.createElement("div");
            tablaContenedor.className = "solicitud-tabla-contenedor";
            tablaContenedor.append(tabla);
            seccion.append(tablaContenedor);
        } else {
            var vacio = document.createElement("p");
            vacio.className = "solicitud-pasajeros-vacio";
            vacio.textContent = detalle.estado === "P"
                ? "Aún no hay pasajeros ni cupos. Agregue uno o importe una planilla."
                : "No hay pasajeros ni cupos para esta solicitud.";
            seccion.append(vacio);
        }
        panel.append(seccion);
        d.abrirPanel("Solicitud N° " + detalle.id, panel);
    }
    function abrirDetalle(id) {
        Promise.all([mid("detalle", { id: id }),
            mid("pasajeros", { id: id })])
            .then(function (r) { mostrarDetalle(r[0], r[1].pasajeros); })
            .catch(function (ex) { d.mostrarAviso(ex.message, "error"); });
    }
    function abrirPasajero(detalle, pasajero) {
        mid("catalogo", { grupoId: detalle.areaEmpresaSolicitanteId }).then(function (catalogo) {
            var form = document.createElement("form");
            form.className = "formulario formulario-pasajero";
            var porCantidad = !pasajero ? campo(form, "¿Por cantidad?", "porCantidad", "checkbox", false) : null;
            var cantidad = !pasajero ? campo(form, "Cantidad", "cantidad", "number", 1) : null;
            if (cantidad) { cantidad.min = "1"; cantidad.max = "999"; }
            var usuario = campo(form, "Pasajero", "usuario", "select", null, true);
            llenar(usuario, catalogo.solicitantes, pasajero && pasajero.usuarioPasajeroId);
            var desde = campo(form, "Check-in", "desde", "date", fechaPasajero(pasajero && pasajero.checkIn, detalle.fechaCheckIn), true);
            var hasta = campo(form, "Check-out", "hasta", "date", pasajero ? fechaPasajero(pasajero.checkOut, null) : fecha(detalle.fechaCheckOut));
            desde.readOnly = true;
            hasta.readOnly = true;
            if (porCantidad) {
                function actualizarCantidad() {
                    cantidad.parentElement.hidden = !porCantidad.checked;
                    cantidad.required = porCantidad.checked;
                    usuario.parentElement.hidden = porCantidad.checked;
                    usuario.required = !porCantidad.checked;
                }
                porCantidad.addEventListener("change", actualizarCantidad);
                actualizarCantidad();
            }
            var guardar = document.createElement("button");
            guardar.type = "submit";
            guardar.className = "boton-primario";
            guardar.textContent = "Guardar";
            var acciones = document.createElement("div");
            acciones.className = "formulario-pie solicitud-acciones";
            acciones.append(boton("Cancelar", function () { abrirDetalle(detalle.id); }), guardar);
            form.append(acciones);
            form.addEventListener("submit", function (e) {
                e.preventDefault();
                if (!form.reportValidity()) return;
                if (porCantidad && porCantidad.checked) {
                    ejecutar(guardar, "cupos", { id: detalle.id, cantidad: Number(cantidad.value) },
                        function () { abrirDetalle(detalle.id); });
                    return;
                }
                ejecutar(guardar, "pasajero", { solicitudId: detalle.id, pasajeroId: pasajero ? pasajero.id : 0,
                    usuarioPasajeroId: Number(usuario.value), fechaCheckIn: desde.value, fechaCheckOut: hasta.value || null },
                    function () { abrirDetalle(detalle.id); });
            });
            d.inicializarSelectores(form);
            d.abrirPanel("Pasajero de solicitud " + detalle.id, form);
        }).catch(function (ex) { d.mostrarAviso(ex.message, "error"); });
    }
    function abrirHabitacionesMultiples(detalle, pasajeros, todosPasajeros) {
        mid("habitaciones", { id: detalle.id }).then(function (resultado) {
            var form = document.createElement("form");
            form.className = "formulario solicitud-asignacion";
            var modulo = campo(form, "Seleccione módulo", "modulo", "select");
            var piso = campo(form, "Seleccione piso", "piso", "select");
            var busqueda = campo(form, "Buscar habitaciones", "busquedaHabitaciones", "search");
            var habitaciones = resultado.habitaciones;
            function opcionesUnicas(lista, id, nombre) {
                return Array.from(new Map(lista.map(function (h) { return [h[id], { id: h[id], nombre: h[nombre] }]; })).values());
            }
            llenar(modulo, opcionesUnicas(habitaciones, "pabellonId", "pabellonName"));
            var lista = document.createElement("div");
            lista.className = "habitaciones-lista";
            var avisoVacio = document.createElement("p");
            avisoVacio.className = "habitaciones-vacio";
            avisoVacio.textContent = "No hay habitaciones disponibles para los filtros seleccionados.";
            var resumenSeleccion = document.createElement("ol");
            resumenSeleccion.className = "habitaciones-seleccion";
            var seleccionadas = [];
            var controlesHabitacion = new Map();
            var habitacionesPorId = new Map(habitaciones.map(function (h) { return [h.habitacionId, h]; }));
            function aplicarOcupacion(ocupacion, id, pasajero) {
                var anterior = pasajero.habitacionId;
                if (anterior === id) return true;
                var habitacion = habitacionesPorId.get(id);
                if (!habitacion || (ocupacion.get(id) || 0) >= habitacion.cantidadCamasDisponibles) return false;
                if (anterior) ocupacion.set(anterior, (ocupacion.get(anterior) || 0) - 1);
                ocupacion.set(id, (ocupacion.get(id) || 0) + 1);
                return true;
            }
            function calcularOcupacion(seleccion) {
                var ocupacion = new Map();
                todosPasajeros.forEach(function (pasajero) {
                    if (pasajero.habitacionId) {
                        ocupacion.set(pasajero.habitacionId, (ocupacion.get(pasajero.habitacionId) || 0) + 1);
                    }
                });
                for (var indice = 0; indice < seleccion.length; indice++) {
                    if (!aplicarOcupacion(ocupacion, seleccion[indice], pasajeros[indice])) return null;
                }
                return ocupacion;
            }
            function cambiarCantidad(id, cantidad) {
                if (cantidad > pasajeros.length) return false;
                var propuesta = seleccionadas.slice();
                var actual = propuesta.filter(function (seleccionada) { return seleccionada === id; }).length;
                while (actual > cantidad) {
                    propuesta.splice(propuesta.lastIndexOf(id), 1);
                    actual--;
                }
                while (actual < cantidad) {
                    propuesta.push(id);
                    actual++;
                }
                if (propuesta.length > pasajeros.length || !calcularOcupacion(propuesta)) return false;
                seleccionadas = propuesta;
                return true;
            }
            function cantidadMaxima(id, actual, ocupacion) {
                var prueba = new Map(ocupacion);
                var maximo = actual;
                for (var indice = seleccionadas.length; indice < pasajeros.length; indice++) {
                    if (!aplicarOcupacion(prueba, id, pasajeros[indice])) break;
                    maximo++;
                }
                return maximo;
            }
            function aplicarCantidad(id, valor) {
                if (!/^\d+$/.test(valor) || !Number.isSafeInteger(Number(valor)) ||
                    !cambiarCantidad(id, Number(valor))) {
                    d.mostrarAviso("La cantidad no es válida o supera las camas disponibles. Revise las selecciones anteriores.", "error");
                }
                actualizarResumen();
            }
            function camasDisponibles(h, ocupacion) {
                return Math.max(0, Math.min(h.totalCamas, h.cantidadCamasDisponibles - (ocupacion.get(h.habitacionId) || 0)));
            }
            function descripcionHabitacion(h) {
                return [h.pabellonName, h.moduloName, h.habitacionName].filter(Boolean).join(" · ");
            }
            function normalizar(texto) {
                return texto.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
            }
            function asignarAutomaticamente() {
                var propuesta = seleccionadas.slice();
                var ocupacion = calcularOcupacion(propuesta);
                var visibles = Array.from(controlesHabitacion.keys());
                var indiceHabitacion = 0;
                while (propuesta.length < pasajeros.length && indiceHabitacion < visibles.length) {
                    var id = visibles[indiceHabitacion];
                    if (aplicarOcupacion(ocupacion, id, pasajeros[propuesta.length])) {
                        propuesta.push(id);
                    } else {
                        indiceHabitacion++;
                    }
                }
                if (propuesta.length !== pasajeros.length) {
                    d.mostrarAviso("No se pudo completar la asignación automática. Pasajeros pendientes: " +
                        (pasajeros.length - propuesta.length) + ". Revise las habitaciones y filtros.", "error");
                    return;
                }
                seleccionadas = propuesta;
                actualizarResumen();
                guardarAsignaciones();
            }
            function actualizarResumen() {
                contador.textContent = "Pasajeros con habitación seleccionada: " + seleccionadas.length + " de " + pasajeros.length;
                resumenSeleccion.replaceChildren();
                seleccionadas.forEach(function (id, indice) {
                    var item = document.createElement("li");
                    var pasajero = pasajeros[indice];
                    item.textContent = (pasajero.nombrePasajero || "Cupo " + pasajero.id) + " → " +
                        descripcionHabitacion(habitacionesPorId.get(id));
                    resumenSeleccion.append(item);
                });
                var ocupacion = calcularOcupacion(seleccionadas);
                var siguiente = pasajeros[seleccionadas.length];
                siguienteNombre.textContent = siguiente ? (siguiente.nombrePasajero || "Cupo " + siguiente.id) : "—";
                controlesHabitacion.forEach(function (control, id) {
                    var h = habitacionesPorId.get(id);
                    var disponibles = camasDisponibles(h, ocupacion);
                    control.cantidad.textContent = disponibles;
                    control.disponibilidad.className = "habitacion-disponibilidad " +
                        (disponibles === 0 ? "disponibilidad-agotada" :
                            disponibles === h.totalCamas ? "disponibilidad-completa" : "disponibilidad-parcial");
                    var seleccionadasHabitacion = seleccionadas.filter(function (valor) { return valor === id; }).length;
                    var maximo = cantidadMaxima(id, seleccionadasHabitacion, ocupacion);
                    control.input.value = String(seleccionadasHabitacion);
                    control.input.max = String(maximo);
                    control.menos.disabled = seleccionadasHabitacion === 0;
                    control.mas.disabled = seleccionadasHabitacion >= maximo;
                });
                asignar.disabled = seleccionadas.length !== pasajeros.length;
            }
            function mostrarHabitaciones() {
                lista.replaceChildren();
                controlesHabitacion.clear();
                var termino = normalizar(busqueda.value.trim());
                var filtradas = habitaciones.filter(function (h) {
                    return (!modulo.value || String(h.pabellonId) === modulo.value) &&
                        (!piso.value || String(h.moduloId) === piso.value) &&
                        normalizar(descripcionHabitacion(h)).includes(termino);
                });
                avisoVacio.hidden = filtradas.length > 0;
                filtradas.forEach(function (h) {
                    var tarjeta = document.createElement("div");
                    tarjeta.className = "habitacion-opcion";
                    var controlCantidad = document.createElement("span");
                    controlCantidad.className = "campo-cantidad";
                    var input = document.createElement("input");
                    input.type = "number";
                    input.min = "0";
                    input.step = "1";
                    input.inputMode = "numeric";
                    input.setAttribute("aria-label", "Camas asignadas en " + descripcionHabitacion(h));
                    var menos = boton("-", function () { aplicarCantidad(h.habitacionId, String(Number(input.value) - 1)); });
                    var mas = boton("+", function () { aplicarCantidad(h.habitacionId, String(Number(input.value) + 1)); });
                    menos.className = mas.className = "campo-cantidad-boton";
                    menos.setAttribute("aria-label", "Disminuir camas en " + descripcionHabitacion(h));
                    mas.setAttribute("aria-label", "Aumentar camas en " + descripcionHabitacion(h));
                    input.addEventListener("change", function () { aplicarCantidad(h.habitacionId, input.value); });
                    input.addEventListener("keydown", function (evento) { if (evento.key === "Enter") { evento.preventDefault(); input.blur(); } });
                    controlCantidad.append(menos, input, mas);
                    var contenido = document.createElement("span");
                    contenido.className = "habitacion-opcion-contenido";
                    var nombre = document.createElement("strong");
                    nombre.textContent = h.habitacionName || "—";
                    var ubicacion = document.createElement("span");
                    ubicacion.textContent = [h.pabellonName, h.moduloName].filter(Boolean).join(" · ");
                    var disponibilidad = document.createElement("span");
                    disponibilidad.className = "habitacion-disponibilidad";
                    disponibilidad.setAttribute("aria-live", "polite");
                    var cantidad = document.createElement("span");
                    var conector = document.createElement("span");
                    conector.textContent = " de ";
                    var total = document.createElement("span");
                    total.textContent = h.totalCamas;
                    var etiquetaDisponibilidad = document.createElement("span");
                    etiquetaDisponibilidad.textContent = " camas disponibles";
                    disponibilidad.append(cantidad, conector, total, etiquetaDisponibilidad);
                    contenido.append(nombre, ubicacion, disponibilidad, controlCantidad);
                    tarjeta.append(contenido);
                    lista.append(tarjeta);
                    controlesHabitacion.set(h.habitacionId, { input: input, menos: menos, mas: mas, disponibilidad: disponibilidad, cantidad: cantidad });
                });
                actualizarResumen();
            }
            modulo.addEventListener("change", function () {
                var filtradas = habitaciones.filter(function (h) { return !modulo.value || String(h.pabellonId) === modulo.value; });
                llenar(piso, opcionesUnicas(filtradas, "moduloId", "moduloName"));
                mostrarHabitaciones();
            });
            piso.addEventListener("change", mostrarHabitaciones);
            busqueda.addEventListener("input", mostrarHabitaciones);
            busqueda.addEventListener("keydown", function (evento) {
                if (evento.key === "Enter") evento.preventDefault();
            });
            llenar(piso, opcionesUnicas(habitaciones, "moduloId", "moduloName"));
            var contador = document.createElement("p");
            contador.className = "habitaciones-contador";
            contador.setAttribute("aria-live", "polite");
            var instruccion = document.createElement("p");
            instruccion.className = "habitaciones-instruccion";
            instruccion.textContent = "Indique la cantidad de camas por habitación. Las camas se asignan a los pasajeros en el orden seleccionado.";
            var siguientePasajero = document.createElement("p");
            siguientePasajero.className = "habitaciones-contador";
            var siguienteEtiqueta = document.createElement("span");
            siguienteEtiqueta.textContent = "Siguiente pasajero: ";
            var siguienteNombre = document.createElement("strong");
            siguientePasajero.append(siguienteEtiqueta, siguienteNombre);
            var asignar = document.createElement("button");
            asignar.type = "submit";
            asignar.className = "boton-primario";
            asignar.textContent = "Asignar";
            var automatico = boton("Asignar automático", asignarAutomaticamente);
            var acciones = document.createElement("div");
            acciones.className = "solicitud-acciones";
            acciones.append(boton("Cancelar", function () { abrirDetalle(detalle.id); }), asignar, automatico);
            form.prepend(acciones);
            form.append(contador, instruccion, siguientePasajero, avisoVacio, lista, resumenSeleccion);
            mostrarHabitaciones();
            async function guardarAsignaciones() {
                if (seleccionadas.length !== pasajeros.length) {
                    d.mostrarAviso("Seleccione una habitación para cada pasajero.", "error");
                    return;
                }
                var ids = seleccionadas.slice();
                form.querySelectorAll("button, input, select").forEach(function (control) { control.disabled = true; });
                var realizados = 0;
                try {
                    for (var i = 0; i < pasajeros.length; i++) {
                        await mid("asignar", { solicitudId: detalle.id,
                            pasajeroId: pasajeros[i].id, habitacionId: ids[i] });
                        realizados++;
                    }
                    d.mostrarAviso("Habitaciones asignadas: " + realizados + ".", "success");
                } catch (ex) {
                    d.mostrarAviso("Asignaciones realizadas antes del error: " + realizados + ". " + ex.message, "error");
                }
                abrirDetalle(detalle.id);
            }
            form.addEventListener("submit", function (e) {
                e.preventDefault();
                guardarAsignaciones();
            });
            d.inicializarSelectores(form);
            d.abrirPanel("Asignar habitaciones", form);
        }).catch(function (ex) { d.mostrarAviso(ex.message, "error"); });
    }
    function abrirImportacion(detalle) {
        var form = document.createElement("form");
        form.className = "formulario solicitud-importacion";
        var aviso = document.createElement("p");
        aviso.textContent = "La importación puede crear usuarios y actualizar nombres, género, grupo y empresa de usuarios existentes. Revise las filas antes de continuar.";
        var plantilla = document.createElement("a");
        plantilla.href = "/Solicitud/PlantillaPasajeros";
        plantilla.textContent = "Descargar plantilla";
        var archivo = campo(form, "Planilla de pasajeros (.xlsx)", "archivo", "file", null, true);
        archivo.accept = ".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var resultado = document.createElement("div");
        resultado.className = "importacion-resultado";
        resultado.setAttribute("aria-live", "polite");
        var importar = document.createElement("button");
        importar.type = "submit";
        importar.className = "boton-primario";
        importar.textContent = "Importar";
        form.prepend(aviso, plantilla);
        form.append(importar, resultado, boton("Volver a la solicitud", function () { abrirDetalle(detalle.id); }));
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            if (!form.reportValidity() || !window.confirm("La carga puede modificar usuarios existentes. ¿Desea continuar?")) return;
            importar.disabled = true;
            resultado.replaceChildren();
            d.enviarArchivo(ruta, archivo.files[0], { id: detalle.id }).then(function (datos) {
                var rechazadas = datos.filas.filter(function (fila) { return !fila.procesada; });
                var titulo = document.createElement("h3");
                titulo.textContent = "Resultado de la importación";
                var resumen = document.createElement("dl");
                resumen.className = "importacion-resumen";
                [
                    ["Total de filas", datos.filas.length],
                    ["Procesadas", datos.filas.length - rechazadas.length],
                    ["No procesadas", rechazadas.length]
                ].forEach(function (dato) {
                    var tarjeta = document.createElement("div");
                    var etiqueta = document.createElement("dt");
                    var cantidad = document.createElement("dd");
                    etiqueta.textContent = dato[0];
                    cantidad.textContent = dato[1];
                    tarjeta.append(etiqueta, cantidad);
                    resumen.append(tarjeta);
                });
                resultado.append(titulo, resumen);
                if (!rechazadas.length) {
                    var completo = document.createElement("p");
                    completo.className = "importacion-completa";
                    completo.textContent = "No hay filas pendientes de revisión.";
                    resultado.append(completo);
                    return;
                }
                d.mostrarAviso("Hay filas de la importación pendientes de revisión.", "error");
                var subtitulo = document.createElement("h4");
                subtitulo.textContent = "Detalle de filas no procesadas";
                var lista = document.createElement("ol");
                lista.className = "importacion-errores";
                rechazadas.forEach(function (x) {
                    var item = document.createElement("li");
                    var fila = document.createElement("span");
                    fila.className = "importacion-fila";
                    fila.textContent = "Fila " + x.numeroFila;
                    var mensaje = document.createElement("span");
                    mensaje.textContent = x.mensaje;
                    item.append(fila, mensaje);
                    lista.append(item);
                });
                resultado.append(subtitulo, lista);
            }).catch(function (ex) {
                d.mostrarAviso(ex.message, "error");
            })
                .finally(function () { importar.disabled = false; });
        });
        d.abrirPanel("Importar pasajeros", form);
    }
    function fila(origen) {
        return { acciones: origen, numero: origen.numero, fecha: fecha(origen.fecha),
            empresa: origen.empresa, solicitante: origen.solicitante || "", estado: origen.estado || "" };
    }
    function columnas() {
        return ["acciones", "numero", "fecha", "empresa", "solicitante", "estado"].map(function (nombre) {
            if (nombre === "acciones") return { data: nombre, orderable: false, createdCell: function (td, dato) {
                td.replaceChildren(boton("Ver", function () { abrirDetalle(dato.numero); }));
            } };
            return { data: nombre, render: DataTable.render.text() };
        });
    }
    return { filtros: filtros, filtroInicial: filtroInicial, buscar: buscar,
        fila: fila, columnas: columnas, abrirNueva: function () { abrirFormulario(null); } };
}
