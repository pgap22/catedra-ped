namespace ProyectoCatedra.Estructuras
{
    public class NodoArbol
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
}
