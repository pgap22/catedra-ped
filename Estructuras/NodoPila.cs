namespace ProyectoCatedra.Estructuras
{
    public class NodoPila
    {
        public object Valor { get; set; }
        public NodoPila? Siguiente { get; set; }
        public NodoPila(object valor) { Valor = valor; Siguiente = null; }
    }
}
