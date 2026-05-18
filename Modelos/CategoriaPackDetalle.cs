namespace ProyectoCatedra.Modelos
{
    public class CategoriaPackDetalle
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public int ProductoId { get; set; }
        public string SKUProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public double Porcentaje { get; set; }
        public double StockDisponible { get; set; }
        public double? MaximoPorEntrega { get; set; }
        public int? DiasReposicion { get; set; }
    }
}
