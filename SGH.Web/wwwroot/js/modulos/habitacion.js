import { renderizadorCeldaTruncada } from "../core/celda-truncada.js";

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
    function mid(opcion, datos, enviarMid) {
        var enviar = enviarMid || dependencias.enviar;
        return enviar(rutaMid, Object.assign({ opcion: opcion }, datos || {}));
    }

    function obtenerCatalogo() {
        return dependencias.obtenerCatalogoHabitacion ? dependencias.obtenerCatalogoHabitacion() : null;
    }

    var recargaSinBloqueo = { bloquearPantalla: false };

    function recargarModuloVisible() {
        var activo = document.querySelector(".modulo:not([hidden])");
        return activo ? dependencias.cargar(activo.dataset.id, recargaSinBloqueo) : null;
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

    function filtrosTiposCama() {
        return [
            dependencias.campo("soloActivos", "Solo activos", "select", [
                { value: "1", text: "Sí" },
                { value: "0", text: "No (todos)" }
            ], { valor: "0" })
        ];
    }

    function filtros(modo) {
        if (modo === "banos") return filtrosBanos();
        if (modo === "tiposcama") return filtrosTiposCama();
        return filtrosHabitaciones();
    }

    function filtroInicial(modo) {
        if (modo === "banos" || modo === "tiposcama") return { soloActivos: 0 };
        return { soloActivas: 0 };
    }

    function buscarHabitaciones(filtro, enviarMid) {
        return mid("listar", {
            pabellonId: dependencias.numero(filtro.pabellonId) || null,
            moduloId: dependencias.numero(filtro.moduloId) || null,
            estado: filtro.estado || null,
            soloActivas: dependencias.numero(filtro.soloActivas) ?? 0
        }, enviarMid);
    }

    function buscarBanos(filtro, enviarMid) {
        return mid("listarbanos", {
            soloActivos: dependencias.numero(filtro.soloActivos) ?? 0
        }, enviarMid);
    }

    function buscarTiposCama(filtro, enviarMid) {
        return mid("listartiposcama", {
            soloActivos: dependencias.numero(filtro.soloActivos) ?? 0
        }, enviarMid);
    }

    function buscar(filtro, modo, enviarMid) {
        if (modo === "banos") return buscarBanos(filtro, enviarMid);
        if (modo === "tiposcama") return buscarTiposCama(filtro, enviarMid);
        return buscarHabitaciones(filtro, enviarMid);
    }

    function fechaCampo(valor) {
        if (!valor) return "";
        var s = String(valor);
        return s.length >= 10 ? s.substring(0, 10) : s;
    }

    function opcionesTiposCama(catalogo) {
        return dependencias.aOpcion(catalogo.tiposCama || []);
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

    function filaTipoCama(fila) {
        var medidas = (fila.anchoCm || 0) + " × " + (fila.largoCm || 0) + " cm";
        return {
            acciones: fila,
            nombre: fila.nombre || "",
            medidas: medidas,
            camas: fila.camasVinculadas || 0,
            estado: fila.isActive ? "Activo" : "Inactivo",
            _fila: fila
        };
    }

    function fila(origen, modo) {
        if (modo === "banos") return filaBano(origen);
        if (modo === "tiposcama") return filaTipoCama(origen);
        return filaHabitacion(origen);
    }

    function columnaDatos(clave) {
        return { data: clave, render: renderizadorCeldaTruncada() };
    }

    function columnas(modo, crearAcciones) {
        if (modo === "tiposcama") {
            return ["acciones", "nombre", "medidas", "camas", "estado"].map(function (clave) {
                if (clave !== "acciones") return columnaDatos(clave);
                return {
                    data: clave,
                    orderable: false,
                    createdCell: function (td, cellData, rowData) {
                        td.replaceChildren(crearAcciones(rowData || cellData, modo));
                    }
                };
            });
        }
        if (modo === "banos") {
            return ["acciones", "codigo", "ubicacion", "sexo", "estado", "habitaciones"].map(function (clave) {
                if (clave !== "acciones") return columnaDatos(clave);
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
            if (clave !== "acciones") return columnaDatos(clave);
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
        var estados = opcionesEstado(catalogo);
        var estadoDefecto = fila && fila.estado
            ? fila.estado
            : (estados[0] ? estados[0].value : "D");
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
                dependencias.campo("estado", "Estado", "select", estados, {
                    sinVacio: estados.length > 0,
                    valor: estadoDefecto
                }),
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
                return recargarModuloVisible();
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
                return recargarModuloVisible();
            }).then(function () {
                dependencias.mostrarAviso("Baño guardado.", "success");
            });
        });
        dependencias.abrirPanel(titulo, form);
    }

    function inactivarHabitacion(id) {
        return mid("inactivar", { id: id }).then(function () {
            return recargarModuloVisible();
        }).then(function () {
            dependencias.mostrarAviso("Habitación inactivada.", "success");
        });
    }

    function inactivarBano(id) {
        return mid("inactivarbano", { id: id }).then(function () {
            if (dependencias.invalidarCatalogoHabitacion) dependencias.invalidarCatalogoHabitacion();
            return recargarModuloVisible();
        }).then(function () {
            dependencias.mostrarAviso("Baño inactivado.", "success");
        });
    }

    function activarBano(id) {
        return mid("activarbano", { id: id }).then(function () {
            if (dependencias.invalidarCatalogoHabitacion) dependencias.invalidarCatalogoHabitacion();
            return recargarModuloVisible();
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

    function tipoCamaEstaActivo(fila) {
        return fila.isActive === true || fila.isActive === 1;
    }

    function abrirNuevoTipoCama() {
        abrirFormularioTipoCama("creartipocama", null);
    }

    function abrirEdicionTipoCama(fila) {
        abrirFormularioTipoCama("editartipocama", fila);
    }

    function abrirFormularioTipoCama(opcion, fila) {
        var esAlta = opcion === "creartipocama";
        var titulo = esAlta ? "Nuevo tipo de cama" : "Editar tipo de cama";
        var form = dependencias.formulario([
            dependencias.seccion("", [
                dependencias.campo("nombre", "Nombre (ej. 1 plaza)", "text", [], {
                    requerido: true,
                    valor: fila ? fila.nombre : ""
                }),
                dependencias.campo("anchoCm", "Ancho (cm)", "number", [], {
                    requerido: true,
                    valor: fila ? fila.anchoCm : ""
                }),
                dependencias.campo("largoCm", "Largo (cm)", "number", [], {
                    requerido: true,
                    valor: fila ? fila.largoCm : ""
                })
            ])
        ], "Guardar", true, function (datos) {
            return mid(opcion, {
                id: fila ? fila.id : 0,
                nombre: (datos.nombre || "").trim(),
                anchoCm: dependencias.numero(datos.anchoCm),
                largoCm: dependencias.numero(datos.largoCm)
            }).then(function () {
                if (dependencias.invalidarCatalogoHabitacion) dependencias.invalidarCatalogoHabitacion();
                return recargarModuloVisible();
            }).then(function () {
                dependencias.mostrarAviso("Tipo de cama guardado.", "success");
            });
        });
        dependencias.abrirPanel(titulo, form);
    }

    function inactivarTipoCama(id) {
        return mid("inactivartipocama", { id: id }).then(function () {
            if (dependencias.invalidarCatalogoHabitacion) dependencias.invalidarCatalogoHabitacion();
            return recargarModuloVisible();
        }).then(function () {
            dependencias.mostrarAviso("Tipo de cama inactivado.", "success");
        });
    }

    function activarTipoCama(id) {
        return mid("activartipocama", { id: id }).then(function () {
            if (dependencias.invalidarCatalogoHabitacion) dependencias.invalidarCatalogoHabitacion();
            return recargarModuloVisible();
        }).then(function () {
            dependencias.mostrarAviso("Tipo de cama activado.", "success");
        });
    }

    function alternarEstadoTipoCama(fila) {
        if (tipoCamaEstaActivo(fila)) return inactivarTipoCama(fila.id);
        return activarTipoCama(fila.id);
    }

    function abrirCamasHabitacion(fila) {
        dependencias.prepararCatalogo("habitaciones").then(function () {
            var catalogo = obtenerCatalogo() || { tiposCama: [] };
            return mid("listarcamas", { habitacionId: fila.id, soloActivas: 0 }).then(function (resultado) {
                var camas = resultado.filas || [];
                var contenedor = document.createElement("div");
                contenedor.className = "panel-camas-habitacion";
                var tituloHab = document.createElement("p");
                tituloHab.textContent = "Habitación " + (fila.nombre || fila.id) + " — inventario por plaza";
                contenedor.appendChild(tituloHab);
                if (!camas.length) {
                    var vacio = document.createElement("p");
                    vacio.textContent = "No hay camas activas. Guarde la habitación con N° camas para generarlas.";
                    contenedor.appendChild(vacio);
                }
                camas.forEach(function (cama) {
                    var tarjeta = document.createElement("div");
                    tarjeta.className = "tarjeta-cama";
                    var resumen = document.createElement("p");
                    var tipo = cama.tipoCamaNombre
                        ? cama.tipoCamaNombre + " (" + (cama.tipoCamaAnchoCm || "?") + "×" + (cama.tipoCamaLargoCm || "?") + " cm)"
                        : "Sin tipo asignado";
                    var estadoPlaza = cama.isActive === false || cama.isActive === 0 ? " — Plaza inactiva (IsActive)" : "";
                    resumen.textContent = (cama.codigoCama || "Cama") + " — " + tipo
                        + (cama.habilitada ? " — Habilitada" : " — No habilitada")
                        + estadoPlaza;
                    tarjeta.appendChild(resumen);
                    var editar = dependencias.boton ? dependencias.boton("Editar inventario", "accion-fila", function () {
                        abrirFormularioCama(fila, cama, catalogo);
                    }) : document.createElement("button");
                    if (!dependencias.boton) {
                        editar.textContent = "Editar inventario";
                        editar.addEventListener("click", function () { abrirFormularioCama(fila, cama, catalogo); });
                    }
                    tarjeta.appendChild(editar);
                    contenedor.appendChild(tarjeta);
                });
                dependencias.abrirPanel("Camas de la habitación", contenedor);
            });
        }).catch(function (error) { dependencias.mostrarAviso(error.message, "error"); });
    }

    function abrirFormularioCama(habitacion, cama, catalogo) {
        var tiposOpts = opcionesTiposCama(catalogo);
        var form = dependencias.formulario([
            dependencias.seccion("Plaza", [
                dependencias.campo("codigoCama", "Código", "text", [], {
                    requerido: true,
                    valor: cama.codigoCama || ""
                }),
                dependencias.campo("tipoCamaId", "Tipo de cama", "select", tiposOpts, {
                    valor: cama.tipoCamaId ? String(cama.tipoCamaId) : ""
                }),
                dependencias.campo("habilitada", "Habilitada (operativa)", "select", [
                    { value: "1", text: "Sí" },
                    { value: "0", text: "No" }
                ], { valor: cama.habilitada ? "1" : "0" }),
                dependencias.campo("plazaActiva", "Plaza activa (IsActive)", "select", [
                    { value: "1", text: "Sí — en inventario de la habitación" },
                    { value: "0", text: "No — baja lógica" }
                ], { valor: cama.isActive === false || cama.isActive === 0 ? "0" : "1" })
            ]),
            dependencias.seccion("Colchón", [
                dependencias.campo("fechaColchon", "Fecha cambio colchón", "date", [], {
                    valor: fechaCampo(cama.fechaColchon)
                }),
                dependencias.campo("colchonObservacion", "Observación colchón", "textarea", [], {
                    valor: cama.colchonObservacion || ""
                })
            ]),
            dependencias.seccion("Ropa de cama", [
                dependencias.campo("fechaCambioRopa", "Fecha cambio ropa", "date", [], {
                    valor: fechaCampo(cama.fechaCambioRopa)
                }),
                dependencias.campo("ropaCamaObservacion", "Observación ropa", "textarea", [], {
                    valor: cama.ropaCamaObservacion || ""
                })
            ])
        ], "Guardar", true, function (datos) {
            return mid("guardarcama", {
                id: cama.id,
                habitacionId: habitacion.id,
                codigoCama: (datos.codigoCama || "").trim(),
                tipoCamaId: dependencias.numero(datos.tipoCamaId) || null,
                habilitada: datos.habilitada === "1",
                inactivo: datos.plazaActiva === "0",
                orden: cama.orden || 0,
                fechaColchon: datos.fechaColchon || null,
                colchonObservacion: (datos.colchonObservacion || "").trim() || null,
                fechaCambioRopa: datos.fechaCambioRopa || null,
                ropaCamaObservacion: (datos.ropaCamaObservacion || "").trim() || null
            }).then(function () {
                dependencias.mostrarAviso("Cama guardada.", "success");
                return abrirCamasHabitacion(habitacion);
            });
        });
        dependencias.abrirPanel("Editar cama — " + (habitacion.nombre || ""), form);
    }

  function pintarResultadoImportacion(contenedor, datos) {
        contenedor.replaceChildren();
        var rechazadas = (datos.filas || []).filter(function (fila) { return !fila.procesada; });
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
        contenedor.append(titulo, resumen);
        if (!rechazadas.length) {
            var completo = document.createElement("p");
            completo.className = "importacion-completa";
            completo.textContent = "No hay filas pendientes de revisión.";
            contenedor.append(completo);
            return;
        }
        dependencias.mostrarAviso("Hay filas de la importación pendientes de revisión.", "error");
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
        contenedor.append(subtitulo, lista);
    }

    function abrirImportacionCamas() {
        var form = document.querySelector("form[data-formulario='importacion-camas']");
        if (!form) {
            dependencias.mostrarAviso("No se encontró el formulario de importación en la página.", "error");
            return;
        }
        var clon = form.cloneNode(true);
        clon.hidden = false;
        clon.removeAttribute("aria-hidden");
        var resultado = clon.querySelector(".importacion-resultado");
        var importar = clon.querySelector("button[type='submit']");
        clon.querySelector("[data-accion='cerrar-panel']").addEventListener("click", dependencias.cerrarPanel);
        clon.addEventListener("submit", function (evento) {
            evento.preventDefault();
            if (!clon.reportValidity()) return;
            var archivoInput = clon.querySelector("input[name='archivo']");
            if (!archivoInput.files[0]) return;
            importar.disabled = true;
            resultado.replaceChildren();
            dependencias.enviarArchivo(rutaMid, archivoInput.files[0], {}).then(function (info) {
                var datos = info.registros || info;
                pintarResultadoImportacion(resultado, datos);
                var activo = document.querySelector(".modulo.modulo-habitaciones:not([hidden])");
                if (activo) dependencias.cargar(activo.dataset.id, recargaSinBloqueo);
            }).catch(function (error) {
                dependencias.mostrarAviso(error.message, "error");
            }).finally(function () { importar.disabled = false; });
        });
        dependencias.abrirPanel("Carga masiva de camas", clon);
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
        alternarEstadoBano: alternarEstadoBano,
        abrirNuevoTipoCama: abrirNuevoTipoCama,
        abrirEdicionTipoCama: abrirEdicionTipoCama,
        alternarEstadoTipoCama: alternarEstadoTipoCama,
        abrirCamasHabitacion: abrirCamasHabitacion,
        abrirImportacionCamas: abrirImportacionCamas
    };
}