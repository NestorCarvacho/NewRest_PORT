function pantalla(titulo, columnas, filtros, secciones, accion, modo) {
    return { titulo: titulo, columnas: columnas, filtros: filtros, secciones: secciones, accion: accion, modo: modo };
}

export function definirPantallasReserva(reserva) {
    var modulos = {
        reserva: pantalla(
            "Reserva",
            ["Acciones", "N°", "Fecha Reserva", "Grupo Empresa", "Pasajero", "Habitación", "Estado", "Llegada", "Salida"],
            reserva.filtros,
            reserva.formulario,
            { titulo: "Nueva reserva", boton: "Nueva reserva", confirmar: "Guardar" },
            "reserva"
        ),
        checkin: pantalla(
            "Check In",
            ["Acciones", "N°", "Fecha Reserva", "Grupo Empresa", "Pasajero", "Habitación", "Estado", "Llegada", "Salida"],
            reserva.filtros,
            null,
            null,
            "checkin"
        ),
        checkout: pantalla(
            "Check Out",
            ["Acciones", "N°", "Fecha Reserva", "Grupo Empresa", "Pasajero", "Habitación", "Estado", "Llegada", "Salida"],
            reserva.filtros,
            null,
            null,
            "checkout"
        )
    };
    modulos.reserva.filtroInicial = function () { return reserva.filtroInicial("reserva"); };
    modulos.checkin.filtroInicial = function () { return reserva.filtroInicial("checkin"); };
    modulos.checkout.filtroInicial = function () { return reserva.filtroInicial("checkout"); };
    return modulos;
}

export function definirPantallasCenso(censo) {
    var modulos = {
        censo: pantalla(
            "Censo",
            ["Nº", "Fecha", "Módulo", "Piso", "Habitación", "Estado Censo", "Estado Habitación", "N° Camas", "Camas ocupadas", "Camas ocupadas (CheckIn)"],
            censo.filtros,
            null,
            { titulo: "Censo", boton: "Guardar", confirmar: "Guardar" },
            "censo"
        ),
        misCensos: pantalla(
            "Mis Censos",
            ["N°", "Fecha", "Campamento", "Módulo", "Piso", "Habitación", "N° Camas", "Camas ocupadas", "Turno"],
            censo.filtrosMisCensos,
            null,
            null,
            "misCensos"
        )
    };
    modulos.censo.filtroInicial = function () { return censo.filtroInicial("censo"); };
    modulos.misCensos.filtroInicial = function () { return censo.filtroInicial("misCensos"); };
    return modulos;
}

export function definirPantallasSolicitud(solicitud) {
    var modulos = {
        solicitud: pantalla(
            "Solicitud alojamiento",
            ["Acciones", "N°", "Fecha", "Grupo empresa", "Solicitante", "Estado"],
            solicitud.filtros,
            null,
            { titulo: "Nueva solicitud", boton: "Nueva solicitud" },
            "solicitud"
        )
    };
    modulos.solicitud.filtroInicial = solicitud.filtroInicial;
    return modulos;
}

/** Une definiciones de varios paquetes (escritorio completo). */
export function crearDefinicionPantallas(reserva, censo, solicitud) {
    return Object.assign(
        {},
        definirPantallasReserva(reserva),
        definirPantallasCenso(censo),
        definirPantallasSolicitud(solicitud)
    );
}
