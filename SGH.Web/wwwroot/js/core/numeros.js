export function numero(valor) {
    var convertido = parseInt(valor, 10);
    return isNaN(convertido) ? 0 : convertido;
}
