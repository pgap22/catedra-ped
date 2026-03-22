namespace ProyectoCatedra.Estructuras
{
    // Estructura de Pila (LIFO) para el sistema de Deshacer
    public class Pila
    {
        private class NodoPila
        {
            public object Valor { get; set; }
            public NodoPila Siguiente { get; set; }
            public NodoPila(object valor) { Valor = valor; Siguiente = null; }
        }

        private NodoPila tope;
        private int contador;

        public void Empujar(object valor)
        {
            NodoPila nuevo = new NodoPila(valor);
            nuevo.Siguiente = tope;
            tope = nuevo;
            contador++;
        }

        public object Pop()
        {
            if (EstaVacia()) return null;
            object valor = tope.Valor;
            tope = tope.Siguiente;
            contador--;
            return valor;
        }

        public bool EstaVacia() => tope == null;
        public int Conteo() => contador;
    }
}
