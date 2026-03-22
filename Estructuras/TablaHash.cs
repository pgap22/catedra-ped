namespace ProyectoCatedra.Estructuras
{
    // Nodo para la Tabla Hash (Manejo de colisiones por encadenamiento)
    public class NodoHash
    {
        public string Clave { get; set; }
        public object Valor { get; set; }
        public NodoHash Siguiente { get; set; }

        public NodoHash(string clave, object valor)
        {
            Clave = clave;
            Valor = valor;
            Siguiente = null;
        }
    }

    // Tabla Hash manual para búsqueda instantánea de productos por SKU
    public class TablaHash
    {
        private NodoHash[] tabla;
        private int tamaño;

        public TablaHash(int tamañoInicial = 100)
        {
            tamaño = tamañoInicial;
            tabla = new NodoHash[tamaño];
        }

        // Función Hash simple (Suma de caracteres)
        private int ObtenerIndice(string clave)
        {
            int hash = 0;
            foreach (char c in clave)
            {
                hash += (int)c;
            }
            return hash % tamaño;
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
                // Encadenamiento
                NodoHash actual = tabla[indice];
                while (actual.Siguiente != null)
                {
                    if (actual.Clave == clave) // Si ya existe, actualizamos
                    {
                        actual.Valor = valor;
                        return;
                    }
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevo;
            }
        }

        public object Buscar(string clave)
        {
            int indice = ObtenerIndice(clave);
            NodoHash actual = tabla[indice];

            while (actual != null)
            {
                if (actual.Clave == clave) return actual.Valor;
                actual = actual.Siguiente;
            }
            return null;
        }
    }
}
