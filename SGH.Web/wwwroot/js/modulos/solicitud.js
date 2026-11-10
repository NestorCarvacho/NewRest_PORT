export function crearSolicitud(d) {
    var base = "/Solicitud/Datos?handler=";
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
    function urlBuscar(filtro) {
        return base + "Buscar" + d.consulta(filtro);
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
        label.className = "campo";
        var texto = document.createElement("span");
        texto.textContent = etiqueta;
        var input = document.createElement(tipo === "select" ? "select" : tipo === "textarea" ? "textarea" : "input");
        if (input.tagName === "INPUT") input.type = tipo;
        input.name = nombre;
        if (input.type === "checkbox") input.checked = !!valor;
        else if (valor != null) input.value = valor;
        input.required = !!requerido;
        label.append(texto, input);
        form.append(label);
        return input;
    }
    function llenar(select, lista, seleccionado) {
        select.replaceChildren(new Option("Seleccione", ""));
        (lista || []).forEach(function (item) { select.add(new Option(item.nombre, item.id)); });
        select.value = seleccionado == null ? "" : String(seleccionado);
    }
    function fecha(valor) { return valor ? String(valor).slice(0, 10) : ""; }
    function fechaPasajero(valor, defecto) {
        if (!valor) return fecha(defecto);
        var partes = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(valor);
        return partes ? partes[3] + "-" + partes[2] + "-" + partes[1] : fecha(valor);
    }
    function errorEn(contenedor, error) {
        var aviso = contenedor.querySelector("[data-error]");
        if (aviso) aviso.textContent = error.message;
        else d.mostrarAviso(error.message, "error");
    }
    function crearFormulario(detalle, catalogo, organizacion) {
        var form = document.createElement("form");
        form.className = "formulario";
        var error = document.createElement("p");
        error.dataset.error = "";
        error.setAttribute("role", "alert");
        form.append(error);
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
        var versionOrganizacion = 0;
        var versionGrupo = 0;
        function cargarOrganizacion(unidadId, divisionId, deptoId) {
            var version = ++versionOrganizacion;
            return d.pedir(base + "Organizacion" + d.consulta({ unidadId: unidadId || 0, divisionId: divisionId || 0 }))
                .then(function (datos) {
                    if (version !== versionOrganizacion) return;
                    llenar(unidad, datos.unidadesNegocio, unidadId);
                    llenar(division, datos.divisiones, divisionId);
                    llenar(departamento, datos.departamentos, deptoId);
                }).catch(function (ex) { if (version === versionOrganizacion) errorEn(form, ex); });
        }
        grupo.addEventListener("change", function () {
            var version = ++versionGrupo;
            var seleccionado = catalogo.grupos.find(function (g) { return String(g.id) === grupo.value; });
            turno.value = seleccionado && seleccionado.turnoId ? String(seleccionado.turnoId) : "";
            contrato.value = seleccionado && seleccionado.contrato || "";
            unidad.disabled = !!(seleccionado && seleccionado.unidadNegocioId);
            cargarOrganizacion(seleccionado && seleccionado.unidadNegocioId, 0, null);
            d.pedir(base + "Catalogo" + d.consulta({ grupoId: grupo.value || 0 }))
                .then(function (datos) { if (version === versionGrupo) llenar(solicitante, datos.solicitantes); })
                .catch(function (ex) { if (version === versionGrupo) errorEn(form, ex); });
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
            error.textContent = "";
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
            d.enviar(base + (edicion ? "Actualizar" : "Crear"), cuerpo)
                .then(function (resultado) {
                    d.mostrarAviso("Solicitud guardada N° " + resultado.id + ".", "success");
                    d.cargar("solicitud");
                    abrirDetalle(resultado.id);
                }).catch(function (ex) { errorEn(form, ex); }).finally(function () { guardar.disabled = false; });
        });
        return form;
    }
    function abrirFormulario(detalle) {
        var grupoId = detalle ? detalle.areaEmpresaSolicitanteId : 0;
        var unidadId = detalle ? detalle.unidadNegocioId || 0 : 0;
        var divisionId = detalle ? detalle.divisionId || 0 : 0;
        Promise.all([
            d.pedir(base + "Catalogo" + d.consulta({ grupoId: grupoId })),
            d.pedir(base + "Organizacion" + d.consulta({ unidadId: unidadId, divisionId: divisionId }))
        ]).then(function (resultados) {
            d.abrirPanel(detalle ? "Editar solicitud " + detalle.id : "Nueva solicitud",
                crearFormulario(detalle, resultados[0], resultados[1]));
        }).catch(function (ex) { d.mostrarAviso(ex.message, "error"); });
    }
    function ejecutar(botonAccion, url, cuerpo, alTerminar, contenedor) {
        botonAccion.disabled = true;
        d.enviar(base + url, cuerpo).then(alTerminar)
            .catch(function (ex) { errorEn(contenedor, ex); })
            .finally(function () { botonAccion.disabled = false; });
    }
    function mostrarDetalle(detalle, pasajeros) {
        var panel = document.createElement("div");
        var error = document.createElement("p");
        error.dataset.error = "";
        error.setAttribute("role", "alert");
        panel.append(error);
        var resumen = document.createElement("p");
        resumen.textContent = "Estado: " + (detalle.estadoNombre || detalle.estado) +
            " · Grupo: " + (detalle.areaEmpresaSolicitante || "") +
            " · Estadía: " + fecha(detalle.fechaCheckIn) + " a " + (fecha(detalle.fechaCheckOut) || "sin fecha de salida");
        panel.append(resumen);
        if (detalle.estado === "P") {
            panel.append(boton("Editar solicitud", function () { abrirFormulario(detalle); }));
            var anular = boton("Anular solicitud", function () {
                if (!window.confirm("¿Anular esta solicitud?")) return;
                ejecutar(anular, "Anular", { id: detalle.id }, function () { d.cargar("solicitud"); abrirDetalle(detalle.id); }, panel);
            });
            panel.append(anular);
            panel.append(boton("Agregar pasajero", function () { abrirPasajero(detalle); }));
            panel.append(boton("Importar pasajeros", function () { abrirImportacion(detalle); }));
            var generar = boton("Generar reservas", function () {
                if (!window.confirm("¿Esta seguro que desea generar las reservas para la solicitud seleccionada?")) return;
                ejecutar(generar, "GenerarReservas", { id: detalle.id }, function (resultado) {
                    d.mostrarAviso("Reservas generadas. Primera reserva N° " + resultado.primeraReservaId + ".", "success");
                    if (resultado.advertenciaCorreo) d.mostrarAviso(resultado.advertenciaCorreo, "error");
                    d.cargar("solicitud");
                    abrirDetalle(detalle.id);
                }, panel);
            });
            generar.disabled = true;
            panel.append(generar);
        }
        if (detalle.estado === "R") {
            var notificar = boton("Enviar correo de reservas", function () {
                ejecutar(notificar, "NotificarReservas", { id: detalle.id },
                    function () { d.mostrarAviso("Correo enviado.", "success"); }, panel);
            });
            panel.append(notificar);
        }
        var titulo = document.createElement("h3");
        titulo.textContent = "Pasajeros y cupos (" + pasajeros.length + ")";
        panel.append(titulo);
        var seleccion = new Set();
        var tabla = document.createElement("table");
        var encabezado = document.createElement("tr");
        var seleccionarTodos = document.createElement("input");
        seleccionarTodos.type = "checkbox";
        seleccionarTodos.setAttribute("aria-label", "Seleccionar todos los pasajeros");
        var celdaSeleccion = document.createElement("th");
        if (detalle.estado === "P") celdaSeleccion.append(seleccionarTodos);
        encabezado.append(celdaSeleccion);
        ["Habitación", "Identificador", "Tipo identificador", "Nombre", "Acciones"].forEach(function (texto) {
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
        function actualizarSeleccion() {
            if (detalle.estado !== "P") return;
            generar.disabled = seleccion.size === 0;
            eliminarSeleccionados.disabled = seleccion.size === 0;
            asignarSeleccionados.disabled = seleccion.size === 0;
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
                        await d.enviar(base + "EliminarPasajero", { id: detalle.id, pasajeroId: id });
                        realizados++;
                    }
                    d.mostrarAviso("Pasajeros eliminados: " + realizados + ".", "success");
                } catch (ex) {
                    d.mostrarAviso("Eliminados antes del error: " + realizados + ". " + ex.message, "error");
                }
                abrirDetalle(detalle.id);
            });
            asignarSeleccionados = boton("Asignar habitaciones", function () {
                abrirHabitacionesMultiples(detalle, pasajeros.filter(function (p) { return seleccion.has(p.id); }));
            });
            panel.append(eliminarSeleccionados, asignarSeleccionados);
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
            }
            fila.append(tdSeleccion);
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
                    ejecutar(eliminar, "EliminarPasajero", { id: detalle.id, pasajeroId: pasajero.id }, function () { abrirDetalle(detalle.id); }, panel);
                });
                acciones.append(eliminar);
            }
            fila.append(acciones);
            cuerpo.append(fila);
        });
        panel.append(tabla);
        d.abrirPanel("Solicitud N° " + detalle.id, panel);
    }
    function abrirDetalle(id) {
        Promise.all([d.pedir(base + "Detalle&id=" + encodeURIComponent(id)),
            d.pedir(base + "Pasajeros&id=" + encodeURIComponent(id))])
            .then(function (r) { mostrarDetalle(r[0], r[1].pasajeros); })
            .catch(function (ex) { d.mostrarAviso(ex.message, "error"); });
    }
    function abrirPasajero(detalle, pasajero) {
        d.pedir(base + "Catalogo&grupoId=" + encodeURIComponent(detalle.areaEmpresaSolicitanteId)).then(function (catalogo) {
            var form = document.createElement("form");
            form.className = "formulario";
            var error = document.createElement("p");
            error.dataset.error = "";
            form.append(error);
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
            form.append(guardar);
            form.addEventListener("submit", function (e) {
                e.preventDefault();
                if (!form.reportValidity()) return;
                if (porCantidad && porCantidad.checked) {
                    ejecutar(guardar, "AgregarCupos", { id: detalle.id, cantidad: Number(cantidad.value) },
                        function () { abrirDetalle(detalle.id); }, form);
                    return;
                }
                ejecutar(guardar, "GuardarPasajero", { solicitudId: detalle.id, pasajeroId: pasajero ? pasajero.id : 0,
                    usuarioPasajeroId: Number(usuario.value), fechaCheckIn: desde.value, fechaCheckOut: hasta.value || null },
                    function () { abrirDetalle(detalle.id); }, form);
            });
            d.abrirPanel("Pasajero de solicitud " + detalle.id, form);
        }).catch(function (ex) { d.mostrarAviso(ex.message, "error"); });
    }
    function abrirHabitacionesMultiples(detalle, pasajeros) {
        d.pedir(base + "HabitacionesDisponibles&id=" + detalle.id).then(function (resultado) {
            var form = document.createElement("form");
            form.className = "formulario";
            var error = document.createElement("p");
            error.dataset.error = "";
            error.setAttribute("role", "alert");
            form.append(error);
            var modulo = campo(form, "Seleccione módulo", "modulo", "select");
            var piso = campo(form, "Seleccione piso", "piso", "select");
            var habitaciones = resultado.habitaciones.filter(function (h) { return h.cantidadCamasDisponibles > 0; });
            function opcionesUnicas(lista, id, nombre) {
                return Array.from(new Map(lista.map(function (h) { return [h[id], { id: h[id], nombre: h[nombre] }]; })).values());
            }
            llenar(modulo, opcionesUnicas(habitaciones, "pabellonId", "pabellonName"));
            var lista = document.createElement("div");
            var seleccionadas = new Set();
            function mostrarHabitaciones() {
                lista.replaceChildren();
                habitaciones.filter(function (h) {
                    return (!modulo.value || String(h.pabellonId) === modulo.value) &&
                        (!piso.value || String(h.moduloId) === piso.value);
                }).forEach(function (h) {
                    var label = document.createElement("label");
                    var check = document.createElement("input");
                    check.type = "checkbox";
                    check.checked = seleccionadas.has(h.habitacionId);
                    check.addEventListener("change", function () {
                        if (check.checked && seleccionadas.size >= pasajeros.length) {
                            check.checked = false;
                            return;
                        }
                        if (check.checked) seleccionadas.add(h.habitacionId);
                        else seleccionadas.delete(h.habitacionId);
                        contador.textContent = "Habitaciones seleccionadas: " + seleccionadas.size + " de " + pasajeros.length;
                    });
                    label.append(check, document.createTextNode(" " + h.pabellonName + " · " + h.moduloName + " · " +
                        h.habitacionName + " (" + h.cantidadCamasDisponibles + " camas disponibles)"));
                    lista.append(label);
                });
            }
            modulo.addEventListener("change", function () {
                var filtradas = habitaciones.filter(function (h) { return !modulo.value || String(h.pabellonId) === modulo.value; });
                llenar(piso, opcionesUnicas(filtradas, "moduloId", "moduloName"));
                mostrarHabitaciones();
            });
            piso.addEventListener("change", mostrarHabitaciones);
            llenar(piso, opcionesUnicas(habitaciones, "moduloId", "moduloName"));
            var contador = document.createElement("p");
            contador.textContent = "Habitaciones seleccionadas: 0 de " + pasajeros.length;
            var asignar = document.createElement("button");
            asignar.type = "submit";
            asignar.className = "boton-primario";
            asignar.textContent = "Asignar";
            form.append(contador, lista, asignar, boton("Cancelar", function () { abrirDetalle(detalle.id); }));
            mostrarHabitaciones();
            form.addEventListener("submit", async function (e) {
                e.preventDefault();
                if (seleccionadas.size !== pasajeros.length) {
                    error.textContent = "Seleccione una habitación para cada pasajero.";
                    return;
                }
                var ids = Array.from(seleccionadas);
                form.querySelectorAll("button, input, select").forEach(function (control) { control.disabled = true; });
                var realizados = 0;
                try {
                    for (var i = 0; i < pasajeros.length; i++) {
                        await d.enviar(base + "AsignarHabitacion", { solicitudId: detalle.id,
                            pasajeroId: pasajeros[i].id, habitacionId: ids[i] });
                        realizados++;
                    }
                    d.mostrarAviso("Habitaciones asignadas: " + realizados + ".", "success");
                } catch (ex) {
                    d.mostrarAviso("Asignaciones realizadas antes del error: " + realizados + ". " + ex.message, "error");
                }
                abrirDetalle(detalle.id);
            });
            d.abrirPanel("Asignar habitaciones", form);
        }).catch(function (ex) { d.mostrarAviso(ex.message, "error"); });
    }
    function abrirImportacion(detalle) {
        var form = document.createElement("form");
        form.className = "formulario";
        var aviso = document.createElement("p");
        aviso.textContent = "La importación puede crear usuarios y actualizar nombres, género, grupo y empresa de usuarios existentes. Revise las filas antes de continuar.";
        var plantilla = document.createElement("a");
        plantilla.href = base + "PlantillaPasajeros";
        plantilla.textContent = "Descargar plantilla";
        var archivo = campo(form, "Planilla de pasajeros (.xlsx)", "archivo", "file", null, true);
        archivo.accept = ".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var resultado = document.createElement("div");
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
            d.enviarArchivo(base + "CargarPasajeros&id=" + detalle.id, archivo.files[0]).then(function (datos) {
                var resumen = document.createElement("p");
                resumen.textContent = "Procesadas: " + datos.filas.filter(function (x) { return x.procesada; }).length +
                    " de " + datos.filas.length + ".";
                resultado.append(resumen);
                var lista = document.createElement("ul");
                datos.filas.filter(function (x) { return !x.procesada; }).forEach(function (x) {
                    var item = document.createElement("li");
                    item.textContent = "Fila " + x.numeroFila + ": " + x.mensaje;
                    lista.append(item);
                });
                resultado.append(lista);
            }).catch(function (ex) { resultado.textContent = ex.message; })
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
    return { filtros: filtros, filtroInicial: filtroInicial, urlBuscar: urlBuscar,
        fila: fila, columnas: columnas, abrirNueva: function () { abrirFormulario(null); } };
}
