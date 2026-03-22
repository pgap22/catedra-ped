namespace ProyectoCatedra.Estructuras
{
    public class ListaEnlazada
    {
        private NodoLista? cabeza;
        private int contador;

        public ListaEnlazada()
        {
            cabeza = null;
            contador = 0;
        }

        public void Agregar(object valor)
        {
            NodoLista nuevoNodo = new NodoLista(valor);
            if (cabeza == null)
            {
                cabeza = nuevoNodo;
            }
            else
            {
                NodoLista actual = cabeza;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevoNodo;
            }
            contador++;
        }

        public object? Obtener(int indice)
        {
            if (indice < 0 || indice >= contador || cabeza == null) return null;

            NodoLista actual = cabeza;
            for (int i = 0; i < indice; i++)
            {
                if (actual.Siguiente == null) return null;
                actual = actual.Siguiente;
            }
            return actual.Valor;
        }

        public int Conteo() => contador;

        public void Limpiar()
        {
            cabeza = null;
            contador = 0;
        }
    }
}
