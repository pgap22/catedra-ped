namespace ProyectoCatedra.Estructuras
{
    public class ListaEnlazada
    {
        private NodoLista? cabeza;
        private NodoLista? cola;
        private int contador;

        public ListaEnlazada()
        {
            cabeza = null;
            cola = null;
            contador = 0;
        }

        public void Agregar(object valor)
        {
            NodoLista nuevoNodo = new NodoLista(valor);
            if (cabeza == null)
            {
                cabeza = nuevoNodo;
                cola = nuevoNodo;
            }
            else
            {
                cola!.Siguiente = nuevoNodo;
                cola = nuevoNodo;
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

        public bool EstaVacia() => contador == 0;

        public bool EliminarPrimero(Predicate<object> criterio, out object? eliminado)
        {
            eliminado = null;
            if (cabeza == null) return false;

            NodoLista? anterior = null;
            NodoLista? actual = cabeza;

            while (actual != null)
            {
                if (criterio(actual.Valor))
                {
                    eliminado = actual.Valor;

                    if (anterior == null)
                    {
                        cabeza = actual.Siguiente;
                    }
                    else
                    {
                        anterior.Siguiente = actual.Siguiente;
                    }

                    if (actual == cola)
                    {
                        cola = anterior;
                    }

                    contador--;
                    if (contador == 0)
                    {
                        cabeza = null;
                        cola = null;
                    }

                    return true;
                }

                anterior = actual;
                actual = actual.Siguiente;
            }

            return false;
        }

        public void ParaCada(Action<object> accion)
        {
            NodoLista? actual = cabeza;
            while (actual != null)
            {
                accion(actual.Valor);
                actual = actual.Siguiente;
            }
        }

        public void Limpiar()
        {
            cabeza = null;
            cola = null;
            contador = 0;
        }
    }
}
