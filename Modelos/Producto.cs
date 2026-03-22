namespace ProyectoCatedra.Modelos
{
    public class Producto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int IdCategoria { get; set; }
        public double Stock { get; set; }
        
        // Propiedad extra para mostrar en la UI sin hacer joins complejos
        public string NombreCategoria { get; set; } = string.Empty;
    }
}
