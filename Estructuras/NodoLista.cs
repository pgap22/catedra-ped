namespace ProyectoCatedra.Estructuras
{
    // Clase Nodo para la Lista Enlazada
    public class NodoLista
    {
        public object Valor { get; set; }
        public NodoLista Siguiente { get; set; }

        public NodoLista(object valor)
        {
            Valor = valor;
            Siguiente = null;
        }
    }
}
