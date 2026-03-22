namespace ProyectoCatedra.Modelos
{
    public class Producto
    {
        public int Id { get; set; }
        public string SKU { get; set; }
        public string Nombre { get; set; }
        public int IdCategoria { get; set; }
        public double Stock { get; set; }
        
        // Propiedad extra para mostrar en la UI
        public string NombreCategoria { get; set; }
    }
}
