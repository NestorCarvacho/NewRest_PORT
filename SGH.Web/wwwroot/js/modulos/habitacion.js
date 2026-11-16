var rutaMid = "/Habitacion/Mid/MidHabitaciones.cs";

function siNo(valor) {
    return valor ? "Sí" : "No";
}

/** En UI solo MIXTO / HOMBRE / MUJER; el Mid acepta también M/F legados. */
function tipoSexoBanoParaFormulario(codigo) {
    if (!codigo) return "MIXTO";
    var u = String(codigo).trim().toUpperCase();
    if (u === "M" || u === "HOMBRE") return "HOMBRE";
    if (u === "F" || u === "MUJER") return "MUJER";
    if (u === "MIXTO") return "MIXTO";
    return "MIXTO";
}

function opcionesTipoSexoBano() {
    return [
        { value: "MIXTO", text: "Mixto" },
        { value: "HOMBRE", text: "Hombre" },
        { value: "MUJER", text: "Mujer" }
    ];
}

export function crearHabitacion(dependencias) {
    function mid(opcion, datos) {
        return dependencias.enviar(rutaMid, Object.assign({ opcion: opcion }, datos || {}));
    }

    function obtenerCatalogo() {
        return dependencias.obtenerCatalogoHabitacion ? dependencias.obtenerCatalogoHabitacion() : null;
    }

    function opcionesSexo(catalogo) {
        return (catalogo.sexos || []).map(function (s) {
            return { value: s.codigo, text: s.etiqueta };
        });
    }

    function opcionesEstado(catalogo) {
        return (catalogo.estadosHabitacion || []).map(function (s) {
            return { value: s.codigo, text: s.etiqueta };
        });
    }

    function modulosDePabellon(catalogo, pabellonId) {
        return dependencias.aOpcion((catalogo.modulos || []).filter(function (m) {
            return !pabellonId || String(m.padreId) === String(pabellonId);
        }));
    }

    function filtrosHabitaciones() {
        var catalogo = obtenerCatalogo() || { pabellones: [], modulos: [], estadosHabitacion: [] };
        return [
            dependencias.campo("pabellonId", "Pabellón", "select", dependencias.aOpcion(catalogo.pabellones)),
            dependencias.campo("moduloId", "Módulo", "select", dependencias.aOpcion(catalogo.modulos)),
            dependencias.campo("estado", "Estado", "select", opcionesEstado(catalogo)),
            dependencias.campo("soloActivas", "Solo activas", "select", [
                { value: "1", text: "Sí" },
                { value: "0", text: "No (todas)" }
            ], { valor: "0" })
        ];
    }

    function filtrosBanos() {
        return [
            dependencias.campo("soloActivos", "Solo activos", "select", [
                { value: "1", text: "Sí" },
                { value: "0", text: "No (todos)" }
            ], { valor: "0" })
        ];
    }

    function filtros(modo) {
        if (modo === "banos") return filtrosBanos();
        return filtrosHabitaciones();
    }

    function filtroInicial(modo) {
        if (modo === "banos") return { soloActivos: 0 };
        return { soloActivas: 0 };
    }

    function buscarHabitaciones(filtro) {
        return mid("listar", {
            pabellonId: dependencias.numero(filtro.pabellonId) || null,
            moduloId: dependencias.numero(filtro.moduloId) || null,
            estado: filtro.estado || null,
            soloActivas: dependencias.numero(filtro.soloActivas) ?? 0
        });
    }

    function buscarBanos(filtro) {
        return mid("listarbanos", {
            soloActivos: dependencias.numero(filtro.soloActivos) ?? 0
        });
    }

    function buscar(filtro, modo) {
        if (modo === "banos") return buscarBanos(filtro);
        return buscarHabitaciones(filtro);
    }

    function filaHabitacion(fila) {
        return {
            acciones: fila,
            codigo: fila.nombre || "",
            sexo: fila.sexo || "",
            bano: fila.banoNombre || "—",
            pabellon: fila.pabellonNombre || "",
            modulo: fila.moduloNombre || "",
            camas: fila.cantidadCamas,
            habilitadas: (fila.camasHabilitadas || 0) + " / " + (fila.cantidadCamas || 0),
            turnos: siNo(fila.permiteUsoPorTurnos),
            exclusivo: siNo(fila.permiteUsoExclusivo),
            estado: fila.estado || "",
            _fila: fila
        };
    }

    function filaBano(fila) {
        return {
            acciones: fila,
            codigo: fila.codigoNombre || "",
            ubicacion: fila.ubicacion || "",
            sexo: fila.tipoSexo || "",
            estado: fila.estado || "",
            habitaciones: fila.habitacionesVinculadas || 0,
            _fila: fila
        };
    }

    function fila(origen, modo) {
        if (modo === "banos") return filaBano(origen);
        return filaHabitacion(origen);
    }

    function columnas(modo, crearAcciones) {
        if (modo === "banos") {
            return ["acciones", "codigo", "ubicacion", "sexo", "estado", "habitaciones"].map(function (clave) {
                if (clave !== "acciones") return { data: clave, render: DataTable.render.text() };
                return {
                    data: clave,
                    orderable: false,
                    createdCell: function (td, cellData, rowData) {
                        td.replaceChildren(crearAcciones(rowData || cellData, modo));
                    }
                };
            });
        }
        return ["acciones", "codigo", "sexo", "bano", "pabellon", "modulo", "camas", "habilitadas", "turnos", "exclusivo", "estado"].map(function (clave) {
            if (clave !== "acciones") return { data: clave, render: DataTable.render.text() };
            return {
                data: clave,
                orderable: false,
                createdCell: function (td, cellData, rowData) {
                    td.replaceChildren(crearAcciones(rowData || cellData, modo));
                }
            };
        });
    }

    function enlazarPabellonModulo(form, catalogo, pabellonInicial, moduloInicial) {
        var pabellon = form.querySelector("[name='pabellonId']");
        var modulo = form.querySelector("[name='moduloId']");
        if (!pabellon || !modulo) return;
        function actualizarModulos() {
            var opciones = modulosDePabellon(catalogo, pabellon.value);
            modulo.replaceChildren(new Option("Seleccione", ""));
            opciones.forEach(function (op) {
                modulo.add(new Option(op.text, op.value));
            });
            if (moduloInicial && opciones.some(function (o) { return String(o.value) === String(moduloInicial); })) {
                modulo.value = String(moduloInicial);
            }
            dependencias.actualizarSelector(modulo);
        }
        pabellon.addEventListener("change", actualizarModulos);
        if (pabellonInicial) pabellon.value = String(pabellonInicial);
        dependencias.actualizarSelector(pabellon);
        actualizarModulos();
    }

    function abrirNuevaHabitacion() {
        dependencias.prepararCatalogo("habitaciones").then(function () {
            abrirFormularioHabitacion("crear", null);
        }).catch(function (error) { dependencias.mostrarAviso(error.message, "error"); });
    }

    function abrirEdicionHabitacion(fila) {
        abrirFormularioHabitacion("editar", fila);
    }

    function abrirFormularioHabitacion(opcion, fila) {
        var catalogo = obtenerCatalogo() || { pabellones: [], modulos: [], sexos: [], estadosHabitacion: [], banos: [] };
        var banosOpts = dependencias.aOpcion(catalogo.banos || []);
        var esAlta = opcion === "crear";
        var titulo = esAlta ? "Nueva habitación" : "Editar habitación";
        var form = dependencias.formulario([
            dependencias.seccion("Ubicación", [
                dependencias.campo("pabellonId", "Pabellón", "select", dependencias.aOpcion(catalogo.pabellones), {
                    requerido: true,
                    valor: fila ? fila.pabellonId : ""
                }),
                dependencias.campo("moduloId", "Módulo", "select", [], { requerido: true, valor: fila ? fila.moduloId : "" }),
                dependencias.campo("nombre", "Código / nombre", "text", [], { requerido: true, valor: fila ? fila.nombre : "" })
            ]),
            dependencias.seccion("Características", [
                dependencias.campo("sexo", "Sexo", "select", opcionesSexo(catalogo), { requerido: true, valor: fila ? fila.sexo : "" }),
                dependencias.campo("banoId", "Baño", "select", banosOpts, { valor: fila && fila.banoId ? fila.banoId : "" }),
                dependencias.campo("cantidadCamas", "N° camas", "number", [], { requerido: true, valor: fila ? fila.cantidadCamas : 1 }),
                dependencias.campo("estado", "Estado", "select", opcionesEstado(catalogo), { valor: fila ? fila.estado : "ACTIVO" }),
                dependencias.campo("minutosMin", "Minutos higienización", "number", [], { valor: 0 })
            ]),
            dependencias.seccion("Opciones", [
                dependencias.campo("isReservable", "Reservable", "select", [
                    { value: "1", text: "Sí" },
                    { value: "0", text: "No" }
                ], { valor: fila && fila.isReservable === false ? "0" : "1" }),
                dependencias.campo("usoPorTurnos", "Uso por turnos", "select", [
                    { value: "0", text: "No" },
                    { value: "1", text: "Sí" }
                ], { valor: fila && fila.permiteUsoPorTurnos ? "1" : "0" }),
                dependencias.campo("usoExclusivo", "Uso exclusivo", "select", [
                    { value: "0", text: "No" },
                    { value: "1", text: "Sí" }
                ], { valor: fila && fila.permiteUsoExclusivo ? "1" : "0" })
            ])
        ], "Guardar", true, function (datos) {
            var payload = {
                opcion: opcion,
                id: fila ? fila.id : 0,
                moduloId: dependencias.numero(datos.moduloId),
                nombre: (datos.nombre || "").trim(),
                sexo: datos.sexo,
                banoId: dependencias.numero(datos.banoId) || null,
                cantidadCamas: dependencias.numero(datos.cantidadCamas) || 1,
                estado: datos.estado,
                isReservable: datos.isReservable === "1",
                usoPorTurnos: datos.usoPorTurnos === "1",
                usoExclusivo: datos.usoExclusivo === "1",
                minutosMin: dependencias.numero(datos.minutosMin) || 0,
                tiempoHigienizacionMin: dependencias.numero(datos.minutosMin) || 0
            };
            return mid(opcion, payload).then(function () {
                if (dependencias.invalidarCatalogoHabitacion) dependencias.invalidarCatalogoHabitacion();
                var activo = document.querySelector(".modulo:not([hidden])");
                return activo ? dependencias.cargar(activo.dataset.id) : null;
            }).then(function () {
                dependencias.mostrarAviso("Habitación guardada.", "success");
            });
        });
        enlazarPabellonModulo(form, catalogo, fila ? fila.pabellonId : null, fila ? fila.moduloId : null);
        if (!esAlta && fila) {
            mid("obtener", { id: fila.id }).then(function (det) {
                var min = form.querySelector("[name='minutosMin']");
                if (min) min.value = String(det.tiempoHigienizacionMin || det.minutosMin || 0);
                if (det.banoId) {
                    var bano = form.querySelector("[name='banoId']");
                    if (bano) {
                        bano.value = String(det.banoId);
                        dependencias.actualizarSelector(bano);
                    }
                }
            }).catch(function () { });
        }
        dependencias.abrirPanel(titulo, form);
    }

    function abrirNuevoBano() {
        abrirFormularioBano("crearbano", null);
    }

    function abrirEdicionBano(fila) {
        abrirFormularioBano("editarbano", fila);
    }

    function abrirFormularioBano(opcion, fila) {
        var esAlta = opcion === "crearbano";
        var titulo = esAlta ? "Nuevo baño" : "Editar baño";
        var form = dependencias.formulario([
            dependencias.seccion("", [
                dependencias.campo("codigoNombre", "Código", "text", [], { requerido: true, valor: fila ? fila.codigoNombre : "" }),
                dependencias.campo("ubicacion", "Ubicación", "text", [], { requerido: true, valor: fila ? fila.ubicacion : "" }),
                dependencias.campo("tipoSexo", "Tipo sexo", "select", opcionesTipoSexoBano(), {
                    requerido: true,
                    sinVacio: true,
                    valor: tipoSexoBanoParaFormulario(fila ? fila.tipoSexo : "MIXTO")
                })
            ])
        ], "Guardar", true, function (datos) {
            return mid(opcion, {
                id: fila ? fila.id : 0,
                codigoNombre: (datos.codigoNombre || "").trim(),
                ubicacion: (datos.ubicacion || "").trim(),
                tipoSexo: datos.tipoSexo
            }).then(function () {
                if (dependencias.invalidarCatalogoHabitacion) dependencias.invalidarCatalogoHabitacion();
                var activo = document.querySelector(".modulo:not([hidden])");
                return activo ? dependencias.cargar(activo.dataset.id) : null;
            }).then(function () {
                dependencias.mostrarAviso("Baño guardado.", "success");
            });
        });
        dependencias.abrirPanel(titulo, form);
    }

    function inactivarHabitacion(id) {
        return mid("inactivar", { id: id }).then(function () {
            var activo = document.querySelector(".modulo:not([hidden])");
            return activo ? dependencias.cargar(activo.dataset.id) : null;
        }).then(function () {
            dependencias.mostrarAviso("Habitación inactivada.", "success");
        });
    }

    function inactivarBano(id) {
        return mid("inactivarbano", { id: id }).then(function () {
            if (dependencias.invalidarCatalogoHabitacion) dependencias.invalidarCatalogoHabitacion();
            var activo = document.querySelector(".modulo:not([hidden])");
            return activo ? dependencias.cargar(activo.dataset.id) : null;
        }).then(function () {
            dependencias.mostrarAviso("Baño inactivado.", "success");
        });
    }

    function activarBano(id) {
        return mid("activarbano", { id: id }).then(function () {
            if (dependencias.invalidarCatalogoHabitacion) dependencias.invalidarCatalogoHabitacion();
            var activo = document.querySelector(".modulo:not([hidden])");
            return activo ? dependencias.cargar(activo.dataset.id) : null;
        }).then(function () {
            dependencias.mostrarAviso("Baño activado.", "success");
        });
    }

    function banoEstaActivo(fila) {
        var estadoActivo = String(fila.estado || "").toUpperCase() === "ACTIVO";
        if (fila.isActive === true || fila.isActive === 1) return true;
        if (fila.isActive === false || fila.isActive === 0) return estadoActivo;
        return estadoActivo;
    }

    function alternarEstadoBano(fila) {
        if (banoEstaActivo(fila)) return inactivarBano(fila.id);
        return activarBano(fila.id);
    }

    return {
        filtros: filtros,
        filtroInicial: filtroInicial,
        buscar: buscar,
        fila: fila,
        columnas: columnas,
        abrirNuevaHabitacion: abrirNuevaHabitacion,
        abrirNuevoBano: abrirNuevoBano,
        abrirEdicionHabitacion: abrirEdicionHabitacion,
        abrirEdicionBano: abrirEdicionBano,
        inactivarHabitacion: inactivarHabitacion,
        inactivarBano: inactivarBano,
        activarBano: activarBano,
        alternarEstadoBano: alternarEstadoBano
    };
}