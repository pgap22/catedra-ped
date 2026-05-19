using System;

namespace ProyectoCatedra.Estructuras
{
    public class ArbolBST
    {

        private NodoArbol? raiz;

        public void Insertar(string llave, object valor)
        {
            raiz = InsertarRecursivo(raiz, llave, valor);
        }

        private NodoArbol InsertarRecursivo(NodoArbol? actual, string llave, object valor)
        {
            if (actual == null) return new NodoArbol(llave, valor);

            int comparacion = string.Compare(llave, actual.Llave, StringComparison.OrdinalIgnoreCase);

            if (comparacion == 0)
            {
                actual.Valores.Agregar(valor);
            }
            else if (comparacion < 0)
            {
                actual.Izquierdo = InsertarRecursivo(actual.Izquierdo, llave, valor);
            }
            else
            {
                actual.Derecho = InsertarRecursivo(actual.Derecho, llave, valor);
            }

            return actual;
        }

        public ListaEnlazada? Buscar(string llave)
        {
            return BuscarRecursivo(raiz, llave);
        }

        public bool Eliminar(string llave)
        {
            bool eliminado;
            raiz = EliminarNodoPorLlave(raiz, llave, out eliminado);
            return eliminado;
        }

        public bool EliminarValor(string llave, Predicate<object> criterio)
        {
            bool eliminado;
            raiz = EliminarValorRecursivo(raiz, llave, criterio, out eliminado);
            return eliminado;
        }

        public ListaEnlazada BuscarParcial(string fragmento)
        {
            ListaEnlazada resultados = new ListaEnlazada();
            BuscarParcialRecursivo(raiz, fragmento, resultados);
            return resultados;
        }

        private void BuscarParcialRecursivo(NodoArbol? actual, string fragmento, ListaEnlazada resultados)
        {
            if (actual == null) return;

            BuscarParcialRecursivo(actual.Izquierdo, fragmento, resultados);

            if (actual.Llave.IndexOf(fragmento, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                actual.Valores.ParaCada(valor => resultados.Agregar(valor));
            }

            BuscarParcialRecursivo(actual.Derecho, fragmento, resultados);
        }

        public ListaEnlazada ObtenerInOrder()
        {
            var resultado = new ListaEnlazada();
            RecorrerInOrder(raiz, resultado);
            return resultado;
        }

        private ListaEnlazada? BuscarRecursivo(NodoArbol? actual, string llave)
        {
            if (actual == null) return null;

            int comparacion = string.Compare(llave, actual.Llave, StringComparison.OrdinalIgnoreCase);

            if (comparacion == 0) return actual.Valores;

            if (comparacion < 0) return BuscarRecursivo(actual.Izquierdo, llave);

            return BuscarRecursivo(actual.Derecho, llave);
        }

        private void RecorrerInOrder(NodoArbol? actual, ListaEnlazada resultado)
        {
            if (actual == null) return;

            RecorrerInOrder(actual.Izquierdo, resultado);

            actual.Valores.ParaCada(valor =>
            {
                resultado.Agregar(valor);
            });

            RecorrerInOrder(actual.Derecho, resultado);
        }

        private NodoArbol? EliminarValorRecursivo(NodoArbol? actual, string llave, Predicate<object> criterio, out bool eliminado)
        {
            eliminado = false;
            if (actual == null) return null;

            int comparacion = string.Compare(llave, actual.Llave, StringComparison.OrdinalIgnoreCase);

            if (comparacion < 0)
            {
                actual.Izquierdo = EliminarValorRecursivo(actual.Izquierdo, llave, criterio, out eliminado);
                return actual;
            }

            if (comparacion > 0)
            {
                actual.Derecho = EliminarValorRecursivo(actual.Derecho, llave, criterio, out eliminado);
                return actual;
            }

            eliminado = actual.Valores.EliminarPrimero(criterio, out _);
            if (!eliminado) return actual;
            if (!actual.Valores.EstaVacia()) return actual;

            bool dummy;
            return EliminarNodoPorLlave(actual, llave, out dummy);
        }

        private NodoArbol? EliminarNodoPorLlave(NodoArbol? actual, string llave, out bool eliminado)
        {
            eliminado = false;
            if (actual == null) return null;

            int comparacion = string.Compare(llave, actual.Llave, StringComparison.OrdinalIgnoreCase);

            if (comparacion < 0)
            {
                actual.Izquierdo = EliminarNodoPorLlave(actual.Izquierdo, llave, out eliminado);
                return actual;
            }

            if (comparacion > 0)
            {
                actual.Derecho = EliminarNodoPorLlave(actual.Derecho, llave, out eliminado);
                return actual;
            }

            eliminado = true;

            if (actual.Izquierdo == null) return actual.Derecho;
            if (actual.Derecho == null) return actual.Izquierdo;

            NodoArbol sucesor = ObtenerMinimo(actual.Derecho);
            actual.Llave = sucesor.Llave;
            actual.Valores = sucesor.Valores;

            bool eliminadoSucesor;
            actual.Derecho = EliminarNodoPorLlave(actual.Derecho, sucesor.Llave, out eliminadoSucesor);

            return actual;
        }

        private NodoArbol ObtenerMinimo(NodoArbol actual)
        {
            while (actual.Izquierdo != null)
            {
                actual = actual.Izquierdo;
            }

            return actual;
        }
    }
}
