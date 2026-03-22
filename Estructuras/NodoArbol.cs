namespace ProyectoCatedra.Estructuras
{
    // Nodo para el Árbol Binario de Búsqueda
    public class NodoArbol
    {
        public string Llave { get; set; } // Nombre del beneficiario para buscar
        public object Valor { get; set; } // Objeto Beneficiario completo
        public NodoArbol Izquierdo { get; set; }
        public NodoArbol Derecho { get; set; }

        public NodoArbol(string llave, object valor)
        {
            Llave = llave;
            Valor = valor;
            Izquierdo = null;
            Derecho = null;
        }
    }
}
