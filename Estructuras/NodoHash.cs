namespace ProyectoCatedra.Estructuras
{
    public class NodoHash
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
}
