using System;

namespace ProyectoCatedra.Estructuras
{
    public class ArbolBST
    {
        private class NodoArbol
        {
            public string Llave { get; set; }
            public ListaEnlazada Valores { get; set; }
            public NodoArbol? Izquierdo { get; set; }
            public NodoArbol? Derecho { get; set; }

            public NodoArbol(string llave, object valor)
            {
                Llave = llave;
                Valores = new ListaEnlazada();
                Valores.Agregar(valor);
                Izquierdo = null;
                Derecho = null;
            }
        }

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

        private ListaEnlazada? BuscarRecursivo(NodoArbol? actual, string llave)
        {
            if (actual == null) return null;

            int comparacion = string.Compare(llave, actual.Llave, StringComparison.OrdinalIgnoreCase);

            if (comparacion == 0) return actual.Valores;

            if (comparacion < 0) return BuscarRecursivo(actual.Izquierdo, llave);

            return BuscarRecursivo(actual.Derecho, llave);
        }
    }
}
