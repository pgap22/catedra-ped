namespace ProyectoCatedra.Estructuras
{
    public class TablaHash
    {
        private class NodoHash
        {
            public string Clave { get; set; }
            public object Valor { get; set; }
            public NodoHash? Siguiente { get; set; }

            public NodoHash(string clave, object valor)
            {
                Clave = clave;
                Valor = valor;
                Siguiente = null;
            }
        }

        private NodoHash?[] tabla;
        private int tamaño;

        public TablaHash(int tamañoInicial = 100)
        {
            tamaño = tamañoInicial;
            tabla = new NodoHash[tamaño];
        }

        private int ObtenerIndice(string clave)
        {
            int hash = 0;
            foreach (char c in clave) hash += (int)c;
            return Math.Abs(hash) % tamaño;
        }

        public void Insertar(string clave, object valor)
        {
            int indice = ObtenerIndice(clave);
            NodoHash nuevo = new NodoHash(clave, valor);

            if (tabla[indice] == null)
            {
                tabla[indice] = nuevo;
            }
            else
            {
                NodoHash? actual = tabla[indice];
                while (actual != null)
                {
                    if (actual.Clave == clave) { actual.Valor = valor; return; }
                    if (actual.Siguiente == null) { actual.Siguiente = nuevo; return; }
                    actual = actual.Siguiente;
                }
            }
        }

        public object? Buscar(string clave)
        {
            int indice = ObtenerIndice(clave);
            NodoHash? actual = tabla[indice];
            while (actual != null)
            {
                if (actual.Clave == clave) return actual.Valor;
                actual = actual.Siguiente;
            }
            return null;
        }
    }
}
