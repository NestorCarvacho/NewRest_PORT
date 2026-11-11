import { ensamblarEntorno } from "./ensamblar-entorno.js";
import { PANTALLAS_ESCRITORIO } from "./registro-paquetes.js";

/**
 * @param {{ pantallas?: string[] }} [opciones]
 * - Sin opciones: escritorio completo (Index + solapas).
 * - Con `pantallas: ['censo']`: solo paquete censo (página /Censo/Datos).
 */
export function crearEntornoEscritorio(opciones) {
    var ids = opciones && opciones.pantallas ? opciones.pantallas : PANTALLAS_ESCRITORIO;
    return ensamblarEntorno(ids);
}
