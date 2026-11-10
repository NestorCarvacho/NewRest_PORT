namespace SGH.Domain.Cuenta;

public static class MatrizPerfiles
{
    public const int Pasajero = 2;
    public const int OperacionesA = 4;
    public const int OperacionesB = 5;
    public const int IdeaUno = 11;
    public const int GestionTi = 68;
    public const int Configuracion = 142;

    public static bool EsPasajero(int perfilId) => perfilId == Pasajero;

}
