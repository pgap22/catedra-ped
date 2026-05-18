namespace ProyectoCatedra.Modelos
{
    public class OrdenDetalle
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public int BeneficiarioId { get; set; }
        public string NombreBeneficiario { get; set; } = "";
        public int CategoriaId { get; set; }
        public string NombreCategoria { get; set; } = "";
        public double CantidadAsignada { get; set; }
        public double DeficitCalculado { get; set; }
        public string ExplicacionCalculo { get; set; } = "";
        public string SKUProductoSugerido { get; set; } = "";
        public string NombreProductoSugerido { get; set; } = "";
        public string NombreUnidadMedida { get; set; } = "";
    }
}