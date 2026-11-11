export function crearReserva(dependencias) {
    function filtros() {
        var catalogo = dependencias.obtenerCatalogo() || { grupos: [], origenes: [], pasajeros: [], habitaciones: [], turnos: [] };
        return [
            dependencias.campo("grupo", "Grupo Empresa", "select", dependencias.aOpcion(catalogo.grupos)),
            dependencias.campo("origen", "Origen reserva", "select", dependencias.aOpcion(catalogo.origenes)),
            dependencias.campo("solicitud", "# Solicitud"),
            dependencias.campo("estado", "Estado", "select", [
                { value: "A", text: "Asignada" },
                { value: "I", text: "Asignada con Checkin" },
                { value: "O", text: "Asignada con Checkout" },
                { value: "N", text: "Anulada" },
                { value: "F", text: "Finalizada" }
            ]),
            dependencias.campo("reserva", "Fecha Reserva", "date"),
            dependencias.campo("llegada", "Fecha Llegada", "date"),
            dependencias.campo("salida", "Fecha Salida", "date"),
            dependencias.campo("checkin", "Check In", "date"),
            dependencias.campo("checkout", "Check Out", "date"),
            dependencias.campo("pasajero", "Pasajero", "select", dependencias.aOpcion(catalogo.pasajeros)),
            dependencias.campo("habitacion", "Habitación", "select", dependencias.aOpcion(catalogo.habitaciones)),
            dependencias.campo("agua", "Tiene Agua?", "select", [{ value: "1", text: "SI" }, { value: "2", text: "NO" }])
        ];
    }

    function filtroInicial(modo) {
        var rango = dependencias.rangoReservaInicial();
        if (modo === "checkin") {
            rango.estado = "A";
            return rango;
        }
        if (modo === "checkout") {
            rango.estado = "I";
            return rango;
        }
        if (modo === "reserva") return rango;
        return null;
    }

    function formulario() {
        var catalogo = dependencias.obtenerCatalogo() || { grupos: [], origenes: [], pasajeros: [], habitaciones: [], turnos: [] };
        var origen = (catalogo.origenes || []).filter(function (opcion) { return opcion.id === "R"; })[0];
        return [
            dependencias.seccion("Datos", [
                dependencias.campo("numero", "Número", "text", [], { soloLectura: true }),
                dependencias.campo("grupo", "Grupo Empresa", "select", dependencias.aOpcion(catalogo.grupos), { requerido: true }),
                dependencias.campo("origen", "Origen reserva", "text", [], { soloLectura: true, valor: origen ? origen.etiqueta : "R" }),
                dependencias.campo("solicitud", "# Solicitud", "text", [], { soloLectura: true, valor: "0" })
            ]),
            dependencias.seccion("Estadía", [
                dependencias.campo("reserva", "Fecha Reserva", "datetime-local", [], { requerido: true, valor: dependencias.momentoLocal(true) }),
                dependencias.campo("llegada", "Fecha Llegada", "datetime-local", [], { requerido: true, valor: dependencias.momentoLocal(true) }),
                dependencias.campo("salida", "Fecha Salida", "datetime-local"),
                dependencias.campo("turno", "Turno", "select", dependencias.aOpcion(catalogo.turnos), { requerido: true }),
                dependencias.campo("estado", "Estado", "text", [], { soloLectura: true, valor: "P" })
            ]),
            dependencias.seccion("", [
                dependencias.campo("observaciones", "Observaciones", "textarea")
            ])
        ];
    }

    function abrirImportacion(id) {
        var contenido = document.createElement("div");
        var ayuda = document.createElement("p");
        ayuda.textContent = "Complete la plantilla Excel. Las filas con errores se informarán sin impedir la importación de las demás.";
        var detalle = document.createElement("p");
        detalle.textContent = "Use los nombres exactos de Grupo Empresa y Turno del catálogo. Ingrese las fechas como AAAA-MM-DD HH:mm.";
        var plantilla = document.createElement("a");
        plantilla.href = "/Reserva/PlantillaReservas";
        plantilla.className = "boton-secundario";
        plantilla.textContent = "Descargar plantilla";
        var formularioImportacion = document.createElement("form");
        formularioImportacion.className = "formulario";
        var campoArchivo = document.createElement("label");
        campoArchivo.className = "campo";
        var etiquetaArchivo = document.createElement("span");
        etiquetaArchivo.textContent = "Planilla Excel (.xlsx)";
        var archivo = document.createElement("input");
        archivo.type = "file";
        archivo.name = "archivo";
        archivo.accept = ".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        archivo.required = true;
        campoArchivo.append(etiquetaArchivo, archivo);
        var enviar = document.createElement("button");
        enviar.type = "submit";
        enviar.className = "boton-primario";
        enviar.textContent = "Importar reservas";
        var resultado = document.createElement("div");
        resultado.setAttribute("aria-live", "polite");
        formularioImportacion.append(campoArchivo, enviar, resultado);
        formularioImportacion.addEventListener("submit", function (evento) {
            evento.preventDefault();
            if (!archivo.files || archivo.files.length === 0) return;
            enviar.disabled = true;
            resultado.replaceChildren();
            dependencias.enviarArchivo("/Reserva/Mid/MidReservas.cs", archivo.files[0])
                .then(function (respuesta) {
                    if (respuesta.error) {
                        resultado.textContent = respuesta.error;
                        return;
                    }
                    var creadas = respuesta.filas.filter(function (fila) { return fila.reservaId !== null; }).length;
                    var rechazadas = respuesta.filas.filter(function (fila) { return fila.error; });
                    var resumen = document.createElement("p");
                    resumen.textContent = "Reservas creadas: " + creadas + " de " + respuesta.filasLeidas + ".";
                    resultado.append(resumen);
                    if (respuesta.error) {
                        var errorGeneral = document.createElement("p");
                        errorGeneral.textContent = respuesta.error;
                        resultado.append(errorGeneral);
                    }
                    if (rechazadas.length > 0) {
                        var titulo = document.createElement("p");
                        titulo.textContent = "Filas rechazadas:";
                        var lista = document.createElement("ul");
                        rechazadas.forEach(function (fila) {
                            var elemento = document.createElement("li");
                            elemento.textContent = "Fila " + fila.numeroFila + ": " + fila.error;
                            lista.append(elemento);
                        });
                        resultado.append(titulo, lista);
                    }
                    if (creadas > 0) {
                        dependencias.cargar(id).catch(function (error) {
                            var errorActualizacion = document.createElement("p");
                            errorActualizacion.textContent = error.message;
                            resultado.append(errorActualizacion);
                        });
                    }
                })
                .catch(function (error) {
                    resultado.textContent = error.message;
                })
                .finally(function () { enviar.disabled = false; });
        });
        contenido.append(ayuda, detalle, plantilla, formularioImportacion);
        dependencias.abrirPanel("Importación masiva de reservas", contenido);
    }

    function abrirEdicion(fila) {
        var catalogo = dependencias.obtenerCatalogo() || { grupos: [], pasajeros: [], habitaciones: [], turnos: [] };
        var fecha = dependencias.momentoLocal(false);
        var bloqueada = fila.estadoNumero === "F" || fila.estadoNumero === "N";
        var habitaciones = dependencias.aOpcion(catalogo.habitaciones);
        if (habitaciones.length === 1) {
            habitaciones.push({ value: "", text: "No hay habitaciones reservables", deshabilitado: true });
        }
        var form = dependencias.formulario([
            dependencias.seccion("Asignación", [
                dependencias.campo("areaId", "Grupo Empresa", "select", dependencias.aOpcion(catalogo.grupos), { sinVacio: false, deshabilitado: true, valor: fila.areaId }),
                dependencias.campo("pasajeroId", "Nombre", "select", dependencias.aOpcion(catalogo.pasajeros), { requerido: true, deshabilitado: bloqueada, valor: fila.pasajeroId || "" }),
                dependencias.campo("habitacionId", "Habitación", "select", habitaciones, { deshabilitado: fila.estadoNumero !== "P" && catalogo.habitaciones.length > 0, valor: fila.habitacionId || "" }),
                dependencias.campo("turnoId", "Turno", "select", dependencias.aOpcion(catalogo.turnos), { deshabilitado: true, valor: fila.turnoId || "" })
            ]),
            dependencias.seccion("Estado", [
                dependencias.campo("estado", "Estado", "select", [
                    { value: "P", text: "Pre-reserva" },
                    { value: "A", text: "Asignada" },
                    { value: "F", text: "Finalizada" },
                    { value: "N", text: "Anulada" }
                ], { deshabilitado: true, valor: fila.estadoNumero || "P" }),
                dependencias.campo("fechaReserva", "Fecha Reserva", "date", [], { valor: fecha }),
                dependencias.campo("fechaLlegada", "Fecha Llegada", "date", [], { valor: fecha }),
                dependencias.campo("fechaSalida", "Fecha Salida", "date"),
                dependencias.campo("observacion", "Observación", "textarea")
            ])
        ], "Guardar", true, function (datos) {
            return dependencias.enviar("/Reserva/Mid/MidReservas.cs", {
                opcion: "editar",
                reservaId: fila.id,
                areaId: dependencias.numero(form.querySelector("[name='areaId']").value),
                turnoId: dependencias.numero(datos.turnoId) || fila.turnoId,
                fechaReserva: datos.fechaReserva || fecha,
                fechaLlegada: datos.fechaLlegada || fecha,
                fechaSalida: datos.fechaSalida,
                observacion: datos.observacion,
                pasajeroId: dependencias.numero(form.querySelector("[name='pasajeroId']").value) || null,
                habitacionId: dependencias.numero(datos.habitacionId) || null,
                estado: datos.estado || fila.estadoNumero || "P",
                tieneAgua: false
            }).then(function () {
                var activo = document.querySelector(".modulo:not([hidden])");
                return activo ? dependencias.cargar(activo.dataset.id) : null;
            });
        });
        dependencias.abrirPanel("Editar reserva " + fila.id, form);
    }

    function crear(datos, id) {
        return dependencias.enviar("/Reserva/Mid/MidReservas.cs", {
            opcion: "crear",
            areaId: dependencias.numero(datos.grupo),
            turnoId: dependencias.numero(datos.turno),
            fechaReserva: datos.reserva,
            fechaLlegada: datos.llegada,
            fechaSalida: datos.salida,
            observacion: datos.observaciones
        }).then(function (resultado) {
            return dependencias.cargar(id).then(function () {
                dependencias.mostrarAviso("Reserva creada N° " + resultado.id + ".", "success");
            });
        });
    }

    function buscar(filtro) {
        return dependencias.enviar("/Reserva/Mid/MidReservas.cs", {
            opcion: "listar",
            areaId: filtro.grupo,
            origen: filtro.origen,
            solicitudId: filtro.solicitud,
            estado: filtro.estado,
            fechaReserva: filtro.reserva,
            fechaReservaDesde: filtro.reservaDesde,
            fechaReservaHasta: filtro.reservaHasta,
            fechaLlegada: filtro.llegada,
            fechaSalida: filtro.salida,
            checkIn: filtro.checkin,
            checkOut: filtro.checkout,
            pasajeroId: filtro.pasajero,
            habitacionId: filtro.habitacion,
            tieneAgua: filtro.agua
        });
    }

    function fila(fila) {
        return {
            acciones: fila,
            numero: fila.id,
            fechaReserva: fila.fechaReserva,
            grupo: fila.grupoEmpresa,
            pasajero: fila.pasajero,
            habitacion: fila.habitacion,
            estado: fila.estado,
            llegada: fila.llegada,
            salida: fila.salida,
            _fila: fila
        };
    }

    function columnas(modo, crearAcciones) {
        return ["acciones", "numero", "fechaReserva", "grupo", "pasajero", "habitacion", "estado", "llegada", "salida"].map(function (clave) {
            if (clave !== "acciones") return { data: clave };
            return {
                data: clave,
                orderable: false,
                createdCell: function (td, fila) {
                    td.replaceChildren(crearAcciones(fila, modo));
                }
            };
        });
    }

    return {
        filtros: filtros,
        filtroInicial: filtroInicial,
        formulario: formulario,
        abrirImportacion: abrirImportacion,
        abrirEdicion: abrirEdicion,
        crear: crear,
        buscar: buscar,
        fila: fila,
        columnas: columnas
    };
}
